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

        public Vector3 voxel_position = new Vector3();
        public int last_rayindex = -1;

        public List<Vector3> pos = new List<Vector3>();
        
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
            _mesh.vertices = VoxelChunk._VERTICES;
            _mesh.normals = VoxelChunk._NORMALS;
            _mesh.uv = VoxelChunk._UVS;

            _filter.mesh = _mesh;

             _chunk.Fill(1023);
        }

        // Update is called once per frame
        void Update() {
            UpdateRenderer();

            if (Camera.current == null) {
                return;
            }

            Ray ray = Camera.current.ScreenPointToRay(Input.mousePosition);

            int index = _chunk.PickVoxel(ray, 1000.0f, ref voxel_position);

            if (Input.GetButtonDown("Fire1")) {
                _chunk[(int)voxel_position.x, (int)voxel_position.y, (int)voxel_position.z] = 0;
                pos.Add(new Vector3(voxel_position.x, voxel_position.y, voxel_position.z));
            }

            if (index != last_rayindex) {
                Debug.Log(BitUtil.GetBitStringShort(VoxelChunk.HashValue(_chunk[index])));

                last_rayindex = index;
            }
        }

        public void UpdateRenderer() {
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

        public void OnDrawGizmos() {
            /*for (int i = 0; i < pos.Count; i++) {
                Block.OnDebug(pos[i]);
            }*/

            Block.OnDebug(voxel_position);
        }
    }
}
