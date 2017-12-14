using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.Net.Server.Event;
using System;

namespace xk_System.Net.Server
{
	public class NetServer : MonoBehaviour
	{
		public string ip = "192.168.122.24";
		public int port = 7878;

		NetSystem mNetSystem = null;
		Protobuf3Event mNetEventManager = null;

		public bool bInitFinish = false;
		private void Start ()
		{
			mNetSystem = new NetSystem ();
			mNetEventManager = new Protobuf3Event (mNetSystem);
			mNetSystem.initNet (ip, port);
			bInitFinish = true;
		}

		private void Update ()
		{
			mNetSystem.handleNetData ();
		}

		private void OnDestroy ()
		{
			mNetSystem.closeNet ();
		}

		public void sendNetData (int clientId,int command, object data)
		{
			mNetEventManager.sendNetData (clientId, command, data);
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