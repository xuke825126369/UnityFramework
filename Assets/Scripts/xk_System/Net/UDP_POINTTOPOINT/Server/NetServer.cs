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

		private SocketUdp_Server_Basic mServer = null;
		private Thread mThread = null;

		public void Init()
		{
			mServer = new SocketUdp_Server_Basic ();
			mServer.InitNet (ip, port);

			mThread = new Thread (sUpdate);
			mThread.Start ();
		}

		/*private void Update()
		{
			ClientPeerManager.Instance.Update (Time.deltaTime);
		}*/

		private void sUpdate()
		{
			Timer mTimer = new Timer ();
			double lastFrameTime = 0;
			while (true) {
				ClientPeerManager.Instance.Update (lastFrameTime / 1000f);

				lastFrameTime = mTimer.elapsed ();
				if (lastFrameTime < 10) {
					Thread.Sleep (10);
				}
				mTimer.restart ();
			}
		}

		private void OnDestroy()
		{
			mThread.Abort ();
			mThread = null;
			mServer.CloseNet ();

		}
	}

}