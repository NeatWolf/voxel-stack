
namespace VoxelLUT {
    public class Block_oooooo : BlockVisual {
        private readonly int[] _triangles;

        private Block_oooooo() {
            _triangles = new int[0];
        }

        public override int[] Triangles {
            get {
                return _triangles;
            }
        }

        public static void Create() {
            BlockLUT.Put(0x0, new Block_oooooo());
        }
    }
}
