using UnityEngine;
using UnityEditor;
using VoxelLUT;
using System.IO;

public class VoxelBlockGenerator : EditorWindow {
    [MenuItem("Voxel/Generator")]
    public static void ShowWindow() {
        EditorWindow.GetWindow(typeof(VoxelBlockGenerator));
    }

    void OnGUI() {
        GUILayout.BeginArea(new Rect(0, 0, 300, 600));
        EditorGUILayout.HelpBox("Use this tool to generate skeleton code for all Block variations. Existing Blocks will not be replaced.", MessageType.None);
        EditorGUILayout.LabelField("Maximum Number of Variations: " + BlockLUT.MAX_LUT);

        if (GUILayout.Button("Generate Blocks")) {
            for (int i = 0; i < BlockLUT.MAX_LUT; i++) {
                CreateBlockClass(i);
            }
        }

        GUILayout.EndArea();
    }

    static void CreateBlockClass(int index) {
        string name = BlockLUT.GetRefClassName(index);

        string copyFolder = "Assets/Generated/Blocks_" + BlockLUT.MAX_LUT;
        string copyPath = copyFolder + "/" + name + ".cs";

        if (!Directory.Exists(copyFolder)) {
            Directory.CreateDirectory(copyFolder);
        }

        // do not override
        if (File.Exists(copyPath) == false) {
            using (StreamWriter outfile = new StreamWriter(copyPath)) {
                outfile.WriteLine("/*******************************************************************");
                outfile.WriteLine(" * Class Skeleton Auto-Generated via VoxelBlockGenerator");
                outfile.WriteLine(" * See Editor/VoxelBlockGenerator.cs for source");
                outfile.WriteLine(" * CLASS Name = " + name);
                outfile.WriteLine(" * CLASS Hash = 0x" + index.ToString("X"));
                outfile.WriteLine(" * BlockLUT Automatically Initialises this class for LUT purposes");
                outfile.WriteLine(" *******************************************************************/");
                outfile.WriteLine("namespace VoxelLUT {");
                outfile.WriteLine("\tpublic sealed class " + name + " : BlockVisual {");

                outfile.WriteLine("\t\t// our triangles referencing pre-set vertices");
                outfile.WriteLine("\t\tprivate readonly int[] _triangles;");
                outfile.WriteLine("");
                outfile.WriteLine("\t\t/**");
                outfile.WriteLine("\t\t * Use the private initializer to generate the triangle indices.");
                outfile.WriteLine("\t\t * We use a private initializer as protection so this class does not");
                outfile.WriteLine("\t\t * get generated elsewhere. See Create() function for usage.");
                outfile.WriteLine("\t\t */");
                outfile.WriteLine("\t\tprivate " + name + "() {");
                outfile.WriteLine("\t\t\t// The default triangles gives a blocky look by default");
                outfile.WriteLine("\t\t\t// define the specific block triangles below");
                outfile.WriteLine("\t\t\t_triangles = _DEFAULT_TRIANGLES;");
                outfile.WriteLine("\t\t}");
                outfile.WriteLine("");

                outfile.WriteLine("\t\t/**");
                outfile.WriteLine("\t\t * Simple Getter class for returning the reference to the");
                outfile.WriteLine("\t\t * Triangles array. The Base class uses this for some pre-defined");
                outfile.WriteLine("\t\t * Functionality aswell.");
                outfile.WriteLine("\t\t */");
                outfile.WriteLine("\t\tpublic override int[] Triangles {");
                outfile.WriteLine("\t\t\tget {");
                outfile.WriteLine("\t\t\t\treturn _triangles;");
                outfile.WriteLine("\t\t\t}");
                outfile.WriteLine("\t\t}");
                outfile.WriteLine("");

                outfile.WriteLine("\t\t/**");
                outfile.WriteLine("\t\t * Invoked Dynamically and Automatically via BlockLUT");
                outfile.WriteLine("\t\t */");
                outfile.WriteLine("\t\tprivate static void Create() {");
                outfile.WriteLine("\t\t\tBlockLUT.Put(" + name + ".Hash, new " + name + "());");
                outfile.WriteLine("\t\t}");
                outfile.WriteLine("");

                outfile.WriteLine("\t\t/**");
                outfile.WriteLine("\t\t * Predefined Hash code representing the LUT bucket that");
                outfile.WriteLine("\t\t * this object will be placed in. Unique for each block");
                outfile.WriteLine("\t\t */");
                outfile.WriteLine("\t\tpublic static int Hash {");
                outfile.WriteLine("\t\t\tget {");
                outfile.WriteLine("\t\t\t\treturn 0x" + index.ToString("X") + ";");
                outfile.WriteLine("\t\t\t}");
                outfile.WriteLine("\t\t}");

                outfile.WriteLine("\t}");
                outfile.WriteLine("}");
            }
        }
        AssetDatabase.Refresh();
    }
}