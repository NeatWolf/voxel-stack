using System;
using BitStack;

namespace VoxelStack {

	/**
	 * The WorldChunk manages a number of voxel chunks, contains
	 * methods to get and modify those chunks by the system.
	 *
	 * All chunks are stored in morton z order (z order curve) and must
	 * be accessed via a MortonKey3
	 */
	public class WorldChunk {
		public const int VOXEL_CHUNKS_SIZE = 4;
		public const int VOXEL_CHUNKS = VOXEL_CHUNKS_SIZE * VOXEL_CHUNKS_SIZE * VOXEL_CHUNKS_SIZE;

		static readonly VoxelChunkEmpty EMPTY_CHUNK = new VoxelChunkEmpty();

		readonly VoxelChunk[] chunks;
		
		public WorldChunk() {
			chunks = new VoxelChunk[VOXEL_CHUNKS];
		}
		
		public VoxelChunk this[uint x, uint y, uint z] {
			get {
				return this[new MortonKey3(x, y, z)];
			}
		}
		
		public VoxelChunk this[MortonKey3 key] {
			get {
				uint lutKey = key.key;
				
				VoxelChunk chunk = chunks[lutKey];
				
				if (chunk != null) {
					return chunk;
				}
				
				VoxelChunkRenderer renderer = VoxelChunkRenderer.Pop();
				renderer.Attach(this, key);
				
				chunks[lutKey] = renderer.Chunk;
				
				return chunks[lutKey];
			}
		}
		
		public VoxelChunk GetUpStateFrom(MortonKey3 key) {
			return key.y < VOXEL_CHUNKS_SIZE - 1 ? 
			this[key.IncY()] : 
			EMPTY_CHUNK;
		}
		
		public VoxelChunk GetDownStateFrom(MortonKey3 key) {
			return key.y > 0 ?
			this[key.DecY()] :
			EMPTY_CHUNK;
		}
		
		public VoxelChunk GetRightStateFrom(MortonKey3 key) {
			return key.x < VOXEL_CHUNKS_SIZE - 1 ? 
			this[key.IncX()] :
			EMPTY_CHUNK;
		}
		
		public VoxelChunk GetLeftStateFrom(MortonKey3 key) {
			return key.x > 0 ?
			this[key.DecX()] :
			EMPTY_CHUNK;
		}
		
		public VoxelChunk GetBackStateFrom(MortonKey3 key) {
			return key.z < VOXEL_CHUNKS_SIZE - 1 ? 
			this[key.IncZ()] :
			EMPTY_CHUNK;
		}
		
		public VoxelChunk GetFrontStateFrom(MortonKey3 key) {
			return key.z > 0 ?
			this[key.DecZ()] :
			EMPTY_CHUNK;
		}
		
		/**
		 * Fill the entire world chunk with the provided Voxel
		 * data type.
		 */
		public void Fill(Voxel voxel) {
			for (uint x = 0; x < VOXEL_CHUNKS_SIZE; x++) {
				for (uint y = 0; y < VOXEL_CHUNKS_SIZE; y++) {
					for (uint z = 0; z < VOXEL_CHUNKS_SIZE; z++) {
						this[x,y,z].Fill(voxel);
					}
				} 
			}
		}
	}
}
