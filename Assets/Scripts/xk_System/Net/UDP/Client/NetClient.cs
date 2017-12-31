using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.Net;
using System;

namespace xk_System.Net.UDP.Client
{
	public class NetClient: MonoBehaviour
	{
		public UInt16 port = 7878;

		NetSystem mNetSystem = null;

		public bool bInitFinish = false;
		private void Start()
		{
			mNetSystem = new NetSystem ();
			mNetSystem.InitNet (port);
			bInitFinish = true;
		}

		private void Update()
		{
			mNetSystem.Update();
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