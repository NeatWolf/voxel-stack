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
	public class VoxelChunk {
		public const uint SUBVOXELS_PER_VOXEL = 64;
		public const uint CHUNK_SIZE = 4;
		public const int STATES_PER_REF = 4;
		
		// subvoxel indices - these rep
		public const int SUBVOXEL_FRONT_INDEX = 0;
		public const int SUBVOXEL_BACK_INDEX = 1;
		public const int SUBVOXEL_LEFT_INDEX = 2;
		public const int SUBVOXEL_RIGHT_INDEX = 3;
		public const int SUBVOXEL_UP_INDEX = 4;
		public const int SUBVOXEL_DOWN_INDEX = 5;
		public const int SUBVOXEL_PRIMARY_INDEX = 6;
		
		public const uint CHUNK_VOXELS = CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE;
		public const uint STATES_TOTAL_LEN = (SUBVOXELS_PER_VOXEL / STATES_PER_REF) * CHUNK_VOXELS;
		
		// out of the 8 bits of data per subvoxel, we only
		// use 6. The others should be ignored for LUT purposes.
		public const int STATE_MASK = 0x3F;
		
		// the completly clean Zero State for a single subvoxel
		public const byte STATE_ZERO = 0;

		// the states which are used to figure out which 
		// neighbour data to render/draw
		readonly int[] state;

		// the voxel type, which is a 16 bit unsigned short
		readonly ushort[] types;

		// the state of the substates
		readonly ulong[] substates;
		
		int vertexNumber = -1;
		bool isDirty = true;
		
		WorldChunk parent;
		MortonKey3 localKey;

		public VoxelChunk() {
			state = new int[STATES_TOTAL_LEN];
			types = new ushort[CHUNK_VOXELS];
			substates = new ulong[CHUNK_VOXELS];
		}
		
		public VoxelChunk(int len) {
			state = new int[len];
			types = new ushort[len];
			substates = new ulong[len];
		}
		
		public void Attach(WorldChunk parent, MortonKey3 localKey) {
			this.parent = parent;
			this.localKey = localKey;
		}
		
		public void Detach() {
			parent = null;
		}
		
		public MortonKey3 LocalKey {
			get {
				return localKey;
			}
		}
		
		/**
		 * Fill the entire chunk with the provided Voxel
		 * data type.
		 */
		public void Fill(Voxel voxel) {
			for (uint x = 0; x < CHUNK_SIZE; x++) {
				for (uint y = 0; y < CHUNK_SIZE; y++) {
					for (uint z = 0; z < CHUNK_SIZE; z++) {
						this[x,y,z] = voxel;
					}
				} 
			}
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
							
							int ministate = newstate.BitAt(i);
							MortonKey3 cellOffsetKey = cellLocalKey + offsetKey;
							int mKey = (int)cellOffsetKey.key;
							
							byte originalValue = state.ByteAt(mKey);
							
							state.SetByteAt(originalValue.SetBit(SUBVOXEL_PRIMARY_INDEX, ministate), mKey);
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
							NeighbourState current = GetCurrent(cellOffsetKey);
							
							byte currentValue = current.Value;
							
							// this could be ON (inserted) or OFF (removed)
							int ministate = currentValue.BitAt(SUBVOXEL_PRIMARY_INDEX);
							
							NeighbourState front = GetFrontState(cellOffsetKey);
							NeighbourState back = GetBackState(cellOffsetKey);
							NeighbourState left = GetLeftState(cellOffsetKey);
							NeighbourState right = GetRightState(cellOffsetKey);
							NeighbourState up = GetUpState(cellOffsetKey);
							NeighbourState down = GetDownState(cellOffsetKey);
							
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
		
		public virtual NeighbourState GetCurrent(MortonKey3 key) {
			return new NeighbourState(state, key.key);
		}

		NeighbourState GetUpState(MortonKey3 key) {
			return key.y < 15 ? 
			new NeighbourState(state, key.IncY().key) : 
			parent.GetUpStateFrom(localKey).GetCurrent(new MortonKey3(key.x, 0, key.z));
		}
		
		NeighbourState GetDownState(MortonKey3 key) {
			return key.y > 0 ? 
			new NeighbourState(state, key.DecY().key) : 
			parent.GetDownStateFrom(localKey).GetCurrent(new MortonKey3(key.x, 15, key.z));
		}
		
		NeighbourState GetRightState(MortonKey3 key) {
			return key.x < 15 ? 
			new NeighbourState(state, key.IncX().key) : 
			parent.GetRightStateFrom(localKey).GetCurrent(new MortonKey3(0, key.y, key.z));
		}
		
		NeighbourState GetLeftState(MortonKey3 key) {
			return key.x > 0 ? 
			new NeighbourState(state, key.DecX().key) : 
			parent.GetLeftStateFrom(localKey).GetCurrent(new MortonKey3(15, key.y, key.z));
		}
		
		NeighbourState GetBackState(MortonKey3 key) {
			return key.z < 15 ? 
			new NeighbourState(state, key.IncZ().key) : 
			parent.GetBackStateFrom(localKey).GetCurrent(new MortonKey3(key.x, key.y, 0));
		}
		
		NeighbourState GetFrontState(MortonKey3 key) {
			return key.z > 0 ? 
			new NeighbourState(state, key.DecZ().key) : 
			parent.GetFrontStateFrom(localKey).GetCurrent(new MortonKey3(key.x, key.y, 15));
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
				
				for (int i = 0; i < STATES_TOTAL_LEN * STATES_PER_REF; i++) {
					byte voxel = (byte)(state.ByteAt(i) & STATE_MASK);
					
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
				for (int i = 0; i < STATES_TOTAL_LEN * STATES_PER_REF; i++) {
					MortonKey3 m = new MortonKey3(i);	
					byte voxel = (byte)(state.ByteAt((int)m.key) & STATE_MASK);
					
					from = MeshGenerator.FillVertices(voxel, m, 0.25f, ref newVertices, from);
				}
				
				from = 0;
				
				// fill our normals
				for (int i = 0; i < STATES_TOTAL_LEN * STATES_PER_REF; i++) {
					MortonKey3 m = new MortonKey3(i);	
					byte voxel = (byte)(state.ByteAt((int)m.key) & STATE_MASK);
					
					from = MeshGenerator.FillNormals(voxel, ref newNormals, from);
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
