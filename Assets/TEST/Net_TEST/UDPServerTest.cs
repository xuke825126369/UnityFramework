using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.Net.Server;
using xk_System.Net;
using XkProtobufData;
using System;

public class UDPServerTestObject : Singleton<UDPServerTestObject>
{
	private NetSendSystem mNetSendSystem;
	private NetReceiveSystem mNetReceiveSystem;
	private SocketSystem mNetSocketSystem;

	public UDPServerTestObject()
	{
		mNetSocketSystem = new SocketSystem_UdpServer ();
		mNetSendSystem = new NetSendSystem_Protobuf(mNetSocketSystem);
		mNetReceiveSystem = new NetReceiveSystem_Protobuf(mNetSocketSystem);
	}

	public void initNet(string ServerAddr, int ServerPort)
	{
		mNetSocketSystem.init (ServerAddr, ServerPort);
	}

	public void sendNetData(int clientId,int command, object package)
	{
		mNetSendSystem.SendNetData(clientId,command, package);  
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

public class UDPServerTest : MonoBehaviour 
{
	public string ip = "192.168.1.123";
	public int port = 7878;
	private void Start()
	{
		UDPServerTestObject.Instance.initNet(ip, port);

		UDPServerTestObject.Instance.addNetListenFun((int)ProtoCommand.ProtoChat, Receive_ServerSenddata);

		StartCoroutine (Run ());
	}


	IEnumerator Run()
	{           
		yield return new WaitForSeconds(2f);
		gameObject.AddComponent<UDPClientTest> ();
		gameObject.AddComponent<UDPClientTest> ();
	}

	// Update is called once per frame
	private void Update()
	{
		UDPServerTestObject.Instance.handleNetData();
	}

	private void OnDestroy()
	{
		UDPServerTestObject.Instance.closeNet();
	}

	private void Receive_ServerSenddata(Package package)
	{
		csChatData mServerSendData =package.getData<csChatData>();

		Debug.Log ("收到客户端发来的消息: "+ mServerSendData.ChannelId);

		scChatData mSenddata = new scChatData ();
		mSenddata.ChatInfo = new struct_ChatInfo ();
		mSenddata.ChatInfo.ChannelId = mServerSendData.ChannelId;

		UDPServerTestObject.Instance.sendNetData (package.clientId,(int)ProtoCommand.ProtoChat, mSenddata);
	}
}
