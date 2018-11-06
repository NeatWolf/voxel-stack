using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelLUT;
using BitStack;

public class Test : MonoBehaviour {

	public Vector3[] data;

	// Use this for initialization
	void Start () {
		data = new Vector3[new Vector3(3, 3, 3).MortonKey() + 1];

		for (int i = 0; i < data.Length; i++) {
			data[i] = Vector3.negativeInfinity;
		}
		for (int x = 0; x < 4; x++) {
			for (int y = 0; y < 4; y++) {
				for (int z = 0; z < 4; z++) {
					Vector3 vector = new Vector3(x, y, z);

					data[vector.MortonKey()] = vector;
				}
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnDrawGizmos()
    {
        
    }
}
