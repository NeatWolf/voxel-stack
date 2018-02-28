using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

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

        public const int SIZE = 54;

        private static readonly Vector3[] _POS;
        private static readonly Vector3[] _NOR;

        /**
         * Statically fill in our Vertices and Indices
         * so we can reference them via pre-defined indices
         * from 0-53 (Check Block.SIZE for maximum index size)
         * Indices are encoded in the Enums for the Voxels
         */
        static Block() {
            _POS = new Vector3[SIZE];
            _NOR = new Vector3[SIZE];

            // front face
            Front.v1.Add(ref _POS, ref _NOR);
            Front.v2.Add(ref _POS, ref _NOR);
            Front.v3.Add(ref _POS, ref _NOR);
            Front.v4.Add(ref _POS, ref _NOR);
            Front.v12.Add(ref _POS, ref _NOR);
            Front.v23.Add(ref _POS, ref _NOR);
            Front.v34.Add(ref _POS, ref _NOR);
            Front.v14.Add(ref _POS, ref _NOR);
            Front.vc.Add(ref _POS, ref _NOR);

            // back face
            Back.v1.Add(ref _POS, ref _NOR);
            Back.v2.Add(ref _POS, ref _NOR);
            Back.v3.Add(ref _POS, ref _NOR);
            Back.v4.Add(ref _POS, ref _NOR);
            Back.v12.Add(ref _POS, ref _NOR);
            Back.v23.Add(ref _POS, ref _NOR);
            Back.v34.Add(ref _POS, ref _NOR);
            Back.v14.Add(ref _POS, ref _NOR);
            Back.vc.Add(ref _POS, ref _NOR);

            // left face
            Left.v1.Add(ref _POS, ref _NOR);
            Left.v2.Add(ref _POS, ref _NOR);
            Left.v3.Add(ref _POS, ref _NOR);
            Left.v4.Add(ref _POS, ref _NOR);
            Left.v12.Add(ref _POS, ref _NOR);
            Left.v23.Add(ref _POS, ref _NOR);
            Left.v34.Add(ref _POS, ref _NOR);
            Left.v14.Add(ref _POS, ref _NOR);
            Left.vc.Add(ref _POS, ref _NOR);

            // right face
            Right.v1.Add(ref _POS, ref _NOR);
            Right.v2.Add(ref _POS, ref _NOR);
            Right.v3.Add(ref _POS, ref _NOR);
            Right.v4.Add(ref _POS, ref _NOR);
            Right.v12.Add(ref _POS, ref _NOR);
            Right.v23.Add(ref _POS, ref _NOR);
            Right.v34.Add(ref _POS, ref _NOR);
            Right.v14.Add(ref _POS, ref _NOR);
            Right.vc.Add(ref _POS, ref _NOR);

            // up face
            Up.v1.Add(ref _POS, ref _NOR);
            Up.v2.Add(ref _POS, ref _NOR);
            Up.v3.Add(ref _POS, ref _NOR);
            Up.v4.Add(ref _POS, ref _NOR);
            Up.v12.Add(ref _POS, ref _NOR);
            Up.v23.Add(ref _POS, ref _NOR);
            Up.v34.Add(ref _POS, ref _NOR);
            Up.v14.Add(ref _POS, ref _NOR);
            Up.vc.Add(ref _POS, ref _NOR);

            // down face
            Down.v1.Add(ref _POS, ref _NOR);
            Down.v2.Add(ref _POS, ref _NOR);
            Down.v3.Add(ref _POS, ref _NOR);
            Down.v4.Add(ref _POS, ref _NOR);
            Down.v12.Add(ref _POS, ref _NOR);
            Down.v23.Add(ref _POS, ref _NOR);
            Down.v34.Add(ref _POS, ref _NOR);
            Down.v14.Add(ref _POS, ref _NOR);
            Down.vc.Add(ref _POS, ref _NOR);
        }

        /**
         * Fill the provided array with normalized vertex data. Offset the Vertices
         * using the provided position. Data will be added using the provided index up to
         * Block.SIZE
         */
        public static void FillVertex(Vector3 position, ref Vector3[] array, int startIndex) {
            for (int i = 0; i < SIZE; i++) {
                array[startIndex++] = (_POS[i] + position);
            }
        }

        /**
         * Fill the provided array with normal (direction) data, used for lighting.
         */
        public static void FillNormal(ref Vector3[] array, int startIndex) {
            for (int i = 0; i < SIZE; i++) {
                array[startIndex++] = (_NOR[i]);
            }
        }

        public static Vector3 Get(int index) {
            return _POS[index];
        }

        /**
         * Unity3D Editor only debug to visualize all the points and the
         * Voxel itself.
         */
        public static void OnDebug() {
#if !UNITY_EDITOR
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
