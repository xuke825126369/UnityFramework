using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class EventManager : MonoBehaviour
{
	private Dictionary<int, Action<object>> mDic = new Dictionary<int, Action<object>>();

	private void Update()
	{

	}

}
