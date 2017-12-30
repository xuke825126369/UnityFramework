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

		public void sendNetData (int command, object data)
		{
			mNetSystem.sendNetData (command, data);
		}

		public void addNetListenFun (int command, Action<NetPackage> func)
		{
			mNetSystem.addNetListenFun (command, func);
		}

		public void removeNetListenFun (int command, Action<NetPackage> func)
		{
			mNetSystem.removeNetListenFun (command, func);
		}
	}
}