using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VoxelStack;
using VoxelStackLUT;
using BitStack;

public class BitVoxelVisualiser : MonoBehaviour {

    public int bitvoxel = 0;

    public Text indexText;
    public Text bitText;

    public Camera m_camera;
    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        byte voxel = (byte)bitvoxel;
        indexText.text = "Index  = " + voxel;
        bitText.text = "Bits     = " + voxel.BitString();
    }

    public int resWidth = 256;
    public int resHeight = 256;

    private bool takeHiResShot = false;

    public static string ScreenShotName(int width, int height)
    {
        return string.Format("{0}/screenshots/screen_{1}x{2}_{3}.png",
                             Application.dataPath,
                             width, height,
                             System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
    }

    public void TakeHiResShot()
    {
        takeHiResShot = true;
    }

    void LateUpdate()
    {
        takeHiResShot |= Input.GetKeyDown("k");
        if (takeHiResShot)
        {
            RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
            m_camera.targetTexture = rt;
            Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
            m_camera.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
            m_camera.targetTexture = null;
            RenderTexture.active = null; // JC: added to avoid errors
            Destroy(rt);
            byte[] bytes = screenShot.EncodeToPNG();
            string filename = ScreenShotName(resWidth, resHeight);
            System.IO.File.WriteAllBytes(filename, bytes);
            Debug.Log(string.Format("Took screenshot to: {0}", filename));
            takeHiResShot = false;
        }
    }

    void OnDrawGizmos() {
        byte voxel = (byte)bitvoxel;
        Vector3[] vertexLutTable = GeneratedVoxelTable.VertexTable;
        int[] indexLutTable = GeneratedVoxelTable.IndexTable;

        int indexFrom = indexLutTable[voxel];
        int indexTo = indexLutTable[voxel + 1];

        for (int i = indexFrom; i < indexTo; i+=4) {
            Vector3 v1 = vertexLutTable[i] * 10;
            Vector3 v2 = vertexLutTable[i + 1] * 10;
            Vector3 v3 = vertexLutTable[i + 2] * 10;
            Vector3 v4 = vertexLutTable[i + 3] * 10;

            /*
            Gizmos.DrawLine(v1, v2);
            Gizmos.DrawLine(v2, v3);
            Gizmos.DrawLine(v3, v4);
            Gizmos.DrawLine(v4, v1);
            */

            Gizmos.color = Color.red;

            Gizmos.DrawSphere(v1, 0.05f);
            Gizmos.DrawSphere(v2, 0.05f);
            Gizmos.DrawSphere(v3, 0.05f);
            Gizmos.DrawSphere(v4, 0.05f);

            Gizmos.color = Color.black;

            Gizmos.DrawLine(v1, v2);
            Gizmos.DrawLine(v1, v3);
            Gizmos.DrawLine(v2, v3);

            Gizmos.DrawLine(v1, v3);
            Gizmos.DrawLine(v1, v4);
            Gizmos.DrawLine(v3, v4);

            Gizmos.color = Color.red;

            //Gizmos.DrawSphere((v1 + v2 + v3) / 3.0f, 0.01f);
            //Gizmos.DrawSphere((v1 + v3 + v4) / 3.0f, 0.01f);

            //Gizmos.DrawSphere(vertexLutTable[i] * 10.0f, 0.1f);
        }
    }
}
