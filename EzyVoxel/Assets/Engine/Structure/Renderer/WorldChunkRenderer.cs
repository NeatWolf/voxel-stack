using UnityEngine;
using System.Collections;

namespace VoxelStack {
	public sealed class WorldChunkRenderer : MonoBehaviour {
		
		public WorldChunk chunk;
		public Material material;
		
		// Use this for initialization
		void Start() {
			chunk = new WorldChunk(material);
			
			SimplexNoiseGenerator generator = new SimplexNoiseGenerator();
			
			for (uint x = 0; x < 4; x++) {
				for (uint y = 0; y < 4; y++) {
					for (uint z = 0; z < 4; z++) {
						generateForChunk(chunk[x,y,z], generator, x, y, z);
					}
				}
			}
		}
		
		private void generateForChunk(VoxelChunk vchunk, SimplexNoiseGenerator generator, uint cx, uint cy, uint cz) {
			uint worldX = (cx * 4);
			uint worldY = (cy * 4);
			uint worldZ = (cz * 4);
			
			for (uint x = 0; x < 4; x++) {
				for (uint y = 0; y < 4; y++) {
					for (uint z = 0; z < 4; z++) {
						vchunk[x,y,z] = generateForVoxel(generator, worldX + x, worldY + y, worldZ + z);
					}
				}
			}
		}
		
		private Voxel generateForVoxel(SimplexNoiseGenerator generator, uint vx, uint vy, uint vz) {
			uint worldX = (vx * 4);
			uint worldY = (vy * 4);
			uint worldZ = (vz * 4);
			
			SubVoxel voxel = SubVoxel.ZERO;
			
			for (uint x = 0; x < 4; x++) {
				for (uint y = 0; y < 4; y++) {
					for (uint z = 0; z < 4; z++) {
						int density = generator.getDensity(new Vector3(worldX + x, worldY + y, worldZ + z));
						voxel = voxel.Set(x, y, z, density > 8 ? SubVoxel.SET : SubVoxel.UNSET);
					}
				}
			}
			
			return new Voxel(1, voxel);
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
			//chunk.Fill(new Voxel(1, SubVoxel.FULL.Set(2,3,2,0)));
		}
	}
}
