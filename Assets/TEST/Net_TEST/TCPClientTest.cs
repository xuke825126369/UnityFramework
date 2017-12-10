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

public class TCPClientTestObject:NetEventInterface
{
	private NetSendSystem mNetSendSystem;
	private NetReceiveSystem mNetReceiveSystem;
	private SocketSystem mNetSocketSystem;

	public TCPClientTestObject()
	{
		mNetSocketSystem = new SocketSystem_1 ();
		mNetSendSystem = new NetSendSystem (mNetSocketSystem);
		mNetReceiveSystem = new NetReceiveSystem (mNetSocketSystem);
	}

	public void initNet(string ServerAddr, int ServerPort)
	{
		mNetSocketSystem.init (ServerAddr, ServerPort);
	}

	public void sendNetData(int command, byte[] buffer)
	{
		NetPackage mPackage = NetObjectPool.Instance.mNetPackagePool.Pop();
		mPackage.command = command;
		mPackage.buffer = buffer;
		mNetSendSystem.SendNetData(mPackage);  
	}

	public void Update()
	{
		mNetSocketSystem.Update ();
		mNetSendSystem.HandleNetPackage ();
		mNetReceiveSystem.HandleNetPackage ();
	}

	public void addNetListenFun(Action<NetPackage> fun)
	{
		mNetReceiveSystem.addListenFun(fun);
	}

	public void removeNetListenFun(Action<NetPackage> fun)
	{
		mNetReceiveSystem.removeListenFun(fun);
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
	public string ip = "192.168.122.24";
	public int port = 7878;

	TCPClientTestObject mNetSystem = null;
	Protobuf3Event mNetEventManager = null;
	private void Start()
	{
		mNetSystem = new TCPClientTestObject ();
		mNetEventManager = new Protobuf3Event (mNetSystem);
		mNetSystem.initNet(ip, port);

		StartTest ();
	}

	private void Update()
	{
		mNetSystem.Update();
	}

	private void OnDestroy()
	{
		mNetSystem.closeNet();
	}

	public void sendNetData (int command, object data)
	{
		mNetEventManager.sendNetData (command, data);
	}

	public void addNetListenFun (int command, Action<NetPackage> func)
	{
		mNetEventManager.addNetListenFun (command, func);
	}

	public void removeNetListenFun (int command, Action<NetPackage> func)
	{
		mNetEventManager.removeNetListenFun (command, func);
	}

	private void StartTest()
	{
		mNetEventManager.addNetListenFun ((int)ProtoCommand.ProtoChat, Receive_ServerSenddata);
		StartCoroutine (Run ());
	}

	static int nReceiveCount = 0;

	IEnumerator Run()
	{           
		while (nReceiveCount < 100) {
			for (int i = 0; i < 10; i++) {
				request_ClientSendData (1, "xuke", "I love you");
			}
			yield return new WaitForSeconds (1f);
		}
	}

	public void request_ClientSendData(uint channelId, string sendName, string content)
	{
		//Debug.Log ("Client 发送数量: "+ ++nReceiveCount);
		csChatData mClientSendData = new csChatData();
		mClientSendData.ChannelId = channelId;
		mClientSendData.TalkMsg = content;
		sendNetData((int)ProtoCommand.ProtoChat, mClientSendData);
	}

	private void Receive_ServerSenddata(NetPackage package)
	{
		scChatData mServerSendData = Protocol3Utility.getData<scChatData> (package.buffer);
		Debug.Log ("Client 接受 渠道ID " + mServerSendData.ChatInfo.ChannelId);
	}
}
