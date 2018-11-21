using UnityEngine;
using System.Collections;
using VoxelStack;

#if UNITY_EDITOR || DEBUG
namespace VoxelStackDebug {
	public enum ChunkStyle : ulong {
		ZERO = 0,
		BOX = SubVoxel.BOX,
		BOX_HALF = SubVoxel.BOX_HALF,
		BOX_HALLOW = SubVoxel.BOX_HALLOW,
		BOX_ODD_FILLER = SubVoxel.BOX,
		BOX_ODD_EDGE = SubVoxel.BOX_ODD_FILLER,
		BOX_NO_CORNERS = SubVoxel.BOX_NO_CORNERS,
		BOX_ONLY_CORNERS = SubVoxel.BOX_ONLY_CORNERS,
		DIAGONAL = SubVoxel.DIAGONAL,
		BOX_BOTTOM_HALF = SubVoxel.BOX_BOTTOM_HALF,
		BOX_TOP_HALF = SubVoxel.BOX_TOP_HALF,
		BOX_BOTTOM_QUARTER = SubVoxel.BOX_BOTTOM_QUARTER,
		BOX_TOP_QUARTER = SubVoxel.BOX_TOP_QUARTER
	}
	
	public class VoxelChunkRenderer : MonoBehaviour {
	
		public Material material;
		public bool drawDebug = false;
		
		VoxelChunk chunk;
		
		public ulong[] styles = new ulong[8];
	
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
		}

		void Update() {
			if (chunk == null) {
				chunk = new VoxelChunk();
			}
			
			//chunk[1,1,1] = new Voxel(1, SubVoxel.BOX_BOTTOM_HALF);
			chunk[1,1,1] = new Voxel(1, SubVoxel.BOX);
			chunk[1,1,1] = new Voxel(1, SubVoxel.BOX_BOTTOM_HALF);
			/*
			chunk[1,2,1] = new Voxel(1, styles[1]);
			chunk[1,1,2] = new Voxel(1, styles[2]);
			chunk[1,2,2] = new Voxel(1, styles[3]);
			chunk[2,2,2] = new Voxel(1, styles[4]);
			chunk[2,2,1] = new Voxel(1, styles[5]);
			chunk[2,1,2] = new Voxel(1, styles[6]);
			chunk[2,1,1] = new Voxel(1, styles[7]);
			*/

			MeshFilter filter = gameObject.GetComponent<MeshFilter>();

			if (filter == null) {
				filter = gameObject.AddComponent<MeshFilter>();
			}
			
			if (filter.sharedMesh == null) {
				filter.sharedMesh = new Mesh();
			}
			
			chunk.FillMesh(filter.sharedMesh);
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
