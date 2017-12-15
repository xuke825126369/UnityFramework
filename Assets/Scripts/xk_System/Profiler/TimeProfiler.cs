using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TimeProfiler 
{
	public void Test(Action testFun)
	{
		DateTime lastTIme = System.DateTime.Now;
		testFun ();
		DateTime nowTIme = System.DateTime.Now;

		TimeSpan mTimeSpan = nowTIme - lastTIme;
		double useTIme = mTimeSpan.TotalMilliseconds / 1000;
		Debug.Log ("总用时间(毫秒)：" + useTIme);
	}
}
