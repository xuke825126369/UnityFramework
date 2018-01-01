using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.Net;
using System;

namespace xk_System.Net.UDP.POINTTOPOINT.Client
{
	public class NetClient: MonoBehaviour
	{
		public UInt16 port = 7878;
		public string ip = "127.0.0.1";

		NetSystem mNetSystem = null;

		private void InitNet()
		{
			mNetSystem = new NetSystem ();
			mNetSystem.InitNet (ip, port);
		}

		private void Update(double elapsed)
		{
			if (mNetSystem != null) {
				mNetSystem.Update (elapsed);
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

		public void addNetListenFun (UInt16 nPackageId, Action<NetReceivePackage> func)
		{
			mNetSystem.addNetListenFun (nPackageId, func);
		}

		public void removeNetListenFun (UInt16 nPackageId, Action<NetReceivePackage> func)
		{
			mNetSystem.removeNetListenFun (nPackageId, func);
		}
	}
}