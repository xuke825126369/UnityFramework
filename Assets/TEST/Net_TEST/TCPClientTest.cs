using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.Net;
using xk_System.Net.Client.TCP;
using XkProtobufData;
using System;
using xk_System.Net.Client;

public class TCPClientTestObject
{
	private NetSendSystem mNetSendSystem;
	private NetReceiveSystem mNetReceiveSystem;
	private SocketSystem mNetSocketSystem;

	public TCPClientTestObject()
	{
		mNetSocketSystem = new  SocketSystem_Thread ();
		mNetSendSystem = new NetSendSystem(mNetSocketSystem);
		mNetReceiveSystem = new NetReceiveSystem(mNetSocketSystem);
	}

	public void initNet(string ServerAddr, int ServerPort)
	{
		mNetSocketSystem.init (ServerAddr, ServerPort);
	}

	public void sendNetData(int command, object package)
	{
		mNetSendSystem.SendNetData(command, package);  
	}

	//每帧处理一些事件
	public void handleNetData()
	{
		mNetSendSystem.HandleNetPackage ();
		mNetReceiveSystem.HandleNetPackage ();
	}

	public void addNetListenFun(int command, Action<Package> fun)
	{
		mNetReceiveSystem.addListenFun(command,fun);
	}

	public void removeNetListenFun(int command, Action<Package> fun)
	{
		mNetReceiveSystem.removeListenFun(command, fun);
	}

	public void closeNet()
	{
		mNetSocketSystem.CloseNet ();
		mNetSendSystem.Destory ();
		mNetReceiveSystem.Destory ();
	}
}

public class TCPClientTest : MonoBehaviour 
{
	public string ip = "192.168.1.123";
	public int port = 7878;

	TCPClientTestObject mClient =new TCPClientTestObject();
	private void Start()
	{
		mClient.initNet(ip, port);
		mClient.addNetListenFun((int)ProtoCommand.ProtoChat, Receive_ServerSenddata);

		StartCoroutine (Run ());
	}

	static int nReceiveCount = 0;

	IEnumerator Run()
	{           
		while (nReceiveCount<1000)
		{
			request_ClientSendData(1,"xuke","I love you");
			yield return new WaitForSeconds(1f);
		}
	}

	// Update is called once per frame
	private void Update()
	{
		mClient.handleNetData();
	}

	private void OnDestroy()
	{
		mClient.closeNet();
	}

	public void request_ClientSendData(uint channelId, string sendName, string content)
	{
		Debug.Log ("Client 发送数量: "+ ++nReceiveCount);
		csChatData mClientSendData = new csChatData();
		mClientSendData.ChannelId = channelId;
		mClientSendData.TalkMsg = content;
		mClient.sendNetData((int)ProtoCommand.ProtoChat, mClientSendData);
	}

	private void Receive_ServerSenddata(Package package)
	{
		scChatData mServerSendData = package.getData<scChatData>();
		Debug.Log("Client 接受 渠道ID "+mServerSendData.ChatInfo.ChannelId);
	}
}
