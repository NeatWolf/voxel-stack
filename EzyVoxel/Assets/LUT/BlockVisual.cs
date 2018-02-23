
namespace VoxelLUT {

    /**
     * Abstract class which all block rendering types will
     * extend. Has some default helper functionality and forces
     * blocks to implement certain functions.
     */
    public abstract class BlockVisual {

        public const int V0 = 0;
        public const int V1 = 1;
        public const int V2 = 2;
        public const int V3 = 3;

        public const int V4 = 4;
        public const int V5 = 5;
        public const int V6 = 6;
        public const int V7 = 7;

        /**
         * This represents the default voxel type, can be used for a "minecrafty"
         * look during rendering
         */
        protected static readonly int[] _DEFAULT_TRIANGLES = 
        {
            // front
            V0, V1, V2,
            V0, V2, V3,
            // back
            V4, V5, V6,
            V4, V6, V7,
            // top
            V0, V1, V4,
            V4, V1, V5,
            // bottom
            V3, V2, V6,
            V3, V6, V7,
            // left
            V4, V0, V3,
            V4, V3, V7,
            // right
            V5, V1, V2,
            V5, V2, V3
        };

        public abstract int[] Triangles { get; }

        public int Count {
            get {
                return Triangles.Length;
            }
        }

        public int this[int index] { 
            get {
                return Triangles[index];
            }
        }
    }
}
