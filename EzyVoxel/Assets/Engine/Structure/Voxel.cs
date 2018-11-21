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
#if !NET_4_6
	public struct Voxel : IEquatable<Voxel> {
#else 
	public readonly struct Voxel : IEquatable<Voxel> {
#endif
		readonly ushort type;
		readonly SubVoxel state;
		
		public Voxel(ushort type, ulong state) {
			this.type = type;
			this.state = new SubVoxel(state);
		}
		
		public Voxel(ushort type, SubVoxel state) {
			this.type = type;
			this.state = state;
		}
		
		/**
		 * Returns the type (value) of the current Voxel.
		 * All SubVoxels share the same value.
		 */
		public ushort Type { 
			get { 
				return type; 
			}
		}
		
		/**
		 * The states of a voxel is encoded in Morton Z Order. Ensure to encode
		 * any resulting state via morton keys.
		 */
		public SubVoxel State { 
			get { 
				return state; 
			}
		}

		/**
		 * Compares Voxel values and states. Keep in mind that this will NOT
		 * compare references. Voxel types can be duplicates from different
		 * rendering chunks.
		 */
		public bool Equals(Voxel other) {
			return other.type == type && other.state.Equals(state);
		}
	}
}