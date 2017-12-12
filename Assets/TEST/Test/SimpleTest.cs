using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
		List<int> aaa = new List<int> (){ 1, 2, 3, 4, 5 };
		var bbb = aaa.ToArray ();

		bbb [0] = 100;
		Debug.Log (aaa [0]);

		aaa.AddRange(new int[]{1,2,4,5,6,6,7});
		aaa.Clear();

		Debug.Log ("Length: " + aaa.Capacity);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
