using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelStackLUT;
using BitStack;

namespace VoxelStack {
	public class MeshGenerator {
		
		public static int FillVertices(byte voxel, uint mortonKey, ref Vector3[] array, int from) {
			Vector3[] vertexLutTable = GeneratedVoxelTable.VertexTable;
			int[] indexLutTable = GeneratedVoxelTable.IndexTable;
			
			int indexFrom = indexLutTable[voxel];
			int indexTo = indexLutTable[voxel + 1];
			
			Vector3 decodedKey = mortonKey.DecodeMortonKey3();
			
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
}
