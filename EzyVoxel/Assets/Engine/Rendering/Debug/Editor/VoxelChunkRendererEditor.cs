using UnityEngine;
using UnityEditor;
using System;

namespace VoxelStackDebug {
	[CustomEditor(typeof(VoxelChunkRenderer))]
	public class VoxelChunkRendererEditor : Editor {
		public override void OnInspectorGUI() {
			EditorGUILayout.HelpBox("VoxelChunkRenderer.cs is a Debug/Editor Script and will NOT be included in production builds.", MessageType.Warning);
			
			VoxelChunkRenderer renderer = (VoxelChunkRenderer)target;
			
			renderer.drawDebug = EditorGUILayout.Toggle("Render Gizmos", renderer.drawDebug);
			renderer.material = (Material)EditorGUILayout.ObjectField("Material", (Material)renderer.material, typeof(Material));
			
			var valuesAsArray = (ChunkStyle[])Enum.GetValues(typeof(ChunkStyle));
			
			for (int i = 0; i < 8; i++) {
				ChunkStyle type = ChunkStyle.BOX;
				
				for (int j = 0; j < 8; j++) {
					if ((ulong)(valuesAsArray[j]) == renderer.styles[i]) {
						type = valuesAsArray[j];
						break;
					}
				}
				
				ChunkStyle newStyle = (ChunkStyle)EditorGUILayout.EnumPopup("Chunk Section " + i, type);
				
				renderer.styles[i] = (ulong)newStyle;
			}
		}
	}
}
