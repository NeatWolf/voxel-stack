using UnityEngine;
using System.Collections;

namespace EzyVoxel {
    public abstract class HexaLink {
        protected int local_posx = 0;
        protected int local_posy = 0;
        protected int local_posz = 0;

        private VoxelWorld _world = null;

        public void Attach(VoxelWorld world, int posx, int posy, int posz) {
            this.local_posx = posx;
            this.local_posy = posy;
            this.local_posz = posz;
            this._world = world;
        }

        public void Detach() {
            local_posx = 0;
            local_posy = 0;
            local_posz = 0;

            _world = null;
        }

        public VoxelChunk PosY {
            get {
                return IsAttached ? _world[LocalPosX, LocalPosY + 1, LocalPosZ] : null;
            }
        }

        public VoxelChunk NegY {
            get {
                return IsAttached ? _world[LocalPosX, LocalPosY - 1, LocalPosZ] : null;
            }
        }

        public VoxelChunk PosX {
            get {
                return IsAttached ? _world[LocalPosX + 1, LocalPosY, LocalPosZ] : null;
            }
        }

        public VoxelChunk NegX {
            get {
                return IsAttached ? _world[LocalPosX - 1, LocalPosY, LocalPosZ] : null;
            }
        }

        public VoxelChunk PosZ {
            get {
                return IsAttached ? _world[LocalPosX, LocalPosY, LocalPosZ + 1] : null;
            }
        }

        public VoxelChunk NegZ {
            get {
                return IsAttached ? _world[LocalPosX, LocalPosY, LocalPosZ - 1] : null;
            }
        }

        public int LocalPosX {
            get {
                return local_posx;
            }
        }

        public int LocalPosY {
            get {
                return local_posy;
            }
        }

        public int LocalPosZ {
            get {
                return local_posz;
            }
        }

        public int WorldPosX {
            get {
                return local_posx * VoxelChunk.DIM_X;
            }
        }

        public int WorldPosY {
            get {
                return local_posy * VoxelChunk.DIM_Y;
            }
        }

        public int WorldPosZ {
            get {
                return local_posz * VoxelChunk.DIM_Z;
            }
        }

        public bool IsAttached {
            get {
                return _world != null;
            }
        }

        public abstract short this[int x, int y, int z] { get; set; }
        public abstract short[] RawVoxelRef { get; }

        /**
         * Maps an x,y,z coordinate to a 1D array index which is how
         * data is stored in the chunk. Function returns -1 if the data
         * falls out of range from the chunk.
         */
        public static int CalculateLocalIndex(int x, int y, int z) {
            if (x > VoxelChunk.DIM_X_INDEX ||
                y > VoxelChunk.DIM_Y_INDEX ||
                z > VoxelChunk.DIM_Z_INDEX ||
                x < 0 ||
                y < 0 ||
                z < 0) 
            {
                return -1;
            }

            return x + VoxelChunk.DIM_X * (y + VoxelChunk.DIM_Z * z);
        }
    }
}
