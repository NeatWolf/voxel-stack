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
		
		public ulong State { 
			get { 
				return state; 
			}
		}
		
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
		
		public int StateCount {
			get {
				return state.PopCount();
			}
		}

		public bool Equals(Voxel other) {
			return other.type == type && other.state == state;
		}
	}
}