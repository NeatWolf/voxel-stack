using UnityEngine;
using System.Collections;
using BitStack;
using VoxelStackLUT;

#if UNITY_EDITOR || DEBUG
	using VoxelStackDebug;
#endif

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
		public const uint CHUNK_SIZE = 4;
		
		// subvoxel indices - these rep
		public const int SUBVOXEL_FRONT_INDEX = 0;
		public const int SUBVOXEL_BACK_INDEX = 1;
		public const int SUBVOXEL_LEFT_INDEX = 2;
		public const int SUBVOXEL_RIGHT_INDEX = 3;
		public const int SUBVOXEL_UP_INDEX = 4;
		public const int SUBVOXEL_DOWN_INDEX = 5;
		public const int SUBVOXEL_PRIMARY_INDEX = 6;
		
		public const uint CHUNK_VOXELS = CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE;
		public const uint STATES_TOTAL_LEN = (SUBVOXELS_PER_VOXEL / 4) * CHUNK_VOXELS;
		
		// out of the 8 bits of data per subvoxel, we only
		// use 6. The others should be ignored for LUT purposes.
		public const int STATE_MASK = 0x3F;
		
		// the completly clean Zero State for a single subvoxel
		public const byte STATE_ZERO = 0;

		// the states which are used to figure out which 
		// neighbour data to render/draw
		readonly byte[] state;

		// the voxel type, which is a 16 bit unsigned short
		readonly ushort[] types;

		// the state of the substates
		readonly ulong[] substates;
		
		int vertexNumber = -1;
		bool isDirty = true;

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
				uint lutKey = key.key;
				
				return new Voxel(types[lutKey], substates[lutKey]);
			}
			set {
				MortonKey3 key = new MortonKey3(x, y, z);
			
				uint lutKey = key.key;
				
				ulong oldstate = substates[lutKey];
				ulong newstate = value.State.Value;
				
				ulong differences = oldstate ^ newstate;
				
				types[lutKey] = value.Type;
				substates[lutKey] = newstate;
				
				// we need to re-generate our structure if and only if
				// the provided bits for the cell has changed
				// for efficiency, if only a single cell has changed, then we
				// update only that cell, saving on precious performance.
				if (differences != 0) {
					// we will only be re-generating the bits which have
					// been changed by the user. This change will be reflected
					// in the final rendering
					MortonKey3 offsetKey = new MortonKey3(x * 4, y * 4, z * 4);
					
					// first pass, we set the switch for our subvoxels so the next
					// pass is working with the correct data.
					// since we are working with values only on this cell, it means that
					// this operation has only one branching operation (to check if cell
					// has changed).
					for (int i = 0; i < 64; i++) {
						if (differences.BitAt(i) == 1) {
							MortonKey3 cellLocalKey = new MortonKey3(i);
							
							byte ministate = (byte)newstate.BitAt(i);
							MortonKey3 cellOffsetKey = cellLocalKey + offsetKey;
							uint mKey = cellOffsetKey.key;
							
							// set the state of the cell, if it was enabled or disabled
							state[mKey] = state[mKey].SetBit(SUBVOXEL_PRIMARY_INDEX, ministate);
						}
					}
					
					// second pass, we modify the LUT values so everything renders
					// correctly.
					for (int i = 0; i < 64; i++) {
						// branch op to ensure that only the cells that have changed
						// will proceed
						if (differences.BitAt(i) == 1) {
							// execute only for the bits that have changed
							MortonKey3 cellLocalKey = new MortonKey3(i);
							MortonKey3 cellOffsetKey = cellLocalKey + offsetKey;
							
							// our morton keys for the neighbouring cells
							NeighbourState current = this[cellOffsetKey];
							
							byte currentValue = current.Value;
							
							// this could be ON (inserted) or OFF (removed)
							int ministate = currentValue.BitAt(SUBVOXEL_PRIMARY_INDEX);
							
							NeighbourState front = this[cellOffsetKey.DecZ()];
							NeighbourState back = this[cellOffsetKey.IncZ()];
							NeighbourState left = this[cellOffsetKey.DecX()];
							NeighbourState right = this[cellOffsetKey.IncX()];
							NeighbourState up = this[cellOffsetKey.IncY()];
							NeighbourState down = this[cellOffsetKey.DecY()];
							
							// FRONT - Neighbour state can be from another cell group
							byte frontValue = front.Value;
							int frontState = frontValue.BitAt(SUBVOXEL_PRIMARY_INDEX);
							int frontCross = ministate ^ frontState;
							
							// BACK - Neighbour state can be from another cell group
							byte backValue = back.Value;
							int backState = backValue.BitAt(SUBVOXEL_PRIMARY_INDEX);
							int backCross = ministate ^ backState;
							
							// LEFT - Neighbour state can be from another cell group
							byte leftValue = left.Value;
							int leftState = leftValue.BitAt(SUBVOXEL_PRIMARY_INDEX);
							int leftCross = ministate ^ leftState;
							
							// RIGHT- Neighbour state can be from another cell group
							byte rightValue = right.Value;
							int rightState = rightValue.BitAt(SUBVOXEL_PRIMARY_INDEX);
							int rightCross = ministate ^ rightState;
							
							// UP - Neighbour state can be from another cell group
							byte upValue = up.Value;
							int upState = upValue.BitAt(SUBVOXEL_PRIMARY_INDEX);
							int upCross = ministate ^ upState;
							
							// DOWN - Neighbour state can be from another cell group
							byte downValue = down.Value;
							int downState = downValue.BitAt(SUBVOXEL_PRIMARY_INDEX);
							int downCross = ministate ^ downState;
							
							// set all the neighbouring data states - these are used for rendering
							front.Value = frontValue.SetBit(SUBVOXEL_BACK_INDEX, frontState & frontCross);
							back.Value = backValue.SetBit(SUBVOXEL_FRONT_INDEX, backState & backCross);
							right.Value = rightValue.SetBit(SUBVOXEL_LEFT_INDEX, rightState & rightCross);
							left.Value = leftValue.SetBit(SUBVOXEL_RIGHT_INDEX, leftState & leftCross);
							up.Value = upValue.SetBit(SUBVOXEL_DOWN_INDEX, upState & upCross);
							down.Value = downValue.SetBit(SUBVOXEL_UP_INDEX, downState & downCross);
							
							// set all the current subvoxel data states - these are used for rendering
							currentValue = currentValue.SetBit(SUBVOXEL_FRONT_INDEX, ministate & frontCross);
							currentValue = currentValue.SetBit(SUBVOXEL_BACK_INDEX, ministate & backCross);
							currentValue = currentValue.SetBit(SUBVOXEL_LEFT_INDEX, ministate & leftCross);
							currentValue = currentValue.SetBit(SUBVOXEL_RIGHT_INDEX, ministate & rightCross);
							currentValue = currentValue.SetBit(SUBVOXEL_UP_INDEX, ministate & upCross);
							currentValue = currentValue.SetBit(SUBVOXEL_DOWN_INDEX, ministate & downCross);
							
							// write our current value
							current.Value = currentValue;
						}
					}
					
					isDirty = true;
					vertexNumber = -1;
				}
			}
		}
		
		/**
		 * Given a morton key, grab a neighbouring state which can be updated
		 * for new rendering
		 */
		NeighbourState this[MortonKey3 key] {
			get {
				uint subkey = key.key;
				
				if (subkey < STATES_TOTAL_LEN) {
					return new NeighbourState(state, subkey);
				}
				
				return new NeighbourState();
			}
		}
		
		/**
		 * Performs a count of all visible vertices using the generated
		 * LUT table and it's vertices index.
		 * The final LUT is unpredictable and can be customized, hence why
		 * a fresh count is required each time.
		 */
		public int VertexCount {
			get {
				// no need to re-count if nothing has changed in
				// the internal state of the chunk
				if (vertexNumber > -1) {
					return vertexNumber;
				}
				
				int[] indices = GeneratedVoxelTable.IndexTable;
				
				int count = 0;
				
				for (int i = 0; i < STATES_TOTAL_LEN; i++) {
					byte voxel = (byte)(state[i] & STATE_MASK);
					
					count += indices[voxel + 1] - indices[voxel];
				}
				
				vertexNumber = count;
				
				return vertexNumber;
			}
		}
		
		/**
		 * NOTICE - This functionality should onoy be used in editor
		 * or debug mode. Use the compute shaders for rendering voxels
		 * properly.
		 */
		#if UNITY_EDITOR || DEBUG
			public void FillMesh(Mesh mesh) {
				Vector3[] newVertices = new Vector3[VertexCount];
				Vector3[] newNormals = new Vector3[VertexCount];
				int[] indices = new int[(VertexCount / 4) * 6];
				
				int from = 0;
				
				// fill our vertices
				for (int i = 0; i < STATES_TOTAL_LEN; i++) {
					from = MeshGenerator.FillVertices(
										(byte)(state[i] & STATE_MASK), 
										new MortonKey3(i), 
										0.25f, 
										ref newVertices, 
										from);
				}
				
				from = 0;
				
				// fill our normals
				for (int i = 0; i < STATES_TOTAL_LEN; i++) {
					from = MeshGenerator.FillNormals(
										(byte)(state[i] & STATE_MASK),
										ref newNormals, 
										from);
				}
				
				int len = indices.Length;
				
				// fill our indices
				for (int i = 0, j = 0; i < len; i+=6, j+=4) {
					indices[i+0] = j;
					indices[i+1] = j+1;
					indices[i+2] = j+2;
					indices[i+3] = j;
					indices[i+4] = j+2;
					indices[i+5] = j+3;
				}
				
				mesh.Clear();
				mesh.vertices = newVertices;
				mesh.normals = newNormals;
				mesh.triangles = indices;
				mesh.MarkDynamic();
			}
		#endif
		
		/**
		 * Every time this chunk is updated, the dirty flag is set
		 */
		public bool IsDirty {
			get {
				return isDirty;
			}
			set {
				isDirty = value;
			}
		}
	}
}
