
namespace VoxelLUT {
    public class Block_xxxxxx : BlockVisual {
        private readonly int[] _triangles;

        private Block_xxxxxx() {
            _triangles = new int[0];
        }

        public override int[] Triangles {
            get {
                return _triangles;
            }
        }

        public static void Create() {
            BlockLUT.Put(0x3F, new Block_xxxxxx());
        }
    }
}
