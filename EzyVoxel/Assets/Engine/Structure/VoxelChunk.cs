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
							NeighbourState front = this[cellOffsetKey.IncZ()];
							NeighbourState back = this[cellOffsetKey.DecZ()];
							NeighbourState right = this[cellOffsetKey.IncX()];
							NeighbourState left = this[cellOffsetKey.DecX()];
							NeighbourState up = this[cellOffsetKey.IncY()];
							NeighbourState down = this[cellOffsetKey.DecY()];
							
							front.Value = front.Value.SetBit(1, ministate);
							back.Value = back.Value.SetBit(0, ministate);
							left.Value = left.Value.SetBit(3, ministate);
							right.Value = right.Value.SetBit(2, ministate);
							up.Value = up.Value.SetBit(5, ministate);
							down.Value = down.Value.SetBit(4, ministate);
						}
					}
				}
				
				// set out type and substate types for the 
				// provided index
				types[lutKey] = value.Type;
				substates[lutKey] = newstate;
				
				isDirty = true;
				vertexNumber = -1;
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
					byte voxel = state[i];
					
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
										state[i], 
										new MortonKey3(i), 
										0.25f, 
										ref newVertices, 
										from);
				}
				
				from = 0;
				
				// fill our normals
				for (int i = 0; i < STATES_TOTAL_LEN; i++) {
					from = MeshGenerator.FillNormals(
										state[i],
										ref newVertices, 
										from);
				}
				
				int len = indices.Length;
				
				// fill our indices
				for (int i = 0, j = 0; i < len; i+=6, j+=4) {
					indices[i+0] = j;
					indices[i+1] = j+1;
					indices[i+2] = j+2;
					indices[i+3] = j+1;
					indices[i+4] = j+2;
					indices[i+5] = j+3;
				}
				
				mesh.MarkDynamic();
				mesh.Clear();
				mesh.vertices = newVertices;
				mesh.normals = newNormals;
				mesh.triangles = indices;
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