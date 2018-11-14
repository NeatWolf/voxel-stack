using UnityEngine;
using System.Collections;
using BitStack;

namespace VoxelStack {
	
	/**
	 * Represents a simple structure which encodes data
	 * about a single voxel value. Each voxel has a single value
	 * however contains multiple subvoxels up to 64 values.
	 *
	 * The state of the subvoxels is represented as an unsigned long (64 bits)
	 */
	public struct Voxel {
		private readonly ushort type;
		private readonly ulong state;
		
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
				return state.BitAt(index);
			}
		}
		
		public int StateCount {
			get {
				return state.PopCount();
			}
		}
		
		public int IsEnabled {
			get {
				return state > 0 ? 1 : 0;
			}
		}
	}
	
	/**
	 * Each Voxel Chunk has the following characteristics
	 * - Each Chunk contains
	 * - - 4 x 4 x 4 Voxels = 64 Voxels in Total
	 * - - 1 x 32 bits for the morton key
	 *
	 * - Each Voxel contains
	 * - - 1 x 16 bits for the Voxel Type
	 * - - 1 x 64 bits for the Voxel States
	 * - - 64 x 8 bits for the Voxel Neighbour States
	 */
	public sealed class VoxelChunk {
		public const uint VOXEL_TYPE_BYTES = 16 / 8;
		public const uint VOXEL_STATES_BYTES = 64 / 8;
		public const uint VOXEL_NEIGHBOUR_BYTES = 64;
		
		public const uint CHUNK_SIZE = 4;
		public const uint CHUNK_VOXELS = CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE;
		
		public const uint CHUNK_SHIFT = VOXEL_TYPE_BYTES * CHUNK_VOXELS;
		
		private byte[] state;
		private ulong[] voxelSubstates;
		
		public VoxelChunk() {
			this.state = new byte[CHUNK_VOXELS + (VOXEL_TYPE_BYTES * CHUNK_VOXELS)];
			this.voxelSubstates = new ulong[CHUNK_VOXELS];
		}
		
		public Voxel this[uint x, uint y, uint z] {
			get {
				uint key = BitMath.EncodeMortonKey(x, y, z) + (VOXEL_TYPE_BYTES - 1);
				
				return new Voxel(new ValueTuple<byte, byte>(state[key - 1], state[key]).CombineToUShort(), voxelSubstates[key]);
			}
			set {
				uint key = BitMath.EncodeMortonKey(x, y, z) + (VOXEL_TYPE_BYTES - 1);
				
				var splitValues = value.Type.SplitIntoByte();
				
				state[key - 1] = splitValues.Item1;
				state[key] = splitValues.Item2;
				
				voxelSubstates[key] = value.State;
				
				uint mortonKeyFront = BitMath.EncodeMortonKey(x, y, z - 1);
				uint mortonKeyBack = BitMath.EncodeMortonKey(x, y, z + 1);
				uint mortonKeyLeft = BitMath.EncodeMortonKey(x - 1, y, z);
				uint mortonKeyRight = BitMath.EncodeMortonKey(x + 1, y, z);
				uint mortonKeyUp = BitMath.EncodeMortonKey(x, y + 1, z);
				uint mortonKeyDown = BitMath.EncodeMortonKey(x, y - 1, z);
				
				
			}
		}
	}
}
