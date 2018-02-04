using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Threading;

namespace xk_System.Net.UDP.POINTTOPOINT.Server
{
	public class NetServer : MonoBehaviour
	{
		public UInt16 port = 7878;
		public string ip = "192.168.122.24";

		private Thread mThread = null;

		public void Init()
		{
			SocketUdp_Server_Basic.Instance.InitNet (ip, port);

			mThread = new Thread (sUpdate);
			mThread.Start ();
		}

		private void sUpdate()
		{
			Timer mTimer = new Timer ();
			double lastFrameTime = 0;
			while (true) {
				ClientPeerManager.Instance.Update (lastFrameTime / 1000f);

				lastFrameTime = mTimer.elapsed ();
				Thread.Sleep (50);
				mTimer.restart ();
			}
		}

		private void OnDestroy()
		{
			SocketUdp_Server_Basic.Instance.CloseNet ();
			mThread.Abort ();
			mThread = null;

		}
	}

}