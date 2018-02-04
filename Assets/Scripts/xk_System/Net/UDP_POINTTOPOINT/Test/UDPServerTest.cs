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
		private NetServer mNetSystem = null;

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
			
			for (int i = 0; i < 100; i++) {
				GameObject obj = new GameObject ();
				obj.AddComponent<UDPClientTest> ();
				yield return 0;
			}

			bStartSendPackage = true;

			while (true) {
				if (nSendCount >= nMaxSendCount && nSendCount >= nReceiveCount) {
					DebugSystem.LogWarning ("客户端 发送接受数量： " + nSendCount + " | " + nServerReceiveCount + " | " + nReceiveCount);
					yield return 0;
				} else {
					
					yield return 0;
				}
			}
		}

		public static bool bStartSendPackage = false;
		public const int nMaxSendCount = 100000;
		public static int nSendCount = 0;
		public static int nReceiveCount = 0;
		public static int nServerReceiveCount = 0;

		private void Receive_ServerSenddata (ClientPeer peer, NetPackage package)
		{
			//DebugSystem.Log ("Server packageLength: " + package.nOrderId + "|" + package.nPackageId + " | " + package.nGroupCount + " | " + package.Length);
			csChatData mServerSendData = Protocol3Utility.getData<csChatData> (package);
			//mServerSendData.Id = peer.getPort ();
			//DebugSystem.Log ("Server: " + mServerSendData.TalkMsg);
			nServerReceiveCount++;
			peer.SendNetData (UdpNetCommand.COMMAND_TESTCHAT, mServerSendData);
		}

	}

}