using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.Net;
using xk_System.Net.Client.TCP;
using XkProtobufData;
using System;
using xk_System.Net.Client;
using Google.Protobuf;
using xk_System.Net.Protocol;
using xk_System.Net.Client.Event;

public class TCPClientTest : MonoBehaviour 
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
		mNetSystem.addNetListenFun ((int)ProtoCommand.ProtoChat, Receive_ServerSenddata);
		yield return Run ();
	}

	private static int nSendCount = 0;
	private static int nReceiveCount = 0;

	IEnumerator Run()
	{           
		int TestCount = 0;
		int nMaxTestCount = 50;
		while (TestCount < nMaxTestCount) {
			for (int i = 0; i < 5; i++) {
				request_ClientSendData (1, "xuke", "I love you111111111111111111111111111111111111111111111111" +
				"111111111111111111111111111111111111111111111111111111111111111111111111111dgdgsdfshsdfh,as" +
				"mfamsfdmasdfamslmdfamsd;fmamfdamfd;amsfdwsjdfasjfasjfdkjaskfdjas;ojfd;asjdfasjdfsfasdfaksfdk" +
				"safdasdfjajfdjadjsajkf;lsdjf;alsjdf;lasjdf;lajsl;fdjalsjdfa;jsdf;aj;fjda;sjdfsjfdkjsdfkjasdf" +
				"jsfdasfsdfasdfasfdasdfasdfjkasjdfassfjpojpeoi97893472941947194y913742057");
			}
			TestCount++;
			yield return new WaitForSeconds (0.5f);
		}

		yield return new WaitForSeconds (1f);

		//if (nSendCount != TCPServerTest.nReceiveCount) {
		//	Debug.LogError ("丢包了000 ： " + nSendCount + " | " + TCPServerTest.nReceiveCount);
		//}

		if (nSendCount != nReceiveCount) {
			Debug.LogError ("丢包了111： " + nSendCount + " | " + nReceiveCount);
		}

		yield return new WaitForSeconds (10f);
		Debug.LogError ("丢包了111： " + nSendCount + " | " + nReceiveCount);

		Debug.Log ("发送完毕： " + nSendCount);
	}

	public void request_ClientSendData(uint channelId, string sendName, string content)
	{
		csChatData mClientSendData = new csChatData ();
		mClientSendData.ChannelId = channelId;
		mClientSendData.TalkMsg = content;
		mNetSystem.sendNetData ((int)ProtoCommand.ProtoChat, mClientSendData);

		nSendCount++;
	}

	private void Receive_ServerSenddata(NetPackage package)
	{
		scChatData mServerSendData = Protocol3Utility.getData<scChatData> (package.buffer, 0, package.Length);
		//Debug.Log ("Client 接受 渠道ID " + mServerSendData.ChatInfo.ChannelId);

		nReceiveCount++;
	}
}
