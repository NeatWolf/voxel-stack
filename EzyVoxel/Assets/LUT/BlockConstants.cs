using UnityEngine;
using System.Collections;
using System;

namespace VoxelLUT {
    /**
     * The Enum LUT attribute for a single face section, with a predefined
     * position and orientation
     */
    public sealed class FaceAttribute : Attribute {
        private readonly Vector3 _pos;
        private readonly Vector3 _nor;
        private readonly int _index;

        internal FaceAttribute(float _px,
                               float _py,
                               float _pz,
                               float _nx,
                               float _ny,
                               float _nz,
                               int index) {
            this._pos = new Vector3(_px, _py, _pz);
            this._nor = new Vector3(_nx, _ny, _nz);
            this._index = index;
        }

        public Vector3 Vertex { get { return _pos; } }
        public Vector3 Normal { get { return _nor; } }
        public int Index { get { return _index; } }
    }

    /**
     * Represents the front of our voxel cube
     * Contains all Vertex and Normal combinations
     * Front = -z
     */
    public enum Front {
        [Face(0.0f, 1.0f, 0.0f, 0.0f, 0.0f, -1.0f, 0)] v1,
        [Face(1.0f, 1.0f, 0.0f, 0.0f, 0.0f, -1.0f, 1)] v2,
        [Face(1.0f, 0.0f, 0.0f, 0.0f, 0.0f, -1.0f, 2)] v3,
        [Face(0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -1.0f, 3)] v4
    }

    /**
     * Represents the back of our voxel cube
     * Contains all Voxel and Normal combinations
     * Back = +z
     */
    public enum Back {
        [Face(0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f, 4)] v1,
        [Face(1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f, 5)] v2,
        [Face(1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 1.0f, 6)] v3,
        [Face(0.0f, 1.0f, 1.0f, 0.0f, 0.0f, 1.0f, 7)] v4
    }

    /**
     * Represents the left of our voxel cube
     * Contains all Voxel and Normal combinations
     * Left = -x
     */
    public enum Left {
        [Face(0.0f, 1.0f, 1.0f, -1.0f, 0.0f, 0.0f, 8)] v1,
        [Face(0.0f, 1.0f, 0.0f, -1.0f, 0.0f, 0.0f, 9)] v2,
        [Face(0.0f, 0.0f, 0.0f, -1.0f, 0.0f, 0.0f, 10)] v3,
        [Face(0.0f, 0.0f, 1.0f, -1.0f, 0.0f, 0.0f, 11)] v4
    }

    /**
     * Represents the Right of our voxel cube
     * Contains all Voxel and Normal combinations
     * Right = +x
     */ 
    public enum Right {
        [Face(1.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 12)] v1,
        [Face(1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 13)] v2,
        [Face(1.0f, 0.0f, 1.0f, 1.0f, 0.0f, 0.0f, 14)] v3,
        [Face(1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 15)] v4
    }

    /**
     * Represents the Up of our voxel cube
     * Contains all Voxel and Normal combinations
     * Up = +y
     */
    public enum Up {
        [Face(0.0f, 1.0f, 1.0f, 0.0f, 1.0f, 0.0f, 16)] v1,
        [Face(1.0f, 1.0f, 1.0f, 0.0f, 1.0f, 0.0f, 17)] v2,
        [Face(1.0f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 18)] v3,
        [Face(0.0f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 19)] v4
    }

    /**
     * Represents the Down of our voxel cube
     * Contains all Voxel and Normal combinations
     * Down = -y
     */
    public enum Down {
        [Face(0.0f, 0.0f, 0.0f, 0.0f, -1.0f, 0.0f, 20)] v1,
        [Face(1.0f, 0.0f, 0.0f, 0.0f, -1.0f, 0.0f, 21)] v2,
        [Face(1.0f, 0.0f, 1.0f, 0.0f, -1.0f, 0.0f, 22)] v3,
        [Face(0.0f, 0.0f, 1.0f, 0.0f, -1.0f, 0.0f, 23)] v4
    }

    /**
     * Defines Extension methods for all our enumerators
     */
    public static class FaceExtensions {
        public static void Add(this Front p, ref Vector3[] _POS, ref Vector3[] _NOR) {
            FaceAttribute face = GetAttr(p);

            _POS[face.Index] = face.Vertex;
            _NOR[face.Index] = face.Normal;
        }

        public static void Add(this Back p, ref Vector3[] _POS, ref Vector3[] _NOR) {
            FaceAttribute face = GetAttr(p);

            _POS[face.Index] = face.Vertex;
            _NOR[face.Index] = face.Normal;
        }

        public static void Add(this Left p, ref Vector3[] _POS, ref Vector3[] _NOR) {
            FaceAttribute face = GetAttr(p);

            _POS[face.Index] = face.Vertex;
            _NOR[face.Index] = face.Normal;
        }

        public static void Add(this Right p, ref Vector3[] _POS, ref Vector3[] _NOR) {
            FaceAttribute face = GetAttr(p);

            _POS[face.Index] = face.Vertex;
            _NOR[face.Index] = face.Normal;
        }

        public static void Add(this Up p, ref Vector3[] _POS, ref Vector3[] _NOR) {
            FaceAttribute face = GetAttr(p);

            _POS[face.Index] = face.Vertex;
            _NOR[face.Index] = face.Normal;
        }

        public static void Add(this Down p, ref Vector3[] _POS, ref Vector3[] _NOR) {
            FaceAttribute face = GetAttr(p);

            _POS[face.Index] = face.Vertex;
            _NOR[face.Index] = face.Normal;
        }

        public static int Index(this Front p) {
            FaceAttribute face = GetAttr(p);

            return face.Index;
        }

        public static int Index(this Back p) {
            FaceAttribute face = GetAttr(p);

            return face.Index;
        }

        public static int Index(this Left p) {
            FaceAttribute face = GetAttr(p);

            return face.Index;
        }

        public static int Index(this Right p) {
            FaceAttribute face = GetAttr(p);

            return face.Index;
        }

        public static int Index(this Up p) {
            FaceAttribute face = GetAttr(p);

            return face.Index;
        }

        public static int Index(this Down p) {
            FaceAttribute face = GetAttr(p);

            return face.Index;
        }

        private static FaceAttribute GetAttr(Front p) {
            return (FaceAttribute)Attribute.GetCustomAttribute(ForValue(p), typeof(FaceAttribute));
        }

        private static FaceAttribute GetAttr(Back p) {
            return (FaceAttribute)Attribute.GetCustomAttribute(ForValue(p), typeof(FaceAttribute));
        }

        private static FaceAttribute GetAttr(Left p) {
            return (FaceAttribute)Attribute.GetCustomAttribute(ForValue(p), typeof(FaceAttribute));
        }

        private static FaceAttribute GetAttr(Right p) {
            return (FaceAttribute)Attribute.GetCustomAttribute(ForValue(p), typeof(FaceAttribute));
        }

        private static FaceAttribute GetAttr(Up p) {
            return (FaceAttribute)Attribute.GetCustomAttribute(ForValue(p), typeof(FaceAttribute));
        }

        private static FaceAttribute GetAttr(Down p) {
            return (FaceAttribute)Attribute.GetCustomAttribute(ForValue(p), typeof(FaceAttribute));
        }

        private static System.Reflection.MemberInfo ForValue(Front p) {
            return typeof(Front).GetField(Enum.GetName(typeof(Front), p));
        }

        private static System.Reflection.MemberInfo ForValue(Back p) {
            return typeof(Back).GetField(Enum.GetName(typeof(Back), p));
        }

        private static System.Reflection.MemberInfo ForValue(Left p) {
            return typeof(Left).GetField(Enum.GetName(typeof(Left), p));
        }

        private static System.Reflection.MemberInfo ForValue(Right p) {
            return typeof(Right).GetField(Enum.GetName(typeof(Right), p));
        }

        private static System.Reflection.MemberInfo ForValue(Up p) {
            return typeof(Up).GetField(Enum.GetName(typeof(Up), p));
        }

        private static System.Reflection.MemberInfo ForValue(Down p) {
            return typeof(Down).GetField(Enum.GetName(typeof(Down), p));
        }
    }
}