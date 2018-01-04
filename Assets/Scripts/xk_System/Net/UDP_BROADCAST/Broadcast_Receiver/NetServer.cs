using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace xk_System.Net.UDP.BROADCAST.Server
{
	public class NetServer : MonoBehaviour
	{
		public UInt16 port = 7878;

		ClientPeer mNetSystem = null;

		public void Init ()
		{
			mNetSystem = new ClientPeer ();
			mNetSystem.InitNet (port);
		}

		private void Update ()
		{
			if (mNetSystem != null) {
				mNetSystem.Update ();
			}
		}

		private void OnDestroy ()
		{
			mNetSystem.CloseNet ();
		}

		public void addNetListenFun (UInt16 command, Action<NetPackage> func)
		{
			mNetSystem.addNetListenFun (command, func);
		}
	}
}