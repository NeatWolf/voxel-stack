using UnityEngine;
using System.Collections;

namespace VoxelStack {
	public sealed class WorldChunkRenderer : MonoBehaviour {
		
		public WorldChunk chunk;
		public int ping = 1;
		// Use this for initialization
		void Start() {
			chunk = new WorldChunk();
		}
	
		// Update is called once per frame
		void Update() {
		/*
			if (ping == 1) {
				ping = 0;
				chunk.Fill(new Voxel(1, SubVoxel.BOX_HALF));
			}
			else {
				ping = 1;
				chunk.Fill(new Voxel(1, SubVoxel.BOX));
			}
		}
		*/
		chunk.Fill(new Voxel(1, SubVoxel.BOX));
		}
	}
}
