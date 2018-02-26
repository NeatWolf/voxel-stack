using UnityEngine;
using System.Collections;
using UnityEditor;

/**
 * Represents Normalized pre-computed coordinates for
 * a single Block/Voxel type. Allows easy and fast access
 * into the internal data structure.
 * 
 * Because all data is stored in a predictable manner, only the triangles
 * need to be updated on a per chunk basis. And since the vertices themselves
 * don't change, we don't need to keep re-uploading the vertex data whenever
 * triangles change.
 */
namespace VoxelLUT {
    public sealed class Block {
        public const int SIZE = 21;

        // top side LUT
        public const int V1 = 0;
        public const int V2 = 1;
        public const int V3 = 2;
        public const int V4 = 3;

        // bottom side LUT
        public const int V5 = 4;
        public const int V6 = 5;
        public const int V7 = 6;
        public const int V8 = 7;

        // top sides (halfs)
        public const int V12 = 8;
        public const int V21 = 8;
        public const int V23 = 9;
        public const int V32 = 9;
        public const int V34 = 10;
        public const int V43 = 10;
        public const int V14 = 11;
        public const int V41 = 11;

        // bottom side (halfs)
        public const int V56 = 12;
        public const int V65 = 12;
        public const int V67 = 13;
        public const int V76 = 13;
        public const int V78 = 14;
        public const int V87 = 14;
        public const int V58 = 15;
        public const int V85 = 15;

        // left side (halfs)
        public const int V15 = 16;
        public const int V51 = 16;
        public const int V26 = 17;
        public const int V62 = 17;

        // right side (halfs)
        public const int V37 = 18;
        public const int V73 = 18;
        public const int V48 = 19;
        public const int V84 = 19;

        // center (halfs)
        public const int VC = 20;

        private static readonly Vector3[] _LUT;
        private static readonly Vector3[] _NOR;

        static Block() {
            _LUT = new Vector3[SIZE];
            _NOR = new Vector3[SIZE];

            _LUT[V1] = new Vector3(0.0f, 1.0f, 0.0f);
            _LUT[V2] = new Vector3(0.0f, 1.0f, 1.0f);
            _LUT[V3] = new Vector3(1.0f, 1.0f, 1.0f);
            _LUT[V4] = new Vector3(1.0f, 1.0f, 0.0f);

            _LUT[V5] = new Vector3(0.0f, 0.0f, 0.0f);
            _LUT[V6] = new Vector3(0.0f, 0.0f, 1.0f);
            _LUT[V7] = new Vector3(1.0f, 0.0f, 1.0f);
            _LUT[V8] = new Vector3(1.0f, 0.0f, 0.0f);

            _LUT[V12] = new Vector3(0.0f, 1.0f, 0.5f);
            _LUT[V23] = new Vector3(0.5f, 1.0f, 1.0f);
            _LUT[V34] = new Vector3(1.0f, 1.0f, 0.5f);
            _LUT[V14] = new Vector3(0.5f, 1.0f, 0.0f);

            _LUT[V56] = new Vector3(0.0f, 0.0f, 0.5f);
            _LUT[V67] = new Vector3(0.5f, 0.0f, 1.0f);
            _LUT[V78] = new Vector3(1.0f, 0.0f, 0.5f);
            _LUT[V58] = new Vector3(0.5f, 0.0f, 0.0f);

            _LUT[V15] = new Vector3(0.0f, 0.5f, 0.0f);
            _LUT[V26] = new Vector3(0.0f, 0.5f, 1.0f);

            _LUT[V37] = new Vector3(1.0f, 0.5f, 1.0f);
            _LUT[V48] = new Vector3(1.0f, 0.5f, 0.0f);

            _LUT[VC] = new Vector3(0.5f, 0.5f, 0.5f);
        }

        /**
         * Fill the provided array with normalized vertex data. Offset the Vertices
         * using the provided position. Data will be added using the provided index up to
         * Block.SIZE
         */
        public static void FillVertex(Vector3 position, ref Vector3[] array, int startIndex) {
            for (int i = 0; i < SIZE; i++) {
                array[startIndex++] = (_LUT[i] + position);
            }
        }

        public static Vector3 Get(int index) {
            return _LUT[index];
        }

        /**
         * Unity3D Editor only debug to visualize all the points and the
         * Voxel itself.
         */
        public static void OnDebug() {
#if UNITY_EDITOR
            Color oldColor = Gizmos.color;
            Gizmos.color = Color.red;

            // draw all the points
            for (int i = 0; i < SIZE; i++) {
                Gizmos.DrawSphere(_LUT[i], 0.02f);
            }

            Gizmos.color = Color.green;

            // draw the top face
            Gizmos.DrawLine(_LUT[V1], _LUT[V2]);
            Gizmos.DrawLine(_LUT[V2], _LUT[V3]);
            Gizmos.DrawLine(_LUT[V3], _LUT[V4]);
            Gizmos.DrawLine(_LUT[V4], _LUT[V1]);

            // draw the bottom face
            Gizmos.DrawLine(_LUT[V5], _LUT[V6]);
            Gizmos.DrawLine(_LUT[V6], _LUT[V7]);
            Gizmos.DrawLine(_LUT[V7], _LUT[V8]);
            Gizmos.DrawLine(_LUT[V8], _LUT[V5]);

            // draw the corners
            Gizmos.DrawLine(_LUT[V1], _LUT[V5]);
            Gizmos.DrawLine(_LUT[V2], _LUT[V6]);
            Gizmos.DrawLine(_LUT[V3], _LUT[V7]);
            Gizmos.DrawLine(_LUT[V4], _LUT[V8]);

            Gizmos.color = Color.yellow;

            // draw top face (mid sections)
            Gizmos.DrawLine(_LUT[V12], _LUT[V23]);
            Gizmos.DrawLine(_LUT[V23], _LUT[V34]);
            Gizmos.DrawLine(_LUT[V34], _LUT[V14]);
            Gizmos.DrawLine(_LUT[V14], _LUT[V12]);

            // draw bottom face (mid sections)
            Gizmos.DrawLine(_LUT[V56], _LUT[V67]);
            Gizmos.DrawLine(_LUT[V67], _LUT[V78]);
            Gizmos.DrawLine(_LUT[V78], _LUT[V58]);
            Gizmos.DrawLine(_LUT[V58], _LUT[V56]);

            // draw front face (mid sections)
            Gizmos.DrawLine(_LUT[V15], _LUT[V14]);
            Gizmos.DrawLine(_LUT[V14], _LUT[V48]);
            Gizmos.DrawLine(_LUT[V48], _LUT[V58]);
            Gizmos.DrawLine(_LUT[V58], _LUT[V15]);

            // draw back face (mid sections)
            Gizmos.DrawLine(_LUT[V23], _LUT[V37]);
            Gizmos.DrawLine(_LUT[V37], _LUT[V67]);
            Gizmos.DrawLine(_LUT[V67], _LUT[V26]);
            Gizmos.DrawLine(_LUT[V26], _LUT[V23]);

            // draw right face (mid sections)
            Gizmos.DrawLine(_LUT[V37], _LUT[V78]);
            Gizmos.DrawLine(_LUT[V78], _LUT[V48]);
            Gizmos.DrawLine(_LUT[V48], _LUT[V34]);
            Gizmos.DrawLine(_LUT[V34], _LUT[V37]);

            // draw left face (mid sections)
            Gizmos.DrawLine(_LUT[V12], _LUT[V26]);
            Gizmos.DrawLine(_LUT[V26], _LUT[V56]);
            Gizmos.DrawLine(_LUT[V56], _LUT[V15]);
            Gizmos.DrawLine(_LUT[V15], _LUT[V12]);

            Gizmos.color = oldColor;
#endif
        }
    }
}
