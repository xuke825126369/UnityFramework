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
using System.Text;

namespace xk_System.Net.UDP.POINTTOPOINT.Test
{
	public class UDPClientTest : MonoBehaviour
	{
		NetClient mNetSystem = null;

		private void Start ()
		{
			mNetSystem = gameObject.AddComponent<NetClient> ();
			mNetSystem.InitNet ();
			mNetSystem.addNetListenFun (UdpNetCommand.COMMAND_TESTCHAT, Receive_ServerSenddata);
			StartCoroutine (SendBroadCast ());
		}

		private static int nSendCount = 0;
		private static int nReceiveCount = 0;

		IEnumerator SendBroadCast ()
		{
			while (true) {
				yield return 0;
				if (nSendCount >= 1000) {
					break;
				}
				Send ();
			}

			DebugSystem.Log ("服务器接受的数量：" + UDPServerTest.nReceiveCount);
			DebugSystem.Log ("客户端 发送接受数量： " + nSendCount + " | " + nReceiveCount);

			while (nSendCount > nReceiveCount) {
				yield return new WaitForSeconds (1f);
				DebugSystem.Log ("服务器接受的数量：" + UDPServerTest.nReceiveCount);
				DebugSystem.Log ("客户端 发送接受数量： " + nSendCount + " | " + nReceiveCount);
			}
		}

		private void Send ()
		{
			csChatData msg = new csChatData ();
			if (UnityEngine.Random.Range (1, 1) == 2) {
				msg.Id = 10000000;
				msg.TalkMsg = "127.0.0.1";	
			} else if (UnityEngine.Random.Range (1, 3) == 1) {
				byte[] aa = new byte[2048];
				msg.TalkMsg = BitConverter.ToString (aa);
			} else if (UnityEngine.Random.Range (1, 5) == 30) {
				msg.TalkMsg = "我2018年，一定会找到老婆的 !!!";
			} else {
				msg.TalkMsg = "127.0.0.1wefrwewqlkjef;ajfdja;ofdjas;jfd;asjdf;kajs;ldfjas;lkjf;lkasjfd;lkajsd;lkfjasd;ljfasljfd;lsa" +
				"asjfasjfd;ljas;fdj;alksjf;lkajsdflkjas;kjf;laskjfl;kasjfdlkjasd;lkjfas;lkjfl;ksadjflkasjfdsdffasjfkasdfaf" +
				"sfasdfasdfjasjf;asjfkasj;dfjsa;lkfj;kasjf;lkjsafj;asljfdasjfkjsalkfjasjfdlasjfasjdfljasldjflasjflkasjfasdjf;" +
				"ajsfajsdjfasdfj;asjfd;ajsfkjas;ljfa;sjf;asjfdlkjasdf;lja;lsjfd;asjf;lasjdfl;jas;lkdjfa;sjdf;lasjdfl;ajsdfajd" +
				"asjf;ajsdfajofjpoweijfawjeruwqeriuqwiourpoqwiurpquwt9293401923750192709174307109237490172329fsncn,xc ., cxsjdfwjf" +
				"sfdsfsfsdfdasfdjaj[qwje[roqj[wfje[awjf[jaw[fejqa[wejfk[j[jv[qj03rqd发士大夫士大夫盎司附近啊觉得 十分大师傅 是放大上来就放大" +
				"阿三打飞机啊是十分的氨基酸的佛哦就【完全【偶尔【二十九发【阿飞【就发的【撒酒疯【怕时间地方【欧舒丹普法考试辅导卡的斯洛伐克死灵法师的发生的分厘卡士大夫看" +
				"士大夫阿克苏的罚款数量的反抗拉萨；离开收费道路开辟【文科i软破i人品oooeeeeeee发d士大夫随风倒就是格式发给老师开发公司健康的" +
				"";
			}

			mNetSystem.sendNetData (UdpNetCommand.COMMAND_TESTCHAT, msg);
			nSendCount++;
		}

		private void Receive_ServerSenddata (NetPackage package)
		{
			//DebugSystem.Log ("Client packageLength: " + package.Length);
			csChatData mServerSendData = Protocol3Utility.getData<csChatData> (package);
			//DebugSystem.Log ("Client: " + mServerSendData.Id + " | " + mServerSendData.TalkMsg);
			nReceiveCount++;

		}
	}
}