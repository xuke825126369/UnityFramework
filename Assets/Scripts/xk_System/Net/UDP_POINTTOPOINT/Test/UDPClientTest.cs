using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Google.Protobuf;
using System.IO;
using xk_System.Debug;
using xk_System.Net.UDP.POINTTOPOINT.Client;
using xk_System.Net.UDP.POINTTOPOINT.Protocol;
using UdpPointtopointProtocols;

namespace xk_System.Net.UDP.POINTTOPOINT.Test
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
			csChatData msg = new csChatData ();
			if (UnityEngine.Random.Range (1, 5) == 1) {
				msg.Id = 10000000;
				msg.TalkMsg = "127.0.0.1";	
			} else if (UnityEngine.Random.Range (1, 5) == 2) {
				byte[] aa = new byte[10];
				msg.TalkMsg = BitConverter.ToString (aa);
			} else if (UnityEngine.Random.Range (1, 5) == 1) {
				msg.TalkMsg = "127.0.0.1wefrwewqlkjef;ajfdja;ofdjas;jfd;asjdf;kajs;ldfjas;lkjf;lkasjfd;lkajsd;lkfjasd;ljfasljfd;lsa" +
				"asjfasjfd;ljas;fdj;alksjf;lkajsdflkjas;kjf;laskjfl;kasjfdlkjasd;lkjfas;lkjfl;ksadjflkasjfdsdffasjfkasdfaf" +
				"sfasdfasdfjasjf;asjfkasj;dfjsa;lkfj;kasjf;lkjsafj;asljfdasjfkjsalkfjasjfdlasjfasjdfljasldjflasjflkasjfasdjf;" +
				"ajsfajsdjfasdfj;asjfd;ajsfkjas;ljfa;sjf;asjfdlkjasdf;lja;lsjfd;asjf;lasjdfl;jas;lkdjfa;sjdf;lasjdfl;ajsdfajd" +
				"asjf;ajsdfajofjpoweijfawjeruwqeriuqwiourpoqwiurpquwt9293401923750192709174307109237490172329fsncn,xc ., cxsjdfwjf" +
				"";
			} else {
				msg.TalkMsg = "我2018年，一定会找到老婆的 !!!";
			}

			mNetSystem.sendNetData (UdpNetCommand.COMMAND_TESTCHAT, msg);
		}

		private void Receive_ServerSenddata (NetPackage package)
		{
			csChatData mServerSendData = Protocol3Utility.getData<csChatData> (package.buffer, package.Offset, package.Length);
			DebugSystem.Log ("Client: " + mServerSendData.Id + " | " + mServerSendData.TalkMsg);
			nReceiveCount++;
		}
	}
}