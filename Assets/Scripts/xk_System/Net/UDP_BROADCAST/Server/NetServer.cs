using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace xk_System.Net.UDP.BROADCAST.Server
{
	public class NetServer : MonoBehaviour
	{
		public string ip = "192.168.122.24";
		public int port = 7878;

		NetSystem mNetSystem = null;
		public bool bInitFinish = false;
		private void Start ()
		{
			mNetSystem = new NetSystem ();
			mNetSystem.initNet (port);
			bInitFinish = true;
		}

		private void Update ()
		{
			mNetSystem.Update ();
		}

		private void OnDestroy ()
		{
			mNetSystem.closeNet ();
		}

		public void sendNetData (int clientId,int command, object data)
		{
			mNetSystem.sendNetData (clientId, command, data);
		}

		public void addNetListenFun (int command, Action<NetPackage> func)
		{
			mNetSystem.addNetListenFun (command, func);
		}
	}
}