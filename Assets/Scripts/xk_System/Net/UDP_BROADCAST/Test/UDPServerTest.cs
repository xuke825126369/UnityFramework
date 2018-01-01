using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using xk_System.Debug;
using xk_System.Net.UDP.BROADCAST.Server;
using UdpBroadcastProtocols;
using xk_System.Net.UDP.BROADCAST.Protocol;

namespace xk_System.Net.UDP.BROADCAST.Test
{
	public class UDPServerTest : MonoBehaviour
	{
		NetServer mNetSystem = null;

		private void Start ()
		{
			gameObject.AddComponent<LogManager> ();
			mNetSystem = gameObject.AddComponent<NetServer> ();

			StartCoroutine (StartTest ());
			//StartCoroutine (SendBroadCast ());
		}

		private IEnumerator StartTest ()
		{
			while (!mNetSystem.bInitFinish) {
				yield return 0;
			}

			mNetSystem.addNetListenFun (UdpNetCommand.COMMAND_SCBROADCASTIP, Receive_ServerSenddata);
			yield return Run ();
		}

		IEnumerator Run ()
		{    
			for (int i = 0; i < 20; i++) {
				GameObject obj = new GameObject ();
				obj.AddComponent<UDPClientTest> ();
				yield return new WaitForSeconds (1f);
			}
		}

		IEnumerator SendBroadCast ()
		{
			while (true) {
				yield return new WaitForSeconds (1f);
				Send ();
			}
		}

		public static int nReceiveCount = 0;

		private void Send ()
		{
			scBroadcastIP msg = new scBroadcastIP ();
			msg.Ip = "127.0.0.1";

			mNetSystem.sendNetData (UdpNetCommand.COMMAND_SCBROADCASTIP, msg);
		}

		private void Receive_ServerSenddata (NetPackage package)
		{
			scBroadcastIP mServerSendData = Protocol3Utility.getData<scBroadcastIP> (package.buffer, 0, package.buffer.Length);
			DebugSystem.Log ("Server: " + mServerSendData.Ip);
			nReceiveCount++;
		}
	}
}