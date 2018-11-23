﻿using UnityEngine;
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
		public const int SUBVOXEL_PRIMARY_INDEX = 6;
		
		public const uint STATES_TOTAL_LEN = SUBVOXELS_PER_VOXEL * CHUNK_VOXELS;
		public const uint CHUNK_VOXELS = CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE;
		
		public const int STATE_MASK = 0x3F;

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
				uint lutKey = key.Key;
				
				return new Voxel(types[lutKey], substates[lutKey]);
			}
			set {
				MortonKey3 key = new MortonKey3(x, y, z);
			
				uint lutKey = key.Key;
				
				ulong oldstate = substates[lutKey];
				ulong newstate = value.State.Value;
				
				types[lutKey] = value.Type;
				substates[lutKey] = newstate;
				
				// we need to re-generate our structure if and only if
				// the provided bits for the cell has changed
				// for efficiency, if only a single cell has changed, then we
				// update only that cell, saving on precious performance.
				if ((oldstate ^ newstate) != 0) {
					// we will only be re-generating the bits which have
					// been changed by the user. This change will be reflected
					// in the final rendering
					MortonKey3 offsetKey = new MortonKey3(x * 4, y * 4, z * 4);
					
					// first pass, we set the switch for our subvoxels so the next
					// pass is working with the correct data.
					// since we are working with values only on this cell, it means that
					// this operation has zero branching operations.
					for (int i = 0; i < 64; i++) {
							MortonKey3 cellLocalKey = new MortonKey3(i);
							
							byte ministate = (byte)newstate.BitAt(i);
							MortonKey3 cellOffsetKey = cellLocalKey + offsetKey;
							uint mKey = cellOffsetKey.Key;
							
							// set the state of the cell, if it was enabled or disabled
							state[mKey] = state[mKey].SetBit(SUBVOXEL_PRIMARY_INDEX, ministate);
					}
					
					// second pass, we modify the LUT values so everything renders
					// correctly.
					for (int i = 0; i < 64; i++) {
						// execute only for the bits that have changed
						MortonKey3 cellLocalKey = new MortonKey3(i);
						MortonKey3 cellOffsetKey = cellLocalKey + offsetKey;
						
						// our morton keys for the neighbouring cells
						NeighbourState current = this[cellOffsetKey];
						
						byte currentValue = current.Value;
						
						// this could be ON (inserted) or OFF (removed)
						byte ministate_inv = (byte)currentValue.BitAt(SUBVOXEL_PRIMARY_INDEX);
						byte ministate = (byte)(1 - ministate_inv);
						
						// FRONT - Neighbour state can be from another cell group
						NeighbourState front = this[cellOffsetKey.DecZ()];
						byte frontValue = front.Value;
						int frontPrim = frontValue.BitAt(SUBVOXEL_PRIMARY_INDEX);
						byte frontPrimIn = (byte)(frontPrim ^ ministate_inv);
						byte frontPrimOu = (byte)(frontPrim ^ ministate_inv);
						currentValue = currentValue.SetBit(0, frontPrimIn);
						front.Value = frontValue.SetBit(1, frontPrimOu);
						
						// BACK - Neighbour state can be from another cell group
						NeighbourState back = this[cellOffsetKey.IncZ()];
						byte backValue = back.Value;
						int backPrim = backValue.BitAt(SUBVOXEL_PRIMARY_INDEX);
						byte backPrimIn = (byte)(backPrim ^ ministate_inv);
						byte backPrimOu = (byte)(backPrim ^ ministate_inv);
						currentValue = currentValue.SetBit(1, backPrimIn);
						back.Value = backValue.SetBit(0, backPrimOu);
						
						// LEFT - Neighbour state can be from another cell group
						NeighbourState left = this[cellOffsetKey.DecX()];
						byte leftValue = left.Value;
						int leftPrim = leftValue.BitAt(SUBVOXEL_PRIMARY_INDEX);
						byte leftPrimIn = (byte)(leftPrim ^ ministate_inv);
						byte leftPrimOu = (byte)(leftPrim ^ ministate_inv);
						currentValue = currentValue.SetBit(2, leftPrimIn);
						left.Value = leftValue.SetBit(3, leftPrimOu);
						
						// RIGHT- Neighbour state can be from another cell group
						NeighbourState right = this[cellOffsetKey.IncX()];
						byte rightValue = right.Value;
						int rightPrim = rightValue.BitAt(SUBVOXEL_PRIMARY_INDEX);
						byte rightPrimIn = (byte)(rightPrim ^ ministate_inv);
						byte rightPrimOu = (byte)(rightPrim ^ ministate_inv);
						currentValue = currentValue.SetBit(3, rightPrimIn);
						right.Value = rightValue.SetBit(2, rightPrimOu);
						
						// UP - Neighbour state can be from another cell group
						NeighbourState up = this[cellOffsetKey.IncY()];
						byte upValue = up.Value;
						int upPrim = upValue.BitAt(SUBVOXEL_PRIMARY_INDEX);
						byte upPrimIn = (byte)(upPrim ^ ministate_inv);
						byte upPrimOu = (byte)(upPrim ^ ministate_inv);
						currentValue = currentValue.SetBit(4, upPrimIn);
						up.Value = upValue.SetBit(5, upPrimOu);
						
						// DOWN - Neighbour state can be from another cell group
						NeighbourState down = this[cellOffsetKey.DecY()];
						byte downValue = down.Value;
						int downPrim = downValue.BitAt(SUBVOXEL_PRIMARY_INDEX);
						byte downPrimIn = (byte)(downPrim ^ ministate_inv);
						byte downPrimOu = (byte)(downPrim ^ ministate_inv);
						currentValue = currentValue.SetBit(5, downPrimIn);
						down.Value = downValue.SetBit(4, downPrimOu);
						
						// write our current value
						current.Value = currentValue;
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
				return new NeighbourState(state, key.Key);
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
