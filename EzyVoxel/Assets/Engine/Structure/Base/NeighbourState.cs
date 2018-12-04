using System;
using BitStack;

namespace VoxelStack {

	/**
	 * Reference for array to index, which allows an easier usage
	 * when reading and writing sub-voxel states
	 */
#if !NET_4_6
	public struct NeighbourState : IEquatable<NeighbourState> {
#else
	public readonly struct NeighbourState : IEquatable<NeighbourState> {
#endif	
		readonly int[] _arrayRef;
		readonly int index;
		
		public NeighbourState(int[] _arrayRef, uint index) {
			#if UNITY_EDITOR || DEBUG
				if (_arrayRef == null) {
					BitDebug.Exception("NeighbourState(int[], uint) - array cannot be null");
				}
				
				if (index < 0 || index >= (_arrayRef.Length * VoxelChunk.STATES_PER_REF)) {
					BitDebug.Exception("NeighbourState(int[], uint) - index must be between 0 and " + (_arrayRef.Length * VoxelChunk.STATES_PER_REF) + " was " + index);
				}
			#endif
			
			this._arrayRef = _arrayRef;
			this.index = (int)index;
		}
		
		public NeighbourState(int[] _arrayRef, MortonKey3 index) {
			#if UNITY_EDITOR || DEBUG
				if (index.key < 0 || index.key >= (_arrayRef.Length * VoxelChunk.STATES_PER_REF)) {
					BitDebug.Exception("NeighbourState(byte[], MortonKey3) - index must be between 0 and " + (_arrayRef.Length * VoxelChunk.STATES_PER_REF) + " was " + index.key);
				}
			#endif
			
			this._arrayRef = _arrayRef;
			this.index = (int)index.key;
		}
		
		public byte Value {
			get {
				return _arrayRef.ByteAt(index);
			}
			set {
				_arrayRef.SetByteAt(value, index);
			}
		}
	
		public bool Equals(NeighbourState other) {
			return false;
		}
	}
}
