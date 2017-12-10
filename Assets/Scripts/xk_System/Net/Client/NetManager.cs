﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.Net;
using System;
using xk_System.Net.Client.Event;

namespace xk_System.Net.Client
{
	public class NetManager : SingleTonMonoBehaviour<NetManager>
	{
		public string ip = "192.168.1.109";
		public int port = 7878;

		NetSystem mNetSystem = null;
		Protobuf3Event mNetEventManager = null;
		private void Start()
		{
			mNetSystem = new NetSystem ();
			mNetEventManager = new Protobuf3Event (mNetSystem);
			mNetSystem.initNet(ip, port);
		}

		private void Update()
		{
			mNetSystem.handleNetData();
		}

		private void OnDestroy()
		{
			mNetSystem.closeNet();
		}

		public void sendNetData (int command, object data)
		{
			mNetEventManager.sendNetData (command, data);
		}

		public void addNetListenFun (int command, Action<NetPackage> func)
		{
			mNetEventManager.addNetListenFun (command, func);
		}

		public void removeNetListenFun (int command, Action<NetPackage> func)
		{
			mNetEventManager.removeNetListenFun (command, func);
		}
	}
}