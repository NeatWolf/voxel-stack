using UnityEngine;
using System.Collections;
using VoxelStack;

#if UNITY_EDITOR || DEBUG
namespace VoxelStackDebug {
	public class VoxelChunkRenderer : MonoBehaviour {
	
		public Material material;
		public bool drawDebug = false;
		
		private VoxelChunk chunk;
	
		void Start() {
			MeshRenderer rend = gameObject.GetComponent<MeshRenderer>();
			
			if (rend == null) {
				rend = gameObject.AddComponent<MeshRenderer>();
			}
			
			rend.material = material;
			
			MeshFilter filter = gameObject.GetComponent<MeshFilter>();
			
			if (filter == null) {
				filter = gameObject.AddComponent<MeshFilter>();
			}
			
			if (filter.sharedMesh == null) {
				filter.sharedMesh = new Mesh();
			}
			
			RenderChunk(filter.sharedMesh);
		}
		
		void RenderChunk(Mesh mesh) {
			if (chunk == null) {
				chunk = new VoxelChunk();
			}
			
			chunk[2,2,2] = new Voxel(1, Voxel.STATE_MAX - 5);
			
			chunk.FillMesh(mesh);
		}

		void OnDrawGizmos() {
			if (!drawDebug) {
				return;
			}
			
			MeshFilter filter = gameObject.GetComponent<MeshFilter>();
			
			if (filter == null) {
				return;
			}
			
			Mesh sharedMesh = filter.sharedMesh;
			
			if (sharedMesh == null) {
				return;
			}
			
			Vector3[] vertices = sharedMesh.vertices;
			
			Gizmos.color = Color.red;
			
			for (int i = 0; i < vertices.Length; i++) {
				Gizmos.DrawCube(vertices[i], new Vector3(0.01f, 0.01f, 0.01f));
			}
			
			int[] triangles = sharedMesh.triangles;
			
			Gizmos.color = Color.green;
			
			for (int i = 0; i < triangles.Length; i+=3) {
				Gizmos.DrawLine(vertices[triangles[i]], vertices[triangles[i+1]]);
				Gizmos.DrawLine(vertices[triangles[i+1]], vertices[triangles[i+2]]);
				Gizmos.DrawLine(vertices[triangles[i+2]], vertices[triangles[i]]);
				
				Gizmos.DrawWireCube(vertices[triangles[i]], new Vector3(0.011f, 0.011f, 0.011f));
				Gizmos.DrawWireCube(vertices[triangles[i+1]], new Vector3(0.011f, 0.011f, 0.011f));
				Gizmos.DrawWireCube(vertices[triangles[i+2]], new Vector3(0.011f, 0.011f, 0.011f));
			}
		}
	}
}
#endif
