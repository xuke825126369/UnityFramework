using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace xk_System.Net.UDP.BROADCAST.Client
{
	public class NetClient: MonoBehaviour
	{
		public UInt16 port = 7878;
		public string ip = "192.168.122.24";

		ClientPeer mNetSystem = null;

		public void InitNet()
		{
			mNetSystem = new ClientPeer ();
			mNetSystem.InitNet (ip, port);
		}

		void Update()
		{
			if (mNetSystem != null) {
				mNetSystem.Update (Time.deltaTime);
			}
		}

		private void OnDestroy()
		{
			mNetSystem.CloseNet();
		}

		public void sendNetData (UInt16 nPackageId, object data)
		{
			mNetSystem.SendNetData (nPackageId, data);
		}
	}
}