using UnityEngine;
using System.Collections;
using BitStack;

namespace VoxelStack {

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
		public const uint VOXEL_INDEX_SHIFT = VOXEL_TYPE_BYTES + VOXEL_STATES_BYTES + VOXEL_NEIGHBOUR_BYTES;
		
		public const uint CHUNK_SIZE = 4;
		public const uint CHUNK_VOXELS = CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE;
		
		private byte[] state;
		
		public VoxelChunk() {
			this.state = new byte[CHUNK_VOXELS * VOXEL_INDEX_SHIFT];
		}
		
		public ushort this[uint x, uint y, uint z] {
			get {
				uint key = BitMath.EncodeMortonKey(x, y, z) * VOXEL_INDEX_SHIFT;
				
				return new ValueTuple<byte, byte>(state[key], state[key+1]).CombineToUShort();
			}
			set {
				uint key = BitMath.EncodeMortonKey(x, y, z) * VOXEL_INDEX_SHIFT;
				
				var splitValues = value.SplitIntoByte();
				
				state[key] = splitValues.Item1;
				state[key+1] = splitValues.Item2;
				
				// TO/DO
			}
		}
	}
}
