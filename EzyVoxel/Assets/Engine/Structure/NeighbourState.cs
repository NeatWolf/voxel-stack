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
		readonly byte[] _arrayRef;
		readonly uint index;
		
		public NeighbourState(ref byte[] _arrayRef, uint index) {
			#if UNITY_EDITOR || DEBUG
				if (_arrayRef == null) {
					BitDebug.Exception("NeighbourState(byte[], uint) - array cannot be null");
				}
				
				if (index < 0 || index >= _arrayRef.Length) {
					BitDebug.Exception("NeighbourState(byte[], uint) - index must be between 0 and " + _arrayRef.Length + " was " + index);
				}
			#endif
			
			this._arrayRef = _arrayRef;
			this.index = index;
		}
		
		public NeighbourState(ref byte[] _arrayRef, MortonKey3 index) {
			#if UNITY_EDITOR || DEBUG
				if (index.Key < 0 || index.Key >= _arrayRef.Length) {
					BitDebug.Exception("NeighbourState(byte[], MortonKey3) - index must be between 0 and " + _arrayRef.Length + " was " + index.Key);
				}
			#endif
			
			this._arrayRef = _arrayRef;
			this.index = index.Key;
		}
		
		public byte Value {
			get {
				return _arrayRef[index];
			}
			set {
				_arrayRef[index] = value;
			}
		}
	
		public bool Equals(NeighbourState other) {
			return false;
		}
	}
}
