/*******************************************************************
 * Class Skeleton Auto-Generated via VoxelBlockGenerator
 * See Editor/VoxelBlockGenerator.cs for source
 * CLASS Name = Block_oxxxxo
 * CLASS Hash = 0x1E
 * BlockLUT Automatically Initialises this class for LUT purposes
 *******************************************************************/
namespace VoxelLUT {
	public sealed class Block_oxxxxo : BlockVisual {
		// our triangles referencing pre-set vertices
		private readonly int[] _triangles;

		/**
		 * Use the private initializer to generate the triangle indices.
		 * We use a private initializer as protection so this class does not
		 * get generated elsewhere. See Create() function for usage.
		 */
		private Block_oxxxxo() {
			// The default triangles gives a blocky look by default
			// define the specific block triangles below
			_triangles = new int[] {
				Right.v1.Index(), Right.v2.Index(), Right.v3.Index(),
				Right.v1.Index(), Right.v3.Index(), Right.v4.Index(),
				Front.v1.Index(), Front.v2.Index(), Front.v3.Index(),
				Front.v1.Index(), Front.v3.Index(), Front.v4.Index(),
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
			BlockLUT.Put(Block_oxxxxo.Hash, new Block_oxxxxo());
		}

		/**
		 * Predefined Hash code representing the LUT bucket that
		 * this object will be placed in. Unique for each block
		 */
		public static int Hash {
			get {
				return 0x1E;
			}
		}
	}
}
