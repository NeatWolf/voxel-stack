/**
 * Class Skeleton Auto-Generated via VoxelBlockGenerator
 * CLASS Name = Block_xoxxxx
 * CLASS Hash = 0x3D
 * BlockLUT Automatically Initialises this class for LUT purposes
 */
namespace VoxelLUT {
	public class Block_xoxxxx : BlockVisual {
		// our triangles referencing pre-set vertices
		private readonly int[] _triangles;

		/**
		 * Use the private initializer to generate the triangle indices
		 */
		private Block_xoxxxx() {
			// The default triangles gives a blocky look by default
			_triangles = _DEFAULT_TRIANGLES;
		}

		/**
		 * Simple Getter class for returning the reference to the
		 * Triangles array. The Base class uses this for some pre-defined
		 * Functionality aswell.
		 */
		public override int[] Triangles {
			get {
				return _triangles;
			}
		}

		/**
		 * Invoked Dynamically and Automatically via BlockLUT
		 */
		private static void Create() {
			BlockLUT.Put(Block_xoxxxx.Hash, new Block_xoxxxx());
		}

		/**
		 * Predefined Hash code representing the LUT bucket that
		 * this object will be placed in. Unique for each block
		 */
		public static int Hash {
			get {
				return 0x3D;
			}
		}
	}
}
