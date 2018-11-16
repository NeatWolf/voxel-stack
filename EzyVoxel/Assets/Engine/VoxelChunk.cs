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
		public const uint SUBVOXELS_PER_VOXEL = 64;
		public const uint STATES_TOTAL_LEN = SUBVOXELS_PER_VOXEL * CHUNK_VOXELS;
		
		public const uint CHUNK_SIZE = 4;
		public const uint CHUNK_VOXELS = CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE;

		// the states which are used to figure out which 
		// neighbour data to render/draw
		readonly byte[] state;

		// the voxel type, which is a 16 bit unsigned short
		readonly ushort[] types;

		// the state of the substates
		readonly ulong[] substates;

		public VoxelChunk() {
			state = new byte[STATES_TOTAL_LEN];
			types = new ushort[CHUNK_VOXELS];
			substates = new ulong[CHUNK_VOXELS];
		}
		
		/**
		 * This functionality will modify internal states for the
		 * set() operation. The modification is a requirement for proper
		 * rendering.
		 * Access via local x,y,z coordinates
		 */
		public Voxel this[uint x, uint y, uint z] {
			get {
				MortonKey3 key = new MortonKey3(x, y, z);
				uint lutKey = key.Key;
				
				return new Voxel(types[lutKey], substates[lutKey]);
			}
			set {
				MortonKey3 key = new MortonKey3(x, y, z);
			
				uint lutKey = key.Key;
				
				ulong substate = substates[lutKey];
				ulong newstate = value.State;
				
				ulong differences = substate ^ newstate;
				
				// we need to re-generate our structure if and only if
				// the provided bits for the cell has changed
				if (differences != 0) {
				
					// we will only be re-generating the bits which have
					// been changed by the user. This change will be reflected
					// in the final rendering
					MortonKey3 offsetKey = new MortonKey3(x * 4, y * 4, z * 4);
					
					for (int i = 0; i < 64; i++) {
						// execute only for the bits that have changed
						if (differences.BitAt(i) == 1) {
							// this could be ON (inserted) or OFF (removed)
							byte ministate = (byte)newstate.BitAt(i);
							
							MortonKey3 cellLocalKey = new MortonKey3(i);
							MortonKey3 cellOffsetKey = cellLocalKey + offsetKey;
							
							// our morton keys for the neighbouring cells
							MortonKey3 front = new MortonKey3(cellOffsetKey.Key);
							MortonKey3 back = new MortonKey3(cellOffsetKey.Key);
							MortonKey3 left = new MortonKey3(cellOffsetKey.Key);
							MortonKey3 right = new MortonKey3(cellOffsetKey.Key);
							MortonKey3 up = new MortonKey3(cellOffsetKey.Key);
							MortonKey3 down = new MortonKey3(cellOffsetKey.Key);
							
							front.IncZ();
							back.DecZ();
							left.DecX();
							right.IncX();
							up.IncY();
							down.DecY();
							
							byte frontv = state[front.Key];
							byte backv = state[back.Key];
							byte leftv = state[left.Key];
							byte rightv = state[right.Key];
							byte upv = state[up.Key];
							byte downv = state[down.Key];
							
							// set all neighbouring states for all subvoxel types
							state[front.Key] = frontv.SetBit(1, ministate);
							state[back.Key] = backv.SetBit(0, ministate);
							state[left.Key] = leftv.SetBit(3, ministate);
							state[right.Key] = rightv.SetBit(2, ministate);
							state[up.Key] = upv.SetBit(5, ministate);
							state[down.Key] = downv.SetBit(4, ministate);
						}
					}
				}
				
				// set out type and substate types for the 
				// provided index
				types[lutKey] = value.Type;
				substates[lutKey] = newstate;
			}
		}
		
		public NeighbourState this[MortonKey3 key] {
			get {
				return new NeighbourState(null, 0);
			}
		}
	}
}
