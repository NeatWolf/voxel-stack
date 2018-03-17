/*******************************************************************
 * Class Skeleton Auto-Generated via VoxelBlockGenerator
 * See Editor/VoxelBlockGenerator.cs for source
 * CLASS Name = Block_oxoxox
 * CLASS Hash = 0x2A
 * BlockLUT Automatically Initialises this class for LUT purposes
 *******************************************************************/
namespace VoxelLUT {
	public sealed class Block_oxoxox : BlockVisual {
		// our triangles referencing pre-set vertices
		private readonly int[] _triangles;

		/**
		 * Use the private initializer to generate the triangle indices.
		 * We use a private initializer as protection so this class does not
		 * get generated elsewhere. See Create() function for usage.
		 */
		private Block_oxoxox() {
			// The default triangles gives a blocky look by default
			// define the specific block triangles below
            _triangles = new int[] {
                Up.v1.Index(), Up.v2.Index(), Up.v3.Index(),
                Up.v1.Index(), Up.v3.Index(), Up.v4.Index(),
                Back.v1.Index(), Back.v2.Index(), Back.v3.Index(),
                Back.v1.Index(), Back.v3.Index(), Back.v4.Index(),
                Right.v1.Index(), Right.v2.Index(), Right.v3.Index(),
                Right.v1.Index(), Right.v3.Index(), Right.v4.Index(),
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
			BlockLUT.Put(Block_oxoxox.Hash, new Block_oxoxox());
		}

		/**
		 * Predefined Hash code representing the LUT bucket that
		 * this object will be placed in. Unique for each block
		 */
		public static int Hash {
			get {
				return 0x2A;
			}
		}
	}
}
