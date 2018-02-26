using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelLUT;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {
        BlockLUT.Get(0);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnDrawGizmos()
    {
        BlockLUT.Get(0).OnDebug();
    }
}
