using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.Net.Server;
using xk_System.Net;
using XkProtobufData;
using System;
using xk_System.Net.Protocol;
using xk_System.Net.Server.Event;
using xk_System.Debug;

public class TCPServerTest : MonoBehaviour 
{
	NetServer mNetSystem = null;
	private void Start()
	{
		gameObject.AddComponent<LogManager> ();
		mNetSystem = gameObject.AddComponent<NetServer> ();
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
		
	IEnumerator Run()
	{    
		for (int i = 0; i < 1; i++) {
			GameObject obj = new GameObject ();
			obj.AddComponent<TCPClientTest> ();
			yield return new WaitForSeconds (1f);
		}
	}

	public static int nReceiveCount = 0;

	private void Receive_ServerSenddata(NetPackage package)
	{
		csChatData mServerSendData = Protocol3Utility.getData<csChatData> (package.buffer, 0, package.buffer.Length);

		scChatData mSenddata = new scChatData ();
		mSenddata.ChatInfo = new struct_ChatInfo ();
		mSenddata.ChatInfo.ChannelId = mServerSendData.ChannelId;
		mSenddata.ChatInfo.TalkMsg = mServerSendData.TalkMsg;
		mNetSystem.sendNetData (package.clientId, (int)ProtoCommand.ProtoChat, mSenddata);

		nReceiveCount++;
		//Debug.Log ("Server接受数量: " + mServerSendData.ChannelId + "| " + ++nReceiveCount);
	}
}


