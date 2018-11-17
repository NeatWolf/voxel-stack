using System;
using BitStack;

namespace VoxelStack {
	/**
	 * Represents a simple structure which encodes data
	 * about a single voxel value. Each voxel has a single value
	 * however contains multiple subvoxels up to 64 values.
	 *
	 * The state of the subvoxels is represented as an unsigned long (64 bits)
	 * The state of the voxel type is represented as an unsigned short (16 bits)
	 * Total memory footprit of this structure is 80 bits or 10 bytes
	 */
	public struct Voxel : IEquatable<Voxel>{
		readonly ushort type;
		readonly ulong state;
		
		public Voxel(ushort type, ulong state) {
			this.type = type;
			this.state = state;
		}
		
		public ushort Type { 
			get { 
				return type; 
			}
		}
		
		/**
		 * The states of a voxel is encoded in Morton Z Order. Ensure to encode
		 * any resulting state via morton keys.
		 */
		public ulong State { 
			get { 
				return state; 
			}
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
		 * the number of states which are enabled and are in the ON state.
		 * Uses PopCount() functionality for efficiency.
		 */
		public int StateCount {
			get {
				return state.PopCount();
			}
		}

		/**
		 * Compares Voxel values and states. Keep in mind that this will NOT
		 * compare references. Voxel types can be duplicates from different
		 * rendering chunks.
		 */
		public bool Equals(Voxel other) {
			return other.type == type && other.state == state;
		}
	}
}