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
		yield return Run();
	}

	static int nReceiveCount = 0;

	IEnumerator Run()
	{           
		while (nReceiveCount < 100) {
			for (int i = 0; i < 1; i++) {
				request_ClientSendData (1, "xuke", "I love you");
			}
			yield return new WaitForSeconds (1f);
		}
	}

	public void request_ClientSendData(uint channelId, string sendName, string content)
	{
		csChatData mClientSendData = new csChatData ();
		mClientSendData.ChannelId = channelId;
		mClientSendData.TalkMsg = content;
		mNetSystem.sendNetData ((int)ProtoCommand.ProtoChat, mClientSendData);
	}

	private void Receive_ServerSenddata(NetPackage package)
	{
		scChatData mServerSendData = Protocol3Utility.getData<scChatData> (package.buffer, 0, package.Length);
		Debug.Log ("Client 接受 渠道ID " + mServerSendData.ChatInfo.ChannelId);
	}
}
