using System;
using BitStack;

namespace VoxelStack {
	/**
	 * Provides useful methods for modifying subvoxel data
	 * structure such as setting/unsetting state
	 */
	public static class SubVoxelExtensions {
		
		/**
		 * Given an unsigned x, y, z coordinate between 0 and 4, sets the provided state
		 * and returns a new instance. NOTE this does not modify the existing value as
		 * SubVoxel value types are all read only.
		 *
		 * All values are stored in morton Z Order (Z Order Curve)
		 */
		public static SubVoxel Set(this SubVoxel data, uint x, uint y, uint z, int value) {
			#if UNITY_EDITOR || DEBUG
				if (x > 3 || x < 0) {
					BitDebug.Exception("SubVoxel.Set(uint, uint, uint, int) - morton key x component must be between 0-3, was " + x);
				}
				
				if (y > 3 || y < 0) {
					BitDebug.Exception("SubVoxel.Set(uint, uint, uint, int) - morton key y component must be between 0-3, was " + y);
				}
				
				if (z > 3 || z < 0) {
					BitDebug.Exception("SubVoxel.Set(uint, uint, uint, int) - morton key z component must be between 0-3, was " + z);
				}
				
				if (value > 1 || value < 0) {
					BitDebug.Exception("SubVoxel.Set(uint, uint, uint, int) - value must either be 1 (SubVoxel.SET) or 0 (SubVoxel.UNSET), was " + value);
				}
			#endif
			return data.Set(BitMath.EncodeMortonKey(x, y, z), value);
		}
		
		/**
		 * Given a signed x, y, z coordinate between 0 and 4, sets the provided state
		 * and returns a new instance. NOTE this does not modify the existing value as
		 * SubVoxel value types are all read only.
		 *
		 * All values are stored in morton Z Order (Z Order Curve)
		 */
		public static SubVoxel Set(this SubVoxel data, int x, int y, int z, int value) {
			#if UNITY_EDITOR || DEBUG
				if (x > 3 || x < 0) {
					BitDebug.Exception("SubVoxel.Set(int, int, int, int) - morton key x component must be between 0-3, was " + x);
				}
				
				if (y > 3 || y < 0) {
					BitDebug.Exception("SubVoxel.Set(int, int, int, int) - morton key y component must be between 0-3, was " + y);
				}
				
				if (z > 3 || z < 0) {
					BitDebug.Exception("SubVoxel.Set(int, int, int, int) - morton key z component must be between 0-3, was " + z);
				}
				
				if (value > 1 || value < 0) {
					BitDebug.Exception("SubVoxel.Set(uint, uint, uint, int) - value must either be 1 (SubVoxel.SET) or 0 (SubVoxel.UNSET) was " + value);
				}
			#endif
			return data.Set(BitMath.EncodeMortonKey((uint)x,(uint)y,(uint)z), value);
		}
		
		/**
		 * Given a compited MortonKey3, sets the provided
		 * state and returns a new instance. NOTE this does not modify the existing value as
		 * SubVoxel value types are all read only.
		 *
		 * All values are stored in morton Z Order (Z Order Curve)
		 */
		public static SubVoxel Set(this SubVoxel data, MortonKey3 mortonKey, int value) {
			#if UNITY_EDITOR || DEBUG
				if (mortonKey.key > 63) {
					BitDebug.Exception("SubVoxel.Set(MortonKey3) - morton key must be between 0-63, was " + mortonKey.key);
				}
				
				if (value > 1 || value < 0) {
					BitDebug.Exception("SubVoxel.Set(uint, uint, uint, int) - value must either be 1 (SubVoxel.SET) or 0 (SubVoxel.UNSET) was " + value);
				}
			#endif
			return data.Set(mortonKey.key, value);
		}
		
		/**
		 * Given a 32 bit (10 bit 10 bit 10 bit) component Morton Key, sets the provided
		 * state and returns a new instance. NOTE this does not modify the existing value as
		 * SubVoxel value types are all read only.
		 *
		 * All values are stored in morton Z Order (Z Order Curve)
		 */
		public static SubVoxel Set(this SubVoxel data, uint mortonKey, int value) {
			#if UNITY_EDITOR || DEBUG
				if (mortonKey > 63) {
					BitDebug.Exception("SubVoxel.Set(uint) - morton key must be between 0-63, was " + mortonKey);
				}
				
				if (value > 1 || value < 0) {
					BitDebug.Exception("SubVoxel.Set(uint, uint, uint, int) - value must either be 1 (SubVoxel.SET) or 0 (SubVoxel.UNSET) was " + value);
				}
			#endif
			ulong val = data.Value;
			
			return new SubVoxel(val.SetBit((int)mortonKey, value));
		}
	}
	
	/**
	 * SubVoxel is a 4x4x4 Data-Structure which effectively represents the 
	 * "innards" of a Voxel value type. Since a possible of 64 values can be
	 * placed, a single Voxel has a possible of 2^64 different renderable types
	 * that can be achieved.
	 *
	 * Use the in-built extension methods to modify the SubVoxel Structure. SubVoxels
	 * are lightweight value types and are read-only.
	 */
#if !NET_4_6
	public struct SubVoxel : IEquatable<SubVoxel>, IEquatable<ulong> {
#else
	public readonly struct SubVoxel : IEquatable<SubVoxel>, IEquatable<ulong> {
#endif
		// easy access to either the FULL or ZERO states of a 
		// subvoxel structure
		public const ulong STATE_FULL = ulong.MaxValue;
		public const ulong STATE_ZERO = 0;
		
		// these are safe since SubVoxel structure is readonly and cannot
		// be modified. Copies must be made same as every other calue type.
		// these are equivalent to 
		// new SubVoxel(SubVoxel.STATE_FULL) and new SubVoxel(SubVoxel.STATE_ZERO)
		public static readonly SubVoxel FULL = new SubVoxel(STATE_FULL);
		public static readonly SubVoxel ZERO = new SubVoxel(STATE_ZERO);
		
		// the only binary values which can either be set or unset
		// for every possible subvoxel value type
		public const int SET = 1;
		public const int UNSET = 0;
		
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
					if (x > 3 || x < 0) {
						BitDebug.Exception("Voxel(uint, uint, uint) - array key x component must be between 0-3, was " + x);
					}
					
					if (y > 3 || y < 0) {
						BitDebug.Exception("Voxel(uint, uint, uint) - array key y component must be between 0-3, was " + y);
					}
					
					if (z > 3 || z < 0) {
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
