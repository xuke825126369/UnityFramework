using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using xk_System.Debug;
using xk_System.Net.UDP.POINTTOPOINT.Server;
using UdpPointtopointProtocols;
using xk_System.Net.UDP.POINTTOPOINT.Protocol;

namespace xk_System.Net.UDP.POINTTOPOINT.Test
{
	public class UDPServerTest : MonoBehaviour
	{
		NetServer mNetSystem = null;

		private void Start ()
		{
			gameObject.AddComponent<LogManager> ();
			mNetSystem = gameObject.AddComponent<NetServer> ();

			mNetSystem.Init ();
			StartCoroutine (StartTest ());
		}

		private IEnumerator StartTest ()
		{
			PackageManager.Instance.addNetListenFun (UdpNetCommand.COMMAND_TESTCHAT, Receive_ServerSenddata);
			yield return Run ();
		}

		IEnumerator Run ()
		{
			for (int i = 0; i < 1; i++) {
				GameObject obj = new GameObject ();
				obj.AddComponent<UDPClientTest> ();
				yield return new WaitForSeconds (0.01f);
			}
		}

		public static int nReceiveCount = 0;
		private void Receive_ServerSenddata (ClientPeer peer, NetPackage package)
		{
			//DebugSystem.Log ("Server packageLength: " + package.nOrderId + "|" + package.nPackageId + " | " + package.nGroupCount + " | " + package.Length);
			csChatData mServerSendData = Protocol3Utility.getData<csChatData> (package);
			//mServerSendData.Id = peer.getPort ();
			//DebugSystem.Log ("Server: " + mServerSendData.TalkMsg);

			nReceiveCount++;
			peer.SendNetData (UdpNetCommand.COMMAND_TESTCHAT, mServerSendData);
		}
	}

}