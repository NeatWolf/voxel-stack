using System;
using BitStack;
using UnityEngine;

namespace VoxelStack {
	
	/**
	 * This is an emoty Voxel Chunk which can be used as a placeholder
	 * for the real thing. This is a requirement due to rendering being
	 * offset depending on the distance/field of view from the character.
	 *
	 * It is a constant variable in the WorldChunk class
	 */
	public sealed class VoxelChunkEmpty : VoxelChunk {
	
		readonly NeighbourState emptyNeighbour;
		readonly int[] emptyNeighbourState;
	
		public VoxelChunkEmpty() : base(0) {
			emptyNeighbourState = new int[1];
			emptyNeighbour = new NeighbourState(emptyNeighbourState, 0);
		}
	
		public override NeighbourState GetCurrent(MortonKey3 key) {
			// ensure to reset state as this gets modified
			emptyNeighbourState[0] = 0;
			
			return emptyNeighbour;
		}
	}
}
