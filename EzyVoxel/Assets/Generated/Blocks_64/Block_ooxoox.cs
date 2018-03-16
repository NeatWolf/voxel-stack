/*******************************************************************
 * Class Skeleton Auto-Generated via VoxelBlockGenerator
 * See Editor/VoxelBlockGenerator.cs for source
 * CLASS Name = Block_ooxoox
 * CLASS Hash = 0x24
 * BlockLUT Automatically Initialises this class for LUT purposes
 *******************************************************************/
namespace VoxelLUT {
	public sealed class Block_ooxoox : BlockVisual {
		// our triangles referencing pre-set vertices
		private readonly int[] _triangles;

		/**
		 * Use the private initializer to generate the triangle indices.
		 * We use a private initializer as protection so this class does not
		 * get generated elsewhere. See Create() function for usage.
		 */
		private Block_ooxoox() {
			// The default triangles gives a blocky look by default
			// define the specific block triangles below
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
			BlockLUT.Put(Block_ooxoox.Hash, new Block_ooxoox());
		}

		/**
		 * Predefined Hash code representing the LUT bucket that
		 * this object will be placed in. Unique for each block
		 */
		public static int Hash {
			get {
				return 0x24;
			}
		}
	}
}
