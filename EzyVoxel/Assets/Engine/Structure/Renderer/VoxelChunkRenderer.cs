using UnityEngine;
using BitStack;
using System.Collections.Generic;

namespace VoxelStack {

	/**
	 * The Voxel Chunk Renderer is responsible for rendering
	 * a single chunk of data either using traditional Mesh objects
	 * or Compute Shader objects.
	 */
	public sealed class VoxelChunkRenderer : MonoBehaviour {
		static readonly Stack<VoxelChunkRenderer> CHUNK_POOL = new Stack<VoxelChunkRenderer>();
		
		public static VoxelChunkRenderer Pop() {
			if (CHUNK_POOL.Count > 0) {
				VoxelChunkRenderer renderer = CHUNK_POOL.Pop();
				renderer.gameObject.SetActive(true);
				
				return renderer;
			}
			
			GameObject newObject = new GameObject();
			return newObject.AddComponent<VoxelChunkRenderer>();
		}

		VoxelChunk chunk = new VoxelChunk();
		
		#if UNITY_EDITOR || DEBUG
			public bool useComputeShader = false;
		#endif

		void Start() {
			if (chunk != null) {
				chunk = new VoxelChunk();
			}
			
			#if UNITY_EDITOR || DEBUG
				if (!useComputeShader) {
					MeshRenderer myRenderer = gameObject.GetComponent<MeshRenderer>();
					
					if (myRenderer == null) {
						myRenderer = gameObject.AddComponent<MeshRenderer>();
					}
					
					MeshFilter myFilter = gameObject.GetComponent<MeshFilter>();
					
					if (myFilter == null) {
						myFilter = gameObject.AddComponent<MeshFilter>();
					}
					
					if (myFilter.sharedMesh == null) {
						myFilter.sharedMesh = new Mesh();
					}
				}
			#endif
		}

		void Update() {
			if (chunk.IsDirty) {
				#if UNITY_EDITOR || DEBUG
					if (!useComputeShader) {
						MeshFilter myFilter = gameObject.GetComponent<MeshFilter>();
						
						if (myFilter == null) {
							myFilter = gameObject.AddComponent<MeshFilter>();
						}
						
						if (myFilter.sharedMesh == null) {
							myFilter.sharedMesh = new Mesh();
						}
						
						chunk.FillMesh(myFilter.sharedMesh);
					}
				#endif
			
				//chunk.IsDirty = false;
			}
		}

		public VoxelChunk Chunk {
			get {
				return chunk;
			}
		}
		
		public void Attach(WorldChunk world, MortonKey3 key) {
			chunk.Attach(world, key);
			
			// transform our game-object
			gameObject.transform.position = (key * 4).Value;
			
			#if UNITY_EDITOR || DEBUG
				gameObject.name = "Chunk_" + key.x + "_" + key.y + "_" + key.z;
			#endif
		}
		
		public void Detach() {
			chunk.Detach();
			
			gameObject.SetActive(false);

			CHUNK_POOL.Push(this);
		}
	}
}
