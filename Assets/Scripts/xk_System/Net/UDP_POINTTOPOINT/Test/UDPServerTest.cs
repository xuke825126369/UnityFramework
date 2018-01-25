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
			yield return new WaitForSeconds (1f);
			
			for (int i = 0; i < 50; i++) {
				GameObject obj = new GameObject ();
				obj.AddComponent<UDPClientTest> ();
				yield return new WaitForSeconds (0.01f);
			}

			while (nSendCount >= nMaxSendCount) {
				yield return new WaitForSeconds (1f);
				DebugSystem.Log ("客户端 发送接受数量： " + nSendCount + " | " + nReceiveCount);
			}
		}

		public static int nMaxSendCount = 2000;
		public static int nSendCount = 0;
		public static int nReceiveCount = 0;

		private void Receive_ServerSenddata (ClientPeer peer, NetPackage package)
		{
			//DebugSystem.Log ("Server packageLength: " + package.nOrderId + "|" + package.nPackageId + " | " + package.nGroupCount + " | " + package.Length);
			csChatData mServerSendData = Protocol3Utility.getData<csChatData> (package);
			//mServerSendData.Id = peer.getPort ();
			//DebugSystem.Log ("Server: " + mServerSendData.TalkMsg);
	
			peer.SendNetData (UdpNetCommand.COMMAND_TESTCHAT, mServerSendData);
		}

	}

}