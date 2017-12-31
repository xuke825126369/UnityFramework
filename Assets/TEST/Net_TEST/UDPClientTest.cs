using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Google.Protobuf;
using System.IO;
using xk_System.Net.UDP.Client;
using xk_System.Net.UDP.Protocol;
using xk_System.Net.UDP;

public class UDPClientTest : MonoBehaviour 
{
	NetClient mNetSystem = null;
	private void Start()
	{
		mNetSystem = gameObject.AddComponent<NetClient> ();
		StartCoroutine (StartTest ());
	}

	private IEnumerator StartTest()
	{
		while (!mNetSystem.bInitFinish) {
			yield return 0;
		}
		mNetSystem.addNetListenFun (UdpNetCommand.COMMAND_TESTCHAT, Receive_ServerSenddata);
		yield return Run ();
	}

	private static int nSendCount = 0;
	private static int nReceiveCount = 0;

	IEnumerator Run()
	{           
		int TestCount = 0;
		int nMaxTestCount = 53;
		while (TestCount < nMaxTestCount) {
			for (int i = 0; i < 53; i++) {
				request_ClientSendData (1, "xuke", "I love you111111111111111111111111111111111111111111111111" +
					"111111111111111111111111111111111111111111111111111111111111111111111111111dgdgsdfshsdfh,as" +
					"mfamsfdmasdfamslmdfamsd;fmamfdamfd;amsfdwsjdfasjfasjfdkjaskfdjas;ojfd;asjdfasjdfsfasdfaksfdk" +
					"safdasdfjajfdjadjsajkf;lsdjf;alsjdf;lasjdf;lajsl;fdjalsjdfa;jsdf;aj;fjda;sjdfsjfdkjsdfkjasdf" +
					"jsfdasfsdfasdfasfdasdfasdfjkasjdfassfjpojpeoi97893472941947194y913742057sdfasfdasdfasdfsfasdfasfdasd" +
					"sfasfdasdfasdfasfddddddddddddddddddddddddddddddddddddddddddddddddddfasdfasdfasdfasdfasdfsdfasdfasfda" +
					"sfsfsfsdfsd09035923-940592394523096-548623489510948920384*((*&^&%^$%$$%#sfsfd");
			}
			TestCount++;
			yield return new WaitForSeconds (0.5f);
		}

		if (nSendCount != nReceiveCount) {
			Debug.LogError ("丢包了111： " + nSendCount + " | " + nReceiveCount);
		}

		while (nSendCount != nReceiveCount) {
			yield return new WaitForSeconds (1f);
			Debug.LogError ("丢包了111： " + nSendCount + " | " + nReceiveCount + " | " + TCPServerTest.nReceiveCount);
		}

		Debug.Log ("发送完毕： " + nSendCount);
	}

	public void request_ClientSendData(uint channelId, string sendName, string content)
	{
		TestProtocols.csChatData mClientSendData = new TestProtocols.csChatData ();
		mClientSendData.Id = channelId;
		mClientSendData.TalkMsg = content;
		mNetSystem.sendNetData (UdpNetCommand.COMMAND_TESTCHAT, mClientSendData);

		nSendCount++;
	}

	private void Receive_ServerSenddata(NetReceivePackage package)
	{
		TestProtocols.csChatData mServerSendData = Protocol3Utility.getData<TestProtocols.csChatData> (package.buffer.Array, package.buffer.Offset, package.buffer.Count);
		//Debug.Log ("Client 接受 渠道ID " + mServerSendData.ChatInfo.ChannelId);
		nReceiveCount++;
	}
}
