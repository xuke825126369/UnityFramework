using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.DataStructure;
using xk_System.Debug;
using System;

public class SimpleTest : MonoBehaviour {
	void Start ()
	{
		Test5 ();
	}

	class AAA
	{
		public string aaa;

		public AAA(string aa)
		{
			this.aaa = aa;
		}
	}

	private void Test5()
	{
		Dictionary<int,AAA> mDic = new Dictionary<int, AAA> ();
		mDic [1] = new AAA ("1213e23");
		mDic [2] = new AAA ("23232r23r");

		var iter = mDic.GetEnumerator ();
		while (iter.MoveNext ()) {
			iter.Current.Value.aaa = "werwrwerweerwer";

			DebugSystem.Log (iter.Current.Value.aaa);
		}



	}

	System.Timers.Timer tm = null;
	private void Test4()
	{
		tm = new System.Timers.Timer ();
		tm.Interval = 3000.0;
		tm.AutoReset = true;

		tm.Start ();
		tm.Elapsed += (object sender, System.Timers.ElapsedEventArgs args) => {
			Debug.Log ("定时器：" + args.SignalTime.ToString ());
			tm.Stop();
		};


	}

	private void OnDestroy()
	{
		if (tm != null)
			tm.Stop ();
	}

	public void Test3()
	{
		UInt16 whoId = 2;
		UInt16 nOrderId = 1456;

		UInt32 result = (UInt32)((whoId) << 16);
		result |= (UInt32)nOrderId;

		whoId = (UInt16)(result >> 16);
		//nOrderId = (UInt16)(result << 16 >> 16);
		nOrderId = (UInt16)(result & 0x0000FFFF); 

		DebugSystem.Log ("result: " + result);
		DebugSystem.Log ("whoId: " + whoId);
		DebugSystem.Log ("nOrderId: " + nOrderId);
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
