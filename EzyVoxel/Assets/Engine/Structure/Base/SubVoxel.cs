using System;
using BitStack;

namespace VoxelStack {
#if !NET_4_6
	public struct SubVoxel : IEquatable<SubVoxel>, IEquatable<ulong> {
#else
	public readonly struct SubVoxel : IEquatable<SubVoxel>, IEquatable<ulong> {
#endif
		public const ulong STATE_MAX = ulong.MaxValue;
		public const ulong STATE_ZERO = 0;
		
		// interesting sub-voxel patterns which can be used
		public const ulong BOX = STATE_MAX;
		public const ulong BOX_HALF = 0x102040810204080;
		public const ulong BOX_HALLOW = 0xFEFDFBF7EFDFBF7F;
		public const ulong BOX_ODD_FILLER = 0x173F5FEE77FAFCE8;
		public const ulong BOX_ODD_EDGE = 0xE8C0A01188050317;
		public const ulong BOX_NO_CORNERS = 0x7FBFDFEFF7FBFDFE;
		public const ulong BOX_ONLY_CORNERS = 0x8040201008040201;
		public const ulong DIAGONAL = 0x8100000000000081;
		public const ulong BOX_BOTTOM_HALF = 0xFFFF0000FFFF;
		public const ulong BOX_TOP_HALF = 0xFFFF0000FFFF0000;
		public const ulong BOX_BOTTOM_QUARTER = 0x333300003333;
		public const ulong BOX_TOP_QUARTER = 0xCCCC0000CCCC0000;
		
		readonly ulong state;
		
		public SubVoxel(ulong state) {
			this.state = state;
		}
		
		/**
		 * Access a single state from a possible of 64 states of the voxel. The
		 * states are encoded in morton z order and are not linear.
		 */
		public int this[int index] {
			get {
				#if UNITY_EDITOR || DEBUG
					if (index < 0 || index > 63) {
						BitDebug.Exception("Voxel[index] - index must be between 0 and 64 because Voxels only have 64 maximum states, was " + index);
					}
				#endif
				
				return state.BitAt(index);
			}
		}
		
		/**
		 * Access a single state from a possible of 64 states of a voxel. Each
		 * component makes up 4 units, 4 x 4 x 4 = 64 states. This method
		 * will encode the access into a morton z order.
		 */
		public int this[uint x, uint y, uint z] {
			get {
				#if UNITY_EDITOR || DEBUG
					if (x > 4 || x < 0) {
						BitDebug.Exception("Voxel(uint, uint, uint) - array key x component must be between 0-3, was " + x);
					}
					
					if (y > 4 || y < 0) {
						BitDebug.Exception("Voxel(uint, uint, uint) - array key y component must be between 0-3, was " + y);
					}
					
					if (z > 4 || z < 0) {
						BitDebug.Exception("Voxel(uint, uint, uint) - array key z component must be between 0-3, was " + z);
					}
				#endif
				return this[(int)BitMath.EncodeMortonKey(x, y, z)];
			}
		}
		
		/**
		 * Returns the number of ON bits for this particular SubVoxel
		 */
		public int Length {
			get {
				return state.PopCount();
			}
		}
		
		/**
		 * Returns the current encoded 64 bit value, representing
		 * the state of 4 x 4 x 4 SubVoxels.
		 */
		public ulong Value {
			get {
				return state;
			}
		}

		public bool Equals(SubVoxel other) {
			return state == other.state;
		}

		public bool Equals(ulong other) {
			return state == other;
		}
	}
}
