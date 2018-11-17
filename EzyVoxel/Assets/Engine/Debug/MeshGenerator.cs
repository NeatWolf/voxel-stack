using UnityEngine;
using VoxelStackLUT;
using BitStack;

namespace VoxelStack {
#if UNITY_EDITOR || DEBUG
	/**
	 * This functionality is used for debugging purposes to ensure
	 * rendering is performed properly. 
	 * Proper rendering via Compute Shaders should be used for the most optimum
	 * performance.
	 */
	public class MeshGenerator {
		
		public static int FillVertices(byte voxel, MortonKey3 mortonKey, float positionScale, ref Vector3[] array, int from) {
			Vector3[] vertexLutTable = GeneratedVoxelTable.VertexTable;
			int[] indexLutTable = GeneratedVoxelTable.IndexTable;
			
			int indexFrom = indexLutTable[voxel];
			int indexTo = indexLutTable[voxel + 1];
			
			Vector3 decodedKey = mortonKey.Value;
			decodedKey.x *= positionScale;
			decodedKey.y *= positionScale;
			decodedKey.z *= positionScale;
			
			for (int i = indexFrom; i < indexTo; i++) {
				array[from++] = vertexLutTable[i] + decodedKey;
			}
			
			return from;
		}
		
		public static int FillNormals(byte voxel, ref Vector3[] array, int from) {
			Vector3[] vertexLutTable = GeneratedVoxelTable.NormalTable;
			int[] indexLutTable = GeneratedVoxelTable.IndexTable;
			
			int indexFrom = indexLutTable[voxel];
			int indexTo = indexLutTable[voxel + 1];
			
			for (int i = indexFrom; i < indexTo; i++) {
				array[from++] = vertexLutTable[i];
			}
			
			return from;
		}
	}
#endif
}
