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
        public const int DIM_X = 13;
        public const int DIM_Y = 13;
        public const int DIM_Z = 13;
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
         * Provided a raw/encoded voxel value, compute it's hash value
         * and return it. Hash values occupy 6 bits of information 
         * in total
         */
        public static int HashValue(short voxelData) {
            return (voxelData & VOXEL_HEXA_MASK) >> 10;
        }

        /**
         * Provided a raw/encoded voxel value, compute it's voxel value
         * and return it. Voxel values occpy 10 bits of information
         * in total
         */
        public static int VoxelValue(short voxelData) {
            return voxelData & VOXEL_VALUE_MAX;
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

					if (incomingValue > 0) {
						if (negX > -1) {
							short val = voxels[negX];
							voxels[negX] = (short)(val | 1 << 10);
						}

						if (posX > -1) {
							short val = voxels[posX];
							voxels[posX] = (short)(val | 1 << 11);
						}

						if (negY > -1) {
							short val = voxels[negY];
							voxels[negY] = (short)(val | 1 << 12);
						}

						if (posY > -1) {
							short val = voxels[posY];
							voxels[posY] = (short)(val | 1 << 13);
						}

						if (negZ > -1) {
							short val = voxels[negZ];
							voxels[negZ] = (short)(val | 1 << 14);
						}

						if (posZ > -1) {
							short val = voxels[posZ];
							voxels[posZ] = (short)(val | 1 << 15);
						}
					}
					else {
						if (negX > -1) {
                            short val = voxels[negX];
                            voxels[negX] = (short)(val & ~(1 << 10));
                        }

                        if (posX > -1) {
                            short val = voxels[posX];
                            voxels[posX] = (short)(val & ~(1 << 11));
                        }

                        if (negY > -1) {
                            short val = voxels[negY];
                            voxels[negY] = (short)(val & ~(1 << 12));
                        }

                        if (posY > -1) {
                            short val = voxels[posY];
                            voxels[posY] = (short)(val & ~(1 << 13));
                        }

                        if (negZ > -1) {
                            short val = voxels[negZ];
                            voxels[negZ] = (short)(val & ~(1 << 14));
                        }

                        if (posZ > -1) {
                            short val = voxels[posZ];
                            voxels[posZ] = (short)(val & ~(1 << 15));
                        }
					}
                }

                // otherwise the incoming value was either 0 being unset or
                // the block was already set and was being changed, in which case we don't
                // need to do anything to the neighbours
                voxels[index] = (short)(incomingValue | (currentData & VOXEL_HEXA_MASK));

                isDirty = true;
            }
        }

        /**
         * Grabs the Hash value of the provided index coordinates
         * which can be used to perform a Voxel Lookup from one of the global
         * Lookup Tables for rendering purposes.
         */
        public int HashValue(int x, int y, int z) {
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
                        BlockLUT.Get(HashValue(x, y, z)).FillTriangles(triangles, Block.SIZE * CalculateLocalIndex(x, y, z));
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

        /**
         * Performs a raycast against the voxel structure in as efficient manner as
         * possible. Will return the index of the voxel which was first hit and was non
         * zero voxel. Function will return -1 if no voxels were hit.
         */
        public int PickVoxel(Ray ray, float distance, ref Vector3 voxel_position) {
            Vector3 origin = ray.origin;
            Vector3 dir = ray.direction;

            float x1 = origin.x - WorldPosX;
            float y1 = origin.y - WorldPosY;
            float z1 = origin.z - WorldPosZ;

            float x2 = (origin.x + dir.x * distance) - WorldPosX;
            float y2 = (origin.y + dir.y * distance) - WorldPosY;
            float z2 = (origin.z + dir.z * distance) - WorldPosZ;

            int i = Mathf.FloorToInt(x1 / 1);
            int j = Mathf.FloorToInt(y1 / 1);
            int k = Mathf.FloorToInt(z1 / 1);

            /*
            int i = Mathf.FloorToInt(x1 / DIM_X);
            int j = Mathf.FloorToInt(y1 / DIM_Y);
            int k = Mathf.FloorToInt(z1 / DIM_Z);
            */

            int iend = Mathf.FloorToInt(x2 / 1);
            int jend = Mathf.FloorToInt(y2 / 1);
            int kend = Mathf.FloorToInt(z2 / 1);

            /*
            int iend = Mathf.FloorToInt(x2 / DIM_X);
            int jend = Mathf.FloorToInt(y2 / DIM_Y);
            int kend = Mathf.FloorToInt(z2 / DIM_Z);
            */

            int di = ((x1 < x2) ? 1 : ((x1 > x2) ? -1 : 0));
            int dj = ((y1 < y2) ? 1 : ((y1 > y2) ? -1 : 0));
            int dk = ((z1 < z2) ? 1 : ((z1 > z2) ? -1 : 0));

            /*
            float minx = DIM_X * Mathf.Floor(x1 / DIM_X);
            float maxx = minx + DIM_X;
            */
            float minx = 1 * Mathf.Floor(x1 / 1);
            float maxx = minx + 1;
            float tx = ((x1 > x2) ? (x1 - minx) : (maxx - x1)) / Mathf.Abs(x2 - x1);
            /*
            float miny = DIM_Y * Mathf.Floor(y1 / DIM_Y);
            float maxy = miny + DIM_Y;
            */
            float miny = 1 * Mathf.Floor(y1 / 1);
            float maxy = miny + 1;
            float ty = ((y1 > y2) ? (y1 - miny) : (maxy - y1)) / Mathf.Abs(y2 - y1);
            /*
            float minz = DIM_Z * Mathf.Floor(z1 / DIM_Z);
            float maxz = minz + DIM_Z;
            */
            float minz = 1 * Mathf.Floor(z1 / 1);
            float maxz = minz + 1;
            float tz = ((z1 > z2) ? (z1 - minz) : (maxz - z1)) / Mathf.Abs(z2 - z1);

            /*
            float deltatx = DIM_X / Mathf.Abs(x2 - x1);
            float deltaty = DIM_Y / Mathf.Abs(y2 - y1);
            float deltatz = DIM_Z / Mathf.Abs(z2 - z1);
            */

            float deltatx = 1 / Mathf.Abs(x2 - x1);
            float deltaty = 1 / Mathf.Abs(y2 - y1);
            float deltatz = 1 / Mathf.Abs(z2 - z1);


            while (true) {
                int index = CalculateLocalIndex(i, j, k);

                int val = VoxelValue(this[index]);

                if (val > 0) {
                    voxel_position.x = i + WorldPosX;
                    voxel_position.y = j + WorldPosY;
                    voxel_position.z = k + WorldPosZ;

                    return index;
                }

                if (tx <= ty && tx <= tz) {
                    if (i == iend) {
                        return -1;
                    }

                    tx += deltatx;
                    i += di;
                }
                else if (ty <= tx && ty <= tz) {
                    if (j == jend) {
                        return -1;
                    }

                    ty += deltaty;
                    j += dj;
                }
                else {
                    if (k == kend) {
                        return -1;
                    }

                    tz += deltatz;
                    k += dk;
                }
            }
        }

        /**
         * Override to print the current state of all voxels
         */
        public override string ToString() {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            for (int x = 0; x < DIM_X; x++) {
                for (int y = 0; y < DIM_Y; y++) {
                    for (int z = 0; z < DIM_Z; z++) {
                        short voxel = voxels[CalculateLocalIndex(x, y, z)];

                        int v = voxel & VOXEL_VALUE_MAX;
                        int n = (voxel & VOXEL_HEXA_MASK) >> 10;

                        int vox = v | (n << 10);

                        builder.AppendLine("[" + x + "," + y + "," + z + "] V = " + BitUtil.GetBitStringShort(v) + " N = " + BitUtil.GetBitStringShort(n) + " VOX = " + BitUtil.GetBitStringShort(vox));
                    }
                }
            }

            return builder.ToString();
        }
    }
}
