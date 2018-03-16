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

        /**
         * We have 4 vertices per face, 6 faces = 24 maximum size
         */
        public const int SIZE = 24;

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

            // back face
            Back.v1.Add(ref _POS, ref _NOR);
            Back.v2.Add(ref _POS, ref _NOR);
            Back.v3.Add(ref _POS, ref _NOR);
            Back.v4.Add(ref _POS, ref _NOR);

            // left face
            Left.v1.Add(ref _POS, ref _NOR);
            Left.v2.Add(ref _POS, ref _NOR);
            Left.v3.Add(ref _POS, ref _NOR);
            Left.v4.Add(ref _POS, ref _NOR);

            // right face
            Right.v1.Add(ref _POS, ref _NOR);
            Right.v2.Add(ref _POS, ref _NOR);
            Right.v3.Add(ref _POS, ref _NOR);
            Right.v4.Add(ref _POS, ref _NOR);

            // up face
            Up.v1.Add(ref _POS, ref _NOR);
            Up.v2.Add(ref _POS, ref _NOR);
            Up.v3.Add(ref _POS, ref _NOR);
            Up.v4.Add(ref _POS, ref _NOR);

            // down face
            Down.v1.Add(ref _POS, ref _NOR);
            Down.v2.Add(ref _POS, ref _NOR);
            Down.v3.Add(ref _POS, ref _NOR);
            Down.v4.Add(ref _POS, ref _NOR);
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
        public static void OnDebug(Vector3 position) {
#if UNITY_EDITOR
            Color oldColor = Gizmos.color;

            Vector3[] lut = new Vector3[SIZE];

            FillVertex(position + new Vector3(0.0f, 0.0f, -0.1f), ref lut, 0);

            // draw front face points
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(lut[Front.v1.Index()], 0.02f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(lut[Front.v2.Index()], 0.02f);
            Gizmos.DrawSphere(lut[Front.v3.Index()], 0.02f);
            Gizmos.DrawSphere(lut[Front.v4.Index()], 0.02f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(lut[Front.v1.Index()], lut[Front.v2.Index()]);
            Gizmos.DrawLine(lut[Front.v2.Index()], lut[Front.v3.Index()]);
            Gizmos.DrawLine(lut[Front.v3.Index()], lut[Front.v4.Index()]);
            Gizmos.DrawLine(lut[Front.v4.Index()], lut[Front.v1.Index()]);

            FillVertex(position + new Vector3(0.0f, 0.0f, 0.1f), ref lut, 0);

            // draw back face points
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(lut[Back.v1.Index()], 0.02f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(lut[Back.v2.Index()], 0.02f);
            Gizmos.DrawSphere(lut[Back.v3.Index()], 0.02f);
            Gizmos.DrawSphere(lut[Back.v4.Index()], 0.02f);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(lut[Back.v1.Index()], lut[Back.v2.Index()]);
            Gizmos.DrawLine(lut[Back.v2.Index()], lut[Back.v3.Index()]);
            Gizmos.DrawLine(lut[Back.v3.Index()], lut[Back.v4.Index()]);
            Gizmos.DrawLine(lut[Back.v4.Index()], lut[Back.v1.Index()]);

            FillVertex(position + new Vector3(-0.1f, 0.0f, 0.0f), ref lut, 0);

            // draw left face points
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(lut[Left.v1.Index()], 0.02f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(lut[Left.v2.Index()], 0.02f);
            Gizmos.DrawSphere(lut[Left.v3.Index()], 0.02f);
            Gizmos.DrawSphere(lut[Left.v4.Index()], 0.02f);

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(lut[Left.v1.Index()], lut[Left.v2.Index()]);
            Gizmos.DrawLine(lut[Left.v2.Index()], lut[Left.v3.Index()]);
            Gizmos.DrawLine(lut[Left.v3.Index()], lut[Left.v4.Index()]);
            Gizmos.DrawLine(lut[Left.v4.Index()], lut[Left.v1.Index()]);

            FillVertex(position + new Vector3(0.1f, 0.0f, 0.0f), ref lut, 0);

            // draw right face points
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(lut[Right.v1.Index()], 0.02f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(lut[Right.v2.Index()], 0.02f);
            Gizmos.DrawSphere(lut[Right.v3.Index()], 0.02f);
            Gizmos.DrawSphere(lut[Right.v4.Index()], 0.02f);

            Gizmos.color = Color.white;
            Gizmos.DrawLine(lut[Right.v1.Index()], lut[Right.v2.Index()]);
            Gizmos.DrawLine(lut[Right.v2.Index()], lut[Right.v3.Index()]);
            Gizmos.DrawLine(lut[Right.v3.Index()], lut[Right.v4.Index()]);
            Gizmos.DrawLine(lut[Right.v4.Index()], lut[Right.v1.Index()]);

            FillVertex(position + new Vector3(0.0f, 0.1f, 0.0f), ref lut, 0);

            // draw up face points
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(lut[Up.v1.Index()], 0.02f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(lut[Up.v2.Index()], 0.02f);
            Gizmos.DrawSphere(lut[Up.v3.Index()], 0.02f);
            Gizmos.DrawSphere(lut[Up.v4.Index()], 0.02f);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(lut[Up.v1.Index()], lut[Up.v2.Index()]);
            Gizmos.DrawLine(lut[Up.v2.Index()], lut[Up.v3.Index()]);
            Gizmos.DrawLine(lut[Up.v3.Index()], lut[Up.v4.Index()]);
            Gizmos.DrawLine(lut[Up.v4.Index()], lut[Up.v1.Index()]);

            FillVertex(position + new Vector3(0.0f, -0.1f, 0.0f), ref lut, 0);

            // draw down face points
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(lut[Down.v1.Index()], 0.02f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(lut[Down.v2.Index()], 0.02f);
            Gizmos.DrawSphere(lut[Down.v3.Index()], 0.02f);
            Gizmos.DrawSphere(lut[Down.v4.Index()], 0.02f);

            Gizmos.color = Color.black;
            Gizmos.DrawLine(lut[Down.v1.Index()], lut[Down.v2.Index()]);
            Gizmos.DrawLine(lut[Down.v2.Index()], lut[Down.v3.Index()]);
            Gizmos.DrawLine(lut[Down.v3.Index()], lut[Down.v4.Index()]);
            Gizmos.DrawLine(lut[Down.v4.Index()], lut[Down.v1.Index()]);

            Gizmos.color = oldColor;
#endif
        }
    }
}
