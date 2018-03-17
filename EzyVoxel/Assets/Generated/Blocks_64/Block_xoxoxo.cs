/*******************************************************************
 * Class Skeleton Auto-Generated via VoxelBlockGenerator
 * See Editor/VoxelBlockGenerator.cs for source
 * CLASS Name = Block_xoxoxo
 * CLASS Hash = 0x15
 * BlockLUT Automatically Initialises this class for LUT purposes
 *******************************************************************/
namespace VoxelLUT {
	public sealed class Block_xoxoxo : BlockVisual {
		// our triangles referencing pre-set vertices
		private readonly int[] _triangles;

		/**
		 * Use the private initializer to generate the triangle indices.
		 * We use a private initializer as protection so this class does not
		 * get generated elsewhere. See Create() function for usage.
		 */
		private Block_xoxoxo() {
			// The default triangles gives a blocky look by default
			// define the specific block triangles below
            _triangles = new int[] {
                Down.v1.Index(), Down.v2.Index(), Down.v3.Index(),
                Down.v1.Index(), Down.v3.Index(), Down.v4.Index(),
                Front.v1.Index(), Front.v2.Index(), Front.v3.Index(),
                Front.v1.Index(), Front.v3.Index(), Front.v4.Index(),
                Left.v1.Index(), Left.v2.Index(), Left.v3.Index(),
                Left.v1.Index(), Left.v3.Index(), Left.v4.Index()
            };
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
			BlockLUT.Put(Block_xoxoxo.Hash, new Block_xoxoxo());
		}

		/**
		 * Predefined Hash code representing the LUT bucket that
		 * this object will be placed in. Unique for each block
		 */
		public static int Hash {
			get {
				return 0x15;
			}
		}
	}
}
