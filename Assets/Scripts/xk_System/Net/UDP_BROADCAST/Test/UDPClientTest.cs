using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Google.Protobuf;
using System.IO;
using UdpBroadcastProtocols;
using xk_System.Net.UDP.BROADCAST.Protocol;
using xk_System.Debug;
using xk_System.Net.UDP.BROADCAST.Client;

namespace xk_System.Net.UDP.BROADCAST.Test
{
	public class UDPClientTest : MonoBehaviour
	{
		NetClient mNetSystem = null;

		private void Start ()
		{
			mNetSystem = gameObject.AddComponent<NetClient> ();
			mNetSystem.InitNet ();

			StartCoroutine (SendBroadCast ());
		}

		private static int nSendCount = 0;
		private static int nReceiveCount = 0;

		IEnumerator SendBroadCast ()
		{
			while (true) {
				yield return new WaitForSeconds (1f);
				Send ();
			}
		}

		private void Send ()
		{
			scBroadcastIP msg = new scBroadcastIP ();
			msg.Ip = "127.0.0.1";

			mNetSystem.sendNetData (UdpNetCommand.COMMAND_SCBROADCASTIP, msg);
		}

		private void Receive_ServerSenddata (NetReceivePackage package)
		{
			scBroadcastIP mServerSendData = Protocol3Utility.getData<scBroadcastIP> (package.buffer.Array, package.buffer.Offset, package.buffer.Count);
			DebugSystem.Log ("Client: " + mServerSendData.Ip);
			nReceiveCount++;
		}
	}
}