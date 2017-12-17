using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.DataStructure;
using xk_System.Debug;

public class SimpleTest : MonoBehaviour {
	void Start ()
	{
		TimeProfiler mProfiler = new TimeProfiler ();
		mProfiler.Test (Test2);
	}

	public void Test2()
	{
		QueueArraySegment<byte> mBuffer = new QueueArraySegment<byte> (1, 10);
		for (int i = 0; i < 100; i++) {
			byte[] aaa = new byte[]{ 1, 2, 4, 5, 6, 7 };
			mBuffer.WriteFrom (aaa, 0, aaa.Length);
		}

		DebugSystem.Log (mBuffer.ToString());
	}

	public void Test1()
	{
		CircularBuffer<byte> mBuffer1 = new CircularBuffer<byte>(10);
		CircularBuffer<byte> mBuffer2 = new CircularBuffer<byte>(15);
		for (int i = 0; i < 1000; i++) {
			byte[] aaa = new byte[]{ 1, 2, 4, 5, 6, 7};
			mBuffer1.WriteFrom (aaa, 0, aaa.Length);
			//mBuffer1.Print ();
			mBuffer2.WriteFrom (mBuffer1, mBuffer1.Length);
			byte[] bbb = new byte[6];
			mBuffer2.WriteTo (0, bbb, 0, bbb.Length);
			//mBuffer2.Print ();
		}

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
