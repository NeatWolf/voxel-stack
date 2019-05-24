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
    // Start is called before the first frame update
    void Start() {
        bitvoxel = 0;
    }

    // Update is called once per frame
    void Update() {
        byte voxel = (byte)bitvoxel;
        if (indexText != null) {
            indexText.text = "Index  = " + voxel;
            bitText.text = "Bits     = " + voxel.BitString().Substring(2);
        }
    }

    public int resWidth = 256;
    public int resHeight = 256;

    private bool takeHiResShot = false;

    public static string ScreenShotName(int bitvoxel)
    {
        return string.Format("{0}/screenshots/screen_{1}.png",
                             Application.dataPath, bitvoxel);
    }

    public void TakeHiResShot()
    {
        takeHiResShot = true;
    }

    int counter = 33;

    void LateUpdate()
    {
        counter++;

        if (counter == 32) {
            bitvoxel++;
        }

        takeHiResShot |= Input.GetKeyDown("k");
        if (takeHiResShot)
        {
            string filename = ScreenShotName(bitvoxel);
            ScreenCapture.CaptureScreenshot(filename, 1);
            takeHiResShot = false;

            Debug.Log("Captured " + bitvoxel);
            counter = 0;

            /* 
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
            */
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
