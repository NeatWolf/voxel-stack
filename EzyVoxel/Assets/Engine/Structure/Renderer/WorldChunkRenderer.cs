using UnityEngine;
using System.Collections;

namespace VoxelStack {
	public sealed class WorldChunkRenderer : MonoBehaviour {
		
		public WorldChunk chunk;
		public Material material;
		public SimplexNoiseGenerator gen;

		int offsetx = 0;
		int offsety = 0;
		int offsetz = 0;

		int tolerance = 32;
		int voxelTolerance = 32;

		bool isBlocky = false;

		public int offx = 1;
		public int offy = 1;
		public int offz = 1;

		public int toleranceValue = 32;
		public int voxelToleranceValue = 32;

		public bool isBlockyValue = false;
		
		// Use this for initialization
		void Start() {
			chunk = new WorldChunk(material);
			gen = new SimplexNoiseGenerator(42);

            UpdateVoxelMesh();
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
						int density = generator.getDensity(new Vector3(worldX + x + offsetx, worldY + y + offsety, worldZ + z + offsetz));
						voxel = voxel.Set(x, y, z, density > tolerance ? SubVoxel.SET : SubVoxel.UNSET);
					}
				}
			}
			
			if (!isBlocky) {
				return new Voxel(1, voxel);
			}
			
			return new Voxel(1, voxel.Length > voxelTolerance ? SubVoxel.FULL : SubVoxel.ZERO);
		}
	
		// Update is called once per frame
		void Update() {
			if (CheckAndUpdateValues()) {
                UpdateVoxelMesh();
			}
		}

		void UpdateVoxelMesh() {
            for (uint x = 0; x < 4; x++) {
                for (uint y = 0; y < 4; y++) {
                    for (uint z = 0; z < 4; z++) {
                        generateForChunk(chunk[x, y, z], gen, x, y, z);
                    }
                }
            }
		}

		bool CheckAndUpdateValues() {
            if (offsetx != offx || 
				offsety != offy || 
				offsetz != offz || 
				isBlocky != isBlockyValue ||
				tolerance != toleranceValue ||
				voxelTolerance != voxelToleranceValue) {
					offsetx = offx;
					offsety = offy;
					offsetz = offz;
					isBlocky = isBlockyValue;
					tolerance = toleranceValue;
					voxelTolerance = voxelToleranceValue;

					return true;
				}

			return false;
		}
	}
}
