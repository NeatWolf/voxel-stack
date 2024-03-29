﻿using UnityEngine;
using UnityEditor;
using BitStack;
using System.IO;

public class VoxelLUTGenerator : EditorWindow {

	public const float SUBVOXEL_SIZE = 0.25f;

	/**
	 * All the required vertices to make a voxel/cube
	 * in size of 0.25 x 0.25 x 0.25. With this system, each
	 * voxel will have 64 sub-voxels. One voxel will be 1 x 1 x 1
	 */
	private static Vector3[] VERTEX_LUT_N = {
		new Vector3(0,1,0),
		new Vector3(1,1,0),
		new Vector3(1,0,0),
		new Vector3(0,0,0),
		new Vector3(0,1,1),
		new Vector3(1,1,1),
		new Vector3(1,0,1),
		new Vector3(0,0,1)
	};
	
	/**
	 * All the required vertices to make a voxel/cube
	 * in size of 0.25 x 0.25 x 0.25. With this system, each
	 * voxel will have 64 sub-voxels. One voxel will be 1 x 1 x 1
	 */
	private static Vector3[] VERTEX_LUT = {
		new Vector3(VERTEX_LUT_N[0].x * SUBVOXEL_SIZE, VERTEX_LUT_N[0].y * SUBVOXEL_SIZE, VERTEX_LUT_N[0].z * SUBVOXEL_SIZE),
		new Vector3(VERTEX_LUT_N[1].x * SUBVOXEL_SIZE, VERTEX_LUT_N[1].y * SUBVOXEL_SIZE, VERTEX_LUT_N[1].z * SUBVOXEL_SIZE),
		new Vector3(VERTEX_LUT_N[2].x * SUBVOXEL_SIZE, VERTEX_LUT_N[2].y * SUBVOXEL_SIZE, VERTEX_LUT_N[2].z * SUBVOXEL_SIZE),
		new Vector3(VERTEX_LUT_N[3].x * SUBVOXEL_SIZE, VERTEX_LUT_N[3].y * SUBVOXEL_SIZE, VERTEX_LUT_N[3].z * SUBVOXEL_SIZE),
		new Vector3(VERTEX_LUT_N[4].x * SUBVOXEL_SIZE, VERTEX_LUT_N[4].y * SUBVOXEL_SIZE, VERTEX_LUT_N[4].z * SUBVOXEL_SIZE),
		new Vector3(VERTEX_LUT_N[5].x * SUBVOXEL_SIZE, VERTEX_LUT_N[5].y * SUBVOXEL_SIZE, VERTEX_LUT_N[5].z * SUBVOXEL_SIZE),
		new Vector3(VERTEX_LUT_N[6].x * SUBVOXEL_SIZE, VERTEX_LUT_N[6].y * SUBVOXEL_SIZE, VERTEX_LUT_N[6].z * SUBVOXEL_SIZE),
		new Vector3(VERTEX_LUT_N[7].x * SUBVOXEL_SIZE, VERTEX_LUT_N[7].y * SUBVOXEL_SIZE, VERTEX_LUT_N[7].z * SUBVOXEL_SIZE)
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
		new int[]{7, 6, 5, 4},
		// left
		new int[]{3, 7, 4, 0},
		// right
		new int[]{1, 5, 6, 2},
		// up
		new int[]{0, 4, 5, 1},
		// down
		new int[]{2, 6, 7, 3}
	};
	
	/**
	 * All the required face normals for each voxel face.
	 * we will be going for a "Blocky" Voxel look
	 */
	private static Vector3[] VERTEX_NORMALS = {
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
	
	public bool includeComments = true;

	[MenuItem("VoxelStack/LUTGenerator")]	
	public static void ShowWindow() {
		EditorWindow.GetWindow(typeof(VoxelLUTGenerator));
	}

	void OnGUI() {
		GUILayout.BeginArea(new Rect(0, 0, 300, 600));
		EditorGUILayout.HelpBox("This tool is used to generate a LUT table for the VoxelStack Compute Shader.", MessageType.None);
		EditorGUILayout.LabelField("Maximum Number of Variations: " + ("00111111".ByteFromBitString(0) + 1));
		includeComments = EditorGUILayout.Toggle("Include Debug Comments", includeComments);
		
		if (GUILayout.Button("Generate LUT Tables")) {
			GenerateLUTClass(includeComments);
		}
		
		GUILayout.EndArea();
	}
	
	static void GenerateLUTClass(bool includeComments) {
		string copyFolder = "Assets/Generated";
		string copyPath = copyFolder + "/GeneratedVoxelTable.cs";
		
		if (!Directory.Exists(copyFolder)) {
			Directory.CreateDirectory(copyFolder);
		}
		
		FileUtil.DeleteFileOrDirectory(copyPath);
		
		if (File.Exists(copyPath) == false) {
			using (StreamWriter outfile = new StreamWriter(copyPath)) {
				outfile.Write("using UnityEngine;\n\n");
				outfile.Write("namespace VoxelStackLUT {\n\n");
				outfile.Write("\tpublic sealed class GeneratedVoxelTable {\n\n");
				outfile.Write("\t\tprivate static readonly Vector3[] VERTEX_LUT = {\n");
				
				int[] lutIndices = new int[64];
				
				int currentCount = 0;
				int previousCount = 0;
				
				for (int i = 0; i < 64; i++) {
					if (includeComments) {
						outfile.Write("\t\t\t// ----------------- \n");
					}
					
					int count = GenerateVerticesForIndex(outfile, i, includeComments);
					
					currentCount += previousCount;
					previousCount = count;
					
					lutIndices[i] = currentCount;
				}
				
				outfile.Write("\t\t};\n\n");
				
				outfile.Write("\t\tprivate static readonly Vector3[] NORMAL_LUT = {\n");
				
				for (int i = 0; i < 64; i++) {
					if (includeComments) {
						outfile.Write("\t\t\t// ----------------- \n");
					}
					
					GenerateNormalsForIndex(outfile, i, includeComments);
				}
				
				outfile.Write("\t\t};\n\n");
				
				outfile.Write("\t\tprivate static readonly int[] INDEX_LUT = {\n");
				outfile.Write("\t\t\t");
				
				for (int i = 0; i < lutIndices.Length; i++) {
					outfile.Write(lutIndices[i] + ",");
				}
				
				outfile.Write((currentCount + previousCount) + "\n");
				
				outfile.Write("\t\t};\n\n");
				
				GenerateFunctions(outfile);
				
				outfile.Write("\t}\n");
				outfile.Write("}");
			}
		}
		
		AssetDatabase.Refresh();
	}
	
	static void GenerateFunctions(StreamWriter writer) {
		writer.Write("\t\tpublic static Vector3[] VertexTable {\n");
		writer.Write("\t\t\tget {\n");
		writer.Write("\t\t\t\treturn VERTEX_LUT;\n");
		writer.Write("\t\t\t}\n");
		writer.Write("\t\t}\n\n");
		
		writer.Write("\t\tpublic static Vector3[] NormalTable {\n");
		writer.Write("\t\t\tget {\n");
		writer.Write("\t\t\t\treturn NORMAL_LUT;\n");
		writer.Write("\t\t\t}\n");
		writer.Write("\t\t}\n\n");
		
		writer.Write("\t\tpublic static int[] IndexTable {\n");
		writer.Write("\t\t\tget {\n");
		writer.Write("\t\t\t\treturn INDEX_LUT;\n");
		writer.Write("\t\t\t}\n");
		writer.Write("\t\t}\n");
	}
	
	static void GenerateNormalsForIndex(StreamWriter writer, int voxelValue, bool includeComments) {
		GenerateVerticesForIndex(writer, voxelValue, includeComments, false);
	}
	
	static int GenerateVerticesForIndex(StreamWriter writer, int voxelValue, bool includeComments, bool verts = true) {
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
		
		if (includeComments) {
			writer.Write("\t\t\t");
			writer.Write("// Voxel Value='" 
			+ voxelValue 
			+ "' Hex='" 
			+ voxelValue.HexString() 
			+ "' Bin='"
			+ ((byte)voxelValue).BitString()
			+ "' Faces='"
			+ voxelValue.PopCount()
			+ "'\n");
		}
		
		int count = 0;
		
		for (int i = 0; i < lutIndex.Length; i++) {
			if (includeComments) {
				writer.Write("\t\t\t\t");
				writer.Write("// LUT Bit Index='" + i + "' Face='" + lutNames[i] + "' State='" + (lutIndex[i] == 0 ? "OFF" : "ON") + "'\n");
			}
			
			if (lutIndex[i] == 1) {
				writer.Write("\t\t\t");
				
				if (includeComments) {
					writer.Write("\t");
				}
				
				int[] faceIndices = VOXEL_FACES[i];
				
				for (int vIndex = 0; vIndex < faceIndices.Length - 1; vIndex++) {
					Vector3 vertex = verts == true ? VERTEX_LUT[faceIndices[vIndex]] : VERTEX_NORMALS[i];
					writer.Write("new Vector3(");
					writer.Write(floatS(vertex.x) + ",");
					writer.Write(floatS(vertex.y) + ",");
					writer.Write(floatS(vertex.z) + "), ");
					count++;
				}
				
				Vector3 lastVertex = verts == true ? VERTEX_LUT[faceIndices[faceIndices.Length - 1]] : VERTEX_NORMALS[i];
				writer.Write("new Vector3(");
				writer.Write(floatS(lastVertex.x) + ",");
				writer.Write(floatS(lastVertex.y) + ",");
				
				if (voxelValue < 64) {
					writer.Write(floatS(lastVertex.z) + "),");
				}
				
				count++;
				
				writer.Write("\n");
			}
			else {
				if (includeComments) {
					writer.Write("// NO RENDERABLE FACE\n");
				}
			}
		}
		
		return count;
	}
	
	static string floatS(float val) {
		if (val.IsEqual(0.0f)) {
			return "0.0f";
		}
		
		if (val < 0.30f && val > 0.0f) {
			return "0" + val.ToString("#.00") + "f";
		}
		
		return val.ToString("#.0") + "f";
	}
}
