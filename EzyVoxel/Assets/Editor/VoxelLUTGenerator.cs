using UnityEngine;
using UnityEditor;
using BitStack;
using System.IO;

public class VoxelLUTGenerator : EditorWindow {

	/**
	 * All the required vertices to make a voxel/cube
	 * in size of 0.25 x 0.25 x 0.25. With this system, each
	 * voxel will have 64 sub-voxels. One voxel will be 1 x 1 x 1
	 */
	private static Vector3[] VERTEX_LUT = {
		new Vector3(0.0f, 0.25f, 0.0f),
		new Vector3(0.25f, 0.25f, 0.0f),
		new Vector3(0.25f, 0.0f, 0.0f),
		new Vector3(0.0f, 0.0f, 0.0f),
		new Vector3(0.0f, 0.0f, 0.25f),
		new Vector3(0.25f, 0.0f, 0.25f),
		new Vector3(0.25f, 0.25f, 0.25f),
		new Vector3(0.0f, 0.25f, 0.0f)
	};
	
	/**
	 * LUT array which points into the vertices array
	 * to make a single voxel face. Since our geometry
	 * is generated in a compute shader, we will only be
	 * working with arrays of vertices
	 */
	private static int[][] VOXEL_FACES = {
		// front
		new int[]{0, 1, 2, 3},
		// back
		new int[]{4, 5, 6, 7},
		// left
		new int[]{1, 2, 5, 6},
		// right
		new int[]{0, 3, 4, 7},
		// up
		new int[]{0, 1, 6, 7},
		// down
		new int[]{2, 3, 4, 5}
	};
	
	/**
	 * All the required face normals for each voxel face.
	 * we will be going for a "Blocky" Voxel look
	 */
	private static Vector3[] VOXEL_NORMALS = {
		// front
		new Vector3(0.0f, 0.0f, -1.0f),
		// back
		new Vector3(0.0f, 0.0f, 1.0f),
		// left
		new Vector3(-1.0f, 0.0f, 0.0f),
		// right
		new Vector3(1.0f, 0.0f, 0.0f),
		// up
		new Vector3(0.0f, 1.0f, 0.0f),
		// down
		new Vector3(0.0f, -1.0f, 0.0f)
	};

	[MenuItem("VoxelStack/LUTGenerator")]	
	public static void ShowWindow() {
		EditorWindow.GetWindow(typeof(VoxelLUTGenerator));
	}

	void OnGUI() {
		GUILayout.BeginArea(new Rect(0, 0, 300, 600));
		EditorGUILayout.HelpBox("This tool is used to generate a LUT table for the VoxelStack Compute Shader.", MessageType.None);
		EditorGUILayout.LabelField("Maximum Number of Variations: " + ("00111111".ByteFromBitString(0) + 1));
		
		if (GUILayout.Button("Generate LUT Tables")) {
			GenerateLUTClass();
		}
		
		GUILayout.EndArea();
	}
	
	static void GenerateLUTClass() {
		string copyFolder = "Assets/Generated";
		string copyPath = copyFolder + "/GeneratedVoxelTable.cs";
		
		if (!Directory.Exists(copyFolder)) {
			Directory.CreateDirectory(copyFolder);
		}
		
		FileUtil.DeleteFileOrDirectory(copyPath);
		
		if (File.Exists(copyPath) == false) {
			using (StreamWriter outfile = new StreamWriter(copyPath)) {
				outfile.Write("using UnityEngine;\n\n");
				outfile.Write("public sealed class GeneratedVoxelTable {\n\n");
				outfile.Write("\tpublic static Vector3[] VOXEL_LUT = {\n");
				for (int i = 0; i < 64; i++) {
					outfile.Write("\t\t// ----------------- \n");
					GenerateVerticesForIndex(outfile, i);
				}
				outfile.Write("\t};\n");
				outfile.Write("}");
			}
		}
		
		AssetDatabase.Refresh();
	}
	
	static int GenerateVerticesForIndex(StreamWriter writer, int voxelValue) {
		int[] lutIndex = {
			voxelValue.BitAt(0),
			voxelValue.BitAt(1),
			voxelValue.BitAt(2),
			voxelValue.BitAt(3),
			voxelValue.BitAt(4),
			voxelValue.BitAt(5)
		};
		
		string[] lutNames = {
			"Front", 
			"Back", 
			"Left", 
			"Right", 
			"Up", 
			"Down"
		};
		
		writer.Write("\t\t");
		writer.Write("// Voxel Value='" 
		+ voxelValue 
		+ "' Hex='" 
		+ voxelValue.HexString() 
		+ "' Bin='"
		+ ((byte)voxelValue).BitString()
		+ "' Faces='"
		+ voxelValue.PopCount()
		+ "'\n");
		
		for (int i = 0; i < lutIndex.Length; i++) {
			writer.Write("\t\t\t");
			writer.Write("// LUT Bit Index='" + i + "' Face='" + lutNames[i] + "' State='" + (lutIndex[i] == 0 ? "ON" : "OFF") + "'\n");
			writer.Write("\t\t\t");
			
			if (lutIndex[i] == 0) {
				int[] faceIndices = VOXEL_FACES[i];
				Vector3 faceNormals = VOXEL_NORMALS[i];
				
				for (int vIndex = 0; vIndex < faceIndices.Length - 1; vIndex++) {
					Vector3 vertex = VERTEX_LUT[faceIndices[vIndex]];
					writer.Write("new Vector3(");
					writer.Write(floatS(vertex.x) + ",");
					writer.Write(floatS(vertex.y) + ",");
					writer.Write(floatS(vertex.z) + "), ");
				}
				
				Vector3 lastVertex = VERTEX_LUT[faceIndices[faceIndices.Length - 1]];
				writer.Write("new Vector3(");
				writer.Write(floatS(lastVertex.x) + ",");
				writer.Write(floatS(lastVertex.y) + ",");
				
				if (voxelValue != 63) {
					writer.Write(floatS(lastVertex.z) + "),");
				}
			}
			else {
				writer.Write("// NO RENDERABLE FACE");
			}
			
			writer.Write("\n");
		}
		
		return 0;
	}
	
	static string floatS(float val) {
		return "0" + val.ToString("#.00") + "f";
	}
}
