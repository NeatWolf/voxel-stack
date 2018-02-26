using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VoxelLUT;

namespace EzyVoxel {
    public sealed class VoxelChunkRenderer : MonoBehaviour {

        private readonly VoxelChunk _chunk = new VoxelChunk();
        private Mesh _mesh;
        private MeshRenderer _renderer;
        private MeshFilter _filter;
        
        // Use this for initialization
        void Start() {
            _renderer = gameObject.GetComponent<MeshRenderer>();

            if (_renderer == null) {
                _renderer = gameObject.AddComponent<MeshRenderer>();
            }

            _filter = gameObject.GetComponent<MeshFilter>();

            if (_filter == null) {
                _filter = gameObject.AddComponent<MeshFilter>();
            }

            _mesh = new Mesh();
            _mesh.MarkDynamic();
            _mesh.vertices = VoxelChunk._VERTICES;
            _mesh.uv = VoxelChunk._UVS;

            _filter.mesh = _mesh;

            _chunk[5, 5, 5] = 1;
            _chunk[2, 2, 2] = 1;
        }

        // Update is called once per frame
        void Update() {
            if (_chunk.IsDirty) {
                List<int> _tris = new List<int>();

                for (int x = 0; x < VoxelChunk.DIM_X; x++) {
                    for (int y = 0; y < VoxelChunk.DIM_Y; y++) {
                        for (int z = 0; z < VoxelChunk.DIM_Z; z++) {
                            BlockLUT.Get(_chunk.Hash(x, y, z)).FillTriangles(_tris, Block.SIZE * VoxelChunk.CalculateIndex(x, y, z));
                        }
                    }
                }

                _mesh.triangles = _tris.ToArray();
                _mesh.RecalculateNormals();

                _chunk.Clear();
            }
        }

        public VoxelChunk Chunk {
            get {
                return _chunk;
            }
        }
    }
}
