
namespace VoxelLUT {

    /**
     * Abstract class which all block rendering types will
     * extend. Has some default helper functionality and forces
     * blocks to implement certain functions.
     */
    public abstract class BlockVisual {
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
