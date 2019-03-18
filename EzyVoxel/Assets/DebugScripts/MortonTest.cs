using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BitStack;

public class MortonTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
		uint x = BitMath.MortonPart3Encode(uint.MaxValue) << 0;
		uint y = BitMath.MortonPart3Encode(uint.MaxValue) << 1;
		uint z = BitMath.MortonPart3Encode(uint.MaxValue) << 2;
		
		Debug.Log("x3 bin = " + x.BitString());
		Debug.Log("y3 bin = " + y.BitString());
		Debug.Log("z3 bin = " + z.BitString());
		
		Debug.Log("x3 hex = " + x.HexString());
		Debug.Log("y3 hex = " + y.HexString());
		Debug.Log("z3 hex = " + z.HexString());
		
		uint bx = BitMath.MortonPart2Encode(uint.MaxValue) << 0;
		uint by = BitMath.MortonPart2Encode(uint.MaxValue) << 1;
		
		Debug.Log("x2 bin = " + bx.BitString());
		Debug.Log("y2 bin = " + by.BitString());
		
		Debug.Log("x2 hex = " + bx.HexString());
		Debug.Log("y2 hex = " + by.HexString());
		
		Debug.Log("Positive Morton Keys");
		
		// positive tests
		uint mortonTest = new Vector3(3,8,7).MortonKey();
		
		Debug.Log(mortonTest.DecodeMortonKey3());
		
		mortonTest = mortonTest.MortonIncX3();
		
		Debug.Log(mortonTest.DecodeMortonKey3());
		
		mortonTest = mortonTest.MortonIncZ3();
		
		Debug.Log(mortonTest.DecodeMortonKey3());
		
		mortonTest = mortonTest.MortonIncY3();
		
		Debug.Log(mortonTest.DecodeMortonKey3());
		
		Debug.Log("Negative Morton Keys");
		
		// negative tests
		mortonTest = new Vector3(9,4,6).MortonKey();
		
		Debug.Log(mortonTest.DecodeMortonKey3());
		
		mortonTest = mortonTest.MortonDecX3();
		
		Debug.Log(mortonTest.DecodeMortonKey3());
		
		mortonTest = mortonTest.MortonDecZ3();
		
		Debug.Log(mortonTest.DecodeMortonKey3());
		
		mortonTest = mortonTest.MortonDecY3();
		
		Debug.Log(mortonTest.DecodeMortonKey3());
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
