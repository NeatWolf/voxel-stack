using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EzyVoxel {
    public sealed class VoxelChunk {
        public const int DIM_X = 16;
        public const int DIM_Y = 16;
        public const int DIM_Z = 16;

        public const int VOXEL_COUNT = DIM_X * DIM_Y * DIM_Z;

        private readonly short[] voxels;

        public VoxelChunk() {
            this.voxels = new short[VOXEL_COUNT];
        }

        public short this[int index] {
            get {
                return voxels[index];
            }
        }

        public short this[int x, int y, int z] {
            get {
                return voxels[x + DIM_X * (y + DIM_Y * z)];
            }
        }
    }
}
