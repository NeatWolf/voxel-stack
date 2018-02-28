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
        public Material defaultMat;
        
        // Use this for initialization
        void Start() {
            _renderer = gameObject.GetComponent<MeshRenderer>();

            if (_renderer == null) {
                _renderer = gameObject.AddComponent<MeshRenderer>();
            }

            _renderer.material = defaultMat;

            _filter = gameObject.GetComponent<MeshFilter>();

            if (_filter == null) {
                _filter = gameObject.AddComponent<MeshFilter>();
            }

            _mesh = new Mesh();
            _mesh.MarkDynamic();
            _mesh.vertices = VoxelChunk._VERTICES;
            _mesh.normals = VoxelChunk._NORMALS;
            _mesh.uv = VoxelChunk._UVS;

            _filter.mesh = _mesh;

             _chunk.Fill(1);

            _chunk[0, 0, 0] = 0;
            _chunk[9, 0, 0] = 0;
            _chunk[0, 9, 0] = 0;
            _chunk[0, 0, 9] = 0;
            _chunk[9, 0, 9] = 0;
            _chunk[0, 9, 9] = 0;
            _chunk[9, 9, 0] = 0;
            _chunk[9, 9, 9] = 0;


            _chunk[5, 5, 5] = 0;
            _chunk[6, 5, 5] = 0;
            _chunk[4, 5, 5] = 0;
            _chunk[5, 6, 5] = 0;
            _chunk[5, 4, 5] = 0;
            _chunk[5, 5, 6] = 0;
            _chunk[5, 5, 4] = 0;


            //_chunk[4, 4, 4] = 1;
        }

        // Update is called once per frame
        void Update() {
            if (_chunk.IsDirty) {
                _mesh.triangles = _chunk.ComputeTriangles();

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
