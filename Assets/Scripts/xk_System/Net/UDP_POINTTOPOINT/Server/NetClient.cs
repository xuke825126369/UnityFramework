using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.Net;
using System;

namespace xk_System.Net.UDP.POINTTOPOINT.Server
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

		private void Update()
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

		public void addNetListenFun (UInt16 nPackageId, Action<NetPackage> func)
		{
			mNetSystem.addNetListenFun (nPackageId, func);
		}

		public void removeNetListenFun (UInt16 nPackageId, Action<NetPackage> func)
		{
			mNetSystem.removeNetListenFun (nPackageId, func);
		}
	}
}