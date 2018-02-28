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
        [Face(0.0f, 0.0f, 0.0f, 0.0f, 0.0f, -1.0f, 3)] v4,
        [Face(0.5f, 1.0f, 0.0f, 0.0f, 0.0f, -1.0f, 4)] v12,
        [Face(1.0f, 0.5f, 0.0f, 0.0f, 0.0f, -1.0f, 5)] v23,
        [Face(0.5f, 0.0f, 0.0f, 0.0f, 0.0f, -1.0f, 6)] v34,
        [Face(0.0f, 0.5f, 0.0f, 0.0f, 0.0f, -1.0f, 7)] v14,
        [Face(0.5f, 0.5f, 0.0f, 0.0f, 0.0f, -1.0f, 8)] vc
    }

    /**
     * Represents the back of our voxel cube
     * Contains all Voxel and Normal combinations
     * Back = +z
     */
    public enum Back {
        [Face(0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f, 9)] v1,
        [Face(1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f, 10)] v2,
        [Face(1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 1.0f, 11)] v3,
        [Face(0.0f, 1.0f, 1.0f, 0.0f, 0.0f, 1.0f, 12)] v4,
        [Face(0.5f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f, 13)] v12,
        [Face(1.0f, 0.5f, 1.0f, 0.0f, 0.0f, 1.0f, 14)] v23,
        [Face(0.5f, 1.0f, 1.0f, 0.0f, 0.0f, 1.0f, 15)] v34,
        [Face(0.0f, 0.5f, 1.0f, 0.0f, 0.0f, 1.0f, 16)] v14,
        [Face(0.5f, 0.5f, 1.0f, 0.0f, 0.0f, 1.0f, 17)] vc
    }

    /**
     * Represents the left of our voxel cube
     * Contains all Voxel and Normal combinations
     * Left = -x
     */
    public enum Left {
        [Face(0.0f, 1.0f, 1.0f, -1.0f, 0.0f, 0.0f, 18)] v1,
        [Face(0.0f, 1.0f, 0.0f, -1.0f, 0.0f, 0.0f, 19)] v2,
        [Face(0.0f, 0.0f, 0.0f, -1.0f, 0.0f, 0.0f, 20)] v3,
        [Face(0.0f, 0.0f, 1.0f, -1.0f, 0.0f, 0.0f, 21)] v4,
        [Face(0.0f, 1.0f, 0.5f, -1.0f, 0.0f, 0.0f, 22)] v12,
        [Face(0.0f, 0.5f, 0.0f, -1.0f, 0.0f, 0.0f, 23)] v23,
        [Face(0.0f, 0.0f, 0.5f, -1.0f, 0.0f, 0.0f, 24)] v34,
        [Face(0.0f, 0.5f, 1.0f, -1.0f, 0.0f, 0.0f, 25)] v14,
        [Face(0.0f, 0.5f, 0.5f, -1.0f, 0.0f, 0.0f, 26)] vc
    }

    /**
     * Represents the Right of our voxel cube
     * Contains all Voxel and Normal combinations
     * Right = +x
     */ 
    public enum Right {
        [Face(1.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 27)] v1,
        [Face(1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 28)] v2,
        [Face(1.0f, 0.0f, 1.0f, 1.0f, 0.0f, 0.0f, 29)] v3,
        [Face(1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 30)] v4,
        [Face(1.0f, 1.0f, 0.5f, 1.0f, 0.0f, 0.0f, 31)] v12,
        [Face(1.0f, 0.5f, 1.0f, 1.0f, 0.0f, 0.0f, 32)] v23,
        [Face(1.0f, 0.0f, 0.5f, 1.0f, 0.0f, 0.0f, 33)] v34,
        [Face(1.0f, 0.5f, 0.0f, 1.0f, 0.0f, 0.0f, 34)] v14,
        [Face(1.0f, 0.5f, 0.5f, 1.0f, 0.0f, 0.0f, 35)] vc
    }

    /**
     * Represents the Up of our voxel cube
     * Contains all Voxel and Normal combinations
     * Up = +y
     */
    public enum Up {
        [Face(0.0f, 1.0f, 1.0f, 0.0f, 1.0f, 0.0f, 36)] v1,
        [Face(1.0f, 1.0f, 1.0f, 0.0f, 1.0f, 0.0f, 37)] v2,
        [Face(1.0f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 38)] v3,
        [Face(0.0f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 39)] v4,
        [Face(0.5f, 1.0f, 1.0f, 0.0f, 1.0f, 0.0f, 40)] v12,
        [Face(1.0f, 1.0f, 0.5f, 0.0f, 1.0f, 0.0f, 41)] v23,
        [Face(0.5f, 1.0f, 0.0f, 0.0f, 1.0f, 0.0f, 42)] v34,
        [Face(0.0f, 1.0f, 0.5f, 0.0f, 1.0f, 0.0f, 43)] v14,
        [Face(0.5f, 1.0f, 0.5f, 0.0f, 1.0f, 0.0f, 44)] vc
    }

    /**
     * Represents the Down of our voxel cube
     * Contains all Voxel and Normal combinations
     * Down = -y
     */
    public enum Down {
        [Face(0.0f, 0.0f, 0.0f, 0.0f, -1.0f, 0.0f, 45)] v1,
        [Face(1.0f, 0.0f, 0.0f, 0.0f, -1.0f, 0.0f, 46)] v2,
        [Face(1.0f, 0.0f, 1.0f, 0.0f, -1.0f, 0.0f, 47)] v3,
        [Face(0.0f, 0.0f, 1.0f, 0.0f, -1.0f, 0.0f, 48)] v4,
        [Face(0.5f, 0.0f, 0.0f, 0.0f, -1.0f, 0.0f, 49)] v12,
        [Face(1.0f, 0.0f, 0.5f, 0.0f, -1.0f, 0.0f, 50)] v23,
        [Face(0.5f, 0.0f, 1.0f, 0.0f, -1.0f, 0.0f, 51)] v34,
        [Face(0.0f, 0.0f, 0.5f, 0.0f, -1.0f, 0.0f, 52)] v14,
        [Face(0.5f, 0.0f, 0.5f, 0.0f, -1.0f, 0.0f, 53)] vc
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