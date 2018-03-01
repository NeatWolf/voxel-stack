using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelLUT;

namespace EzyVoxel {
    /**
     * Represents a single Voxel Chunk with DIM_X DIM_Y and DIM_Z
     * in x,y,z directions. The Chunk is a volume of voxels which manages
     * setting/unsetting and calculating hash indices for rendering purposes.
     */
    public sealed class VoxelChunk : HexaLink {
        public const int DIM_X = 10;
        public const int DIM_Y = 10;
        public const int DIM_Z = 10;
        public const int DIM_X_INDEX = DIM_X - 1;
        public const int DIM_Y_INDEX = DIM_Y - 1;
        public const int DIM_Z_INDEX = DIM_Z - 1;

        public const int VOXEL_COUNT = DIM_X * DIM_Y * DIM_Z;
        public const int VOXEL_INDEX = VOXEL_COUNT - 1;

        /**
         * Represents the masking hex value for the VALUE section of the
         * Voxel which can store 10 bits of data giving an unsigned range
         * of 0 -> 1024. This allows having a maximum of 1024 different voxel types.
         */
        public const int VOXEL_VALUE_MASK = 0xFFC0;
        public const int VOXEL_VALUE_MAX = VOXEL_VALUE_MASK >> 6;

        /**
         * Represents the masking hex value for the neighbouring data of
         * a voxel. The 6 bits represent the 6 neighbours which are taken into
         * account. When a bit is set, the neighbour is active, otherwise it's inactive
         */
        public const int VOXEL_HEXA_MAX = 0x3F;
        public const int VOXEL_HEXA_MASK = VOXEL_HEXA_MAX << 10;

        /**
         * Precomputed Vertex, Normals and UV's whcih is shared and used across
         * all voxels for rendering purposes.
         */
        public static readonly Vector3[] _VERTICES;
        public static readonly Vector3[] _NORMALS;
        public static readonly Vector2[] _UVS;

        /**
         * Statically generate all Vertex and Normal combinations for all voxels
         * which is designed to fit inside a single Chunk mesh. The Vertex, Normals and
         * UV combinations do not change during rendering, only the triangle order.
         * 
         * The Static init is only called once per application context.
         */
        static VoxelChunk() {
            int size = DIM_X * DIM_Y * DIM_Z * Block.SIZE;

            _VERTICES = new Vector3[size];
            _NORMALS = new Vector3[size];
            _UVS = new Vector2[size];

            for (int x = 0; x < DIM_X; x++) {
                for (int y = 0; y < DIM_Y; y++) {
                    for (int z = 0; z < DIM_Z; z++) {
                        Block.FillVertex(new Vector3(x, y, z), ref _VERTICES, Block.SIZE * CalculateLocalIndex(x, y, z));
                        Block.FillNormal(ref _NORMALS, Block.SIZE * CalculateLocalIndex(x, y, z));
                    }
                }
            }
        }

        /**
         * Represents Voxel data in 16 bit format. 6 bits are used
         * to track and update the state of the neighbour voxels and
         * 10 bits is used to track the current state of the voxel data.
         */
        private readonly short[] voxels;

        /**
         * Temporary variable to fill in the triangles data. There could be
         * a better way to do this without having to generate so much array
         * garbage. Unfortunately the length of the triangles can vary on
         * a per voxel basis.
         */
        private readonly List<int> triangles;

        /**
         * When a vixel gets updated, it's set to dirty mode so the renderer
         * knows when to update it's render state.
         */
        private bool isDirty;

        public VoxelChunk() {
            this.voxels = new short[VOXEL_COUNT];
            this.isDirty = true;
            this.triangles = new List<int>();
        }

        /**
         * This function is primarily used by the voxel system for quick
         * access of data between subsystems without having to go through expensive
         * checks or the like. This returns the voxel data as a full 2^16 range (short)
         * per voxel, which contains the encoded state of each voxel.
         * 
         * Modifying the state of the voxels
         */
        public override short[] RawVoxelRef {
            get {
                return voxels;
            }
        }

        /**
         * Grab the state of the voxel from a single 1D index.
         * Use CalculateIndex(x,y,z) to generate an index from
         * 3D coordinates. Or use [x,y,z] accessor which does this
         * for you.
         */
        public short this[int index] {
            get {
                // if we fall out of range, return a zero index
                // or a zero voxel
                if (index < 0 || index > VOXEL_INDEX) {
                    return 0;
                }

                // otherwise return the voxel at our requested index
                return voxels[index];
            }
        }

        /**
         * Returns the state of the voxel in the provided coordinates.
         * Returns a zero state (0) if the coordinates are out of range
         * 
         * NOTE on the set function
         * 
         * - We use the set to also calculate the neighbouring states related to
         * this particular voxel. It is much better for performance to do this on a per
         * set basis instead of re-computing the LUT hash values each time the chunk changes.
         */
        public override short this[int x, int y, int z] {
            get {
                return this[CalculateLocalIndex(x, y, z)];
            }
            set {
                // the set method also calculates all neighbouring
                // voxel bits to ensure all neighbour relationships are
                // set properly.
                int index = CalculateLocalIndex(x, y, z);

                // out of bounds set, we don't need to do anything
                if (index < 0 || index > VOXEL_INDEX) {
                    return;
                }

                short currentData = voxels[index];

                // first we take a look at the VALUE section of the data
                // ensure we only look into the 2^10 (10 bits) which is the
                // maximum we can store + 6 bits for neighbouring states
                int currentValue = currentData & VOXEL_VALUE_MAX;
                int incomingValue = value & VOXEL_VALUE_MAX;

                // we just switched from going 0 to 1 (block being set)
                // or we just switched from going 1 to 0 (block being unset)
                if ((currentValue == 0 && incomingValue > 0) || 
                    (currentValue > 0 && incomingValue == 0)) 
                {
                    // we need to update all neighbour voxels around this
                    // particular voxel to ensure they know the current state
                    // the rendering context will use the state to perform a LUT
                    // operation for rendering the surrounding voxels

                    // bit index 10
                    int posX = CalculateLocalIndex(x + 1, y, z);
                    // bit index 11
                    int negX = CalculateLocalIndex(x - 1, y, z);
                    // bit index 12
                    int posY = CalculateLocalIndex(x, y + 1, z);
                    // bit index 13
                    int negY = CalculateLocalIndex(x, y - 1, z);
                    // bit index 14
                    int posZ = CalculateLocalIndex(x, y, z + 1);
                    // bit index 15
                    int negZ = CalculateLocalIndex(x, y, z - 1);

                    if (posX > -1) {
                        short val = voxels[posX];
                        voxels[posX] = (short)(incomingValue > 0 ? (val | 1 << 11) : (val & ~(1 << 11)));
                        //Debug.Log("posX = " + x + " " + y + " " + z + " - " + ToBitString(voxels[posX]));
                    }

                    if (negX > -1) {
                        short val = voxels[negX];
                        voxels[negX] = (short)(incomingValue > 0 ? (val | 1 << 10) : (val & ~(1 << 10)));
                        //Debug.Log("negX = " + x + " " + y + " " + z + " - " + ToBitString(voxels[negX]));
                    }

                    if (posY > -1) {
                        short val = voxels[posY];
                        voxels[posY] = (short)(incomingValue > 0 ? (val | 1 << 13) : (val & ~(1 << 13)));
                        //Debug.Log("posY = " + x + " " + y + " " + z + " - " + ToBitString(voxels[posY]));
                    }

                    if (negY > -1) {
                        short val = voxels[negY];
                        voxels[negY] = (short)(incomingValue > 0 ? (val | 1 << 12) : (val & ~(1 << 12)));
                        //Debug.Log("negY = " + x + " " + y + " " + z + " - " + ToBitString(voxels[negY]));
                    }

                    if (posZ > -1) {
                        short val = voxels[posZ];
                        voxels[posZ] = (short)(incomingValue > 0 ? (val | 1 << 15) : (val & ~(1 << 15)));
                        //Debug.Log("posZ = " + x + " " + y + " " + z + " - " + ToBitString(voxels[posZ]));
                    }

                    if (negZ > -1) {
                        short val = voxels[negZ];
                        voxels[negZ] = (short)(incomingValue > 0 ? (val | 1 << 14) : (val & ~(1 << 14)));
                        //Debug.Log("negZ = " + x + " " + y + " " + z + " - " + ToBitString(voxels[negZ]));
                    }
                }

                // otherwise the incoming value was either 0 being unset or
                // the block was already set and was being changed, in which case we don't
                // need to do anything to the neighbours
                voxels[index] = (short)(incomingValue | (currentData & VOXEL_HEXA_MASK));
            }
        }

        /**
         * Grabs the Hash value of the provided index coordinates
         * which can be used to perform a Voxel Lookup from one of the global
         * Lookup Tables for rendering purposes.
         */
        public int Hash(int x, int y, int z) {
            short curr = this[x, y, z];

            // our voxel is zero, we don't need to render anything
            // send over the zero state hash (0x0) from Block_xxxxxx
            if ((curr & VOXEL_VALUE_MAX) == 0) {
                return Block_xxxxxx.Hash;
            }

            // otherwise return our hash value, discarding
            // the actual value of the voxel which is 10 bits
            return (curr & VOXEL_HEXA_MASK) >> 10;
        }

        /**
         * This flag is set if any of the voxels get updated before
         * a Clear() function is called. Marks that this chunk may require
         * synchronization with the rendering context
         */
        public bool IsDirty {
            get {
                return isDirty;
            }
        }

        /**
         * Set the state of this chunk to be dirty, which will force
         * a re-sinchronization of the systems which rely on the IsDirty
         * flag. This is normally set by subsystems directly when modifying
         * internal data.
         * 
         * The cost of setting this flag depends on the runtime complexity
         * of systems which require Synchronization, such as updating Mesh data
         * for rendering contexts.
         */
        public void SetDirty() {
            isDirty = true;
        }

        /**
         * Fill our chunk with the requested data. Note that the maximum
         * data which can be stored is 2^10 = 1024 different combinations.
         */
        public void Fill(short data) {
            for (int x = 0; x < DIM_X; x++) {
                for (int y = 0; y < DIM_Y; y++) {
                    for (int z = 0; z < DIM_Z; z++) {
                        this[x, y, z] = data;
                    }
                }
            }

            isDirty = true;
        }

        /**
         * Compute our triangle indices depending on the state of our current chunk
         */
        public int[] ComputeTriangles() {
            for (int x = 0; x < DIM_X; x++) {
                for (int y = 0; y < DIM_Y; y++) {
                    for (int z = 0; z < DIM_Z; z++) {
                        BlockLUT.Get(Hash(x, y, z)).FillTriangles(triangles, Block.SIZE * CalculateLocalIndex(x, y, z));
                    }
                }
            }

            int[] triArray = triangles.ToArray();
            triangles.Clear();

            return triArray;
        }

        /**
         * Force a clear of our Dirty flag, which denotes that this voxel chunk
         * is now synchronized with the rendering state.
         */
        public void Clear() {
            isDirty = false;
        }

        public override string ToString() {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            for (int x = 0; x < DIM_X; x++) {
                for (int y = 0; y < DIM_Y; y++) {
                    for (int z = 0; z < DIM_Z; z++) {
                        short voxel = voxels[CalculateLocalIndex(x, y, z)];

                        int v = voxel & VOXEL_VALUE_MAX;
                        int n = (voxel & VOXEL_HEXA_MASK) >> 10;

                        int vox = v | (n << 10);

                        builder.AppendLine("[" + x + "," + y + "," + z + "] V = " + ToBitString(v) + " N = " + ToBitString(n) + " VOX = " + ToBitString(vox));
                    }
                }
            }

            return builder.ToString();
        }

        public static string ToBitString(int data) {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            for (int i = 0; i < 16; i++) {
                builder.Append(GetBit(data, i) ? 1 : 0);
            }

            return builder.ToString();
        }

        public static bool GetBit(int data, int pos) {
            return ((data >> pos) & 1) == 1;
        }
    }
}
