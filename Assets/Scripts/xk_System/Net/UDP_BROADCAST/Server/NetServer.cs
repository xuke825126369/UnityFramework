using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace xk_System.Net.UDP.BROADCAST.Server
{
	public class NetServer : MonoBehaviour
	{
		public string ip = "192.168.122.24";
		public UInt16 port = 7878;

		ClientPeer mNetSystem = null;
		public bool bInitFinish = false;
		private void Start ()
		{
			mNetSystem = new ClientPeer ();
			mNetSystem.InitNet (ip, port);
			bInitFinish = true;
		}

		private void Update ()
		{
			mNetSystem.Update ();
		}

		private void OnDestroy ()
		{
			mNetSystem.CloseNet ();
		}

		public void sendNetData (UInt16 command, object data)
		{
			mNetSystem.SendNetData (command, data);
		}

		public void addNetListenFun (UInt16 command, Action<NetPackage> func)
		{
			mNetSystem.addNetListenFun (command, func);
		}
	}
}