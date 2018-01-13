using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace xk_System.Net.UDP.POINTTOPOINT.Server
{
	public class NetServer : MonoBehaviour 
	{
		public UInt16 port = 7878;
		public string ip = "192.168.122.24";

		SocketUdp_Server_Basic mServer = null;

		public void Init()
		{
			mServer = new SocketUdp_Server_Basic ();
			mServer.InitNet (ip, port);
		}

		private void Update()
		{
			ClientPeerManager.Instance.Update (Time.deltaTime);
		}

		private void OnDestroy()
		{
			//ClientPeerManager.Instance.Release ();
			mServer.CloseNet ();
		}
		
	}
}