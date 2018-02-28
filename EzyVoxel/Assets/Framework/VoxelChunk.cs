using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelLUT;

namespace EzyVoxel {
    public sealed class VoxelChunk {
        public const int DIM_X = 10;
        public const int DIM_Y = 10;
        public const int DIM_Z = 10;

        public const int VOXEL_COUNT = DIM_X * DIM_Y * DIM_Z;
        public const int VOXEL_INDEX = VOXEL_COUNT - 1;

        public static readonly Vector3[] _VERTICES;
        public static readonly Vector3[] _NORMALS;
        public static readonly Vector2[] _UVS;

        static VoxelChunk() {
            int size = DIM_X * DIM_Y * DIM_Z * Block.SIZE;

            _VERTICES = new Vector3[size];
            _NORMALS = new Vector3[size];
            _UVS = new Vector2[size];

            for (int x = 0; x < DIM_X; x++) {
                for (int y = 0; y < DIM_Y; y++) {
                    for (int z = 0; z < DIM_Z; z++) {
                        Block.FillVertex(new Vector3(x, y, z), ref _VERTICES, Block.SIZE * CalculateIndex(x, y, z));
                        Block.FillNormal(ref _NORMALS, Block.SIZE * CalculateIndex(x, y, z));
                    }
                }
            }
        }

        private readonly short[] voxels;
        private readonly List<int> triangles;
        private bool isDirty;

        public VoxelChunk() {
            this.voxels = new short[VOXEL_COUNT];
            this.isDirty = true;
            this.triangles = new List<int>();
        }

        public short this[int index] {
            get {
                if (index < 0 || index > VOXEL_INDEX) {
                    return 0;
                }

                return voxels[index];
            }
            set {
                short voxelVal = voxels[index];

                if (voxelVal != value) {
                    voxels[index] = value;

                    isDirty = true;
                }
            }
        }

        public short this[int x, int y, int z] {
            get {
                return this[CalculateIndex(x, y, z)];
            }
            set {
                this[CalculateIndex(x, y, z)] = value;
            }
        }

        public int Hash(int x, int y, int z) {
            int curr = this[x, y, z];

            if (curr == 0) {
                return Block_xxxxxx.Hash;
            }

            int up = this[x, y + 1, z];
            int down = this[x, y - 1, z];
            int left = this[x - 1, y, z];
            int right = this[x + 1, y, z];
            int front = this[x, y, z - 1];
            int back = this[x, y, z + 1];

            int hash = 0;

            hash = hash | (((~up    & (~up      + 1)) >> 31) & 1 << 0);
            hash = hash | (((~down  & (~down    + 1)) >> 31) & 1 << 1);
            hash = hash | (((~left  & (~left    + 1)) >> 31) & 1 << 2);
            hash = hash | (((~right & (~right   + 1)) >> 31) & 1 << 3);
            hash = hash | (((~front & (~front   + 1)) >> 31) & 1 << 4);
            hash = hash | (((~back  & (~back    + 1)) >> 31) & 1 << 5);

            return hash;
        }

        public static int CalculateIndex(int x, int y, int z) {
            return x + DIM_X * (y + DIM_Y * z);
        }

        public bool IsDirty {
            get {
                return isDirty;
            }
        }

        public void Fill(short data) {
            for (int i = 0; i < VOXEL_COUNT; i++) {
                voxels[i] = data;
            }
        }

        public int[] ComputeTriangles() {
            for (int x = 0; x < DIM_X; x++) {
                for (int y = 0; y < DIM_Y; y++) {
                    for (int z = 0; z < DIM_Z; z++) {
                        BlockLUT.Get(Hash(x, y, z)).FillTriangles(triangles, Block.SIZE * CalculateIndex(x, y, z));
                    }
                }
            }

            int[] triArray = triangles.ToArray();
            triangles.Clear();

            return triArray;
        }

        public void Clear() {
            isDirty = false;
        }
    }
}
