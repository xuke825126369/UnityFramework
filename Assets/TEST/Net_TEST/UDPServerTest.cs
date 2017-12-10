using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.Net.Server;
using xk_System.Net;
using XkProtobufData;
using System;
using xk_System.Net.Server.Event;
using xk_System.Net.Protocol;

public class UDPServerTestObject : Singleton<UDPServerTestObject>
{
	private NetSendSystem mNetSendSystem;
	private NetReceiveSystem mNetReceiveSystem;
	private SocketSystem mNetSocketSystem;

	public UDPServerTestObject ()
	{
		mNetSocketSystem = new SocketSystem_TCPServer ();
		mNetSendSystem = new NetSendSystem (mNetSocketSystem);
		mNetReceiveSystem = new NetReceiveSystem (mNetSocketSystem);
	}

	public void initNet (string ServerAddr, int ServerPort)
	{
		mNetSocketSystem.init (ServerAddr, ServerPort);
	}

	public void sendNetData (int socketId, int command, byte[] buffer)
	{
		mNetSendSystem.SendNetData (socketId, command, buffer);  
	}

	public void handleNetData ()
	{
		mNetSendSystem.HandleNetPackage ();
		mNetReceiveSystem.HandleNetPackage ();
	}

	public void addNetListenFun (Action<NetPackage> fun)
	{
		mNetReceiveSystem.addListenFun (fun);
	}

	public void removeNetListenFun (Action<NetPackage> fun)
	{
		mNetReceiveSystem.removeListenFun (fun);
	}

	public void closeNet ()
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

	IEnumerator Run()
	{           
		yield return new WaitForSeconds(2f);
		gameObject.AddComponent<UDPClientTest> ();
		gameObject.AddComponent<UDPClientTest> ();
	}

	NetSystem mNetSystem = null;
	Protobuf3Event mNetEventManager = null;

	private void Start ()
	{
		mNetSystem = new NetSystem ();
		mNetEventManager = new Protobuf3Event (mNetSystem);
		mNetSystem.initNet (ip, port);

		addNetListenFun((int)ProtoCommand.ProtoChat, Receive_ServerSenddata);
	}

	private void Update ()
	{
		mNetSystem.handleNetData ();
	}

	private void OnDestroy ()
	{
		mNetSystem.closeNet ();
	}

	public void sendNetData (int clientId,int command, object data)
	{
		mNetEventManager.sendNetData (clientId, command, data);
	}

	public void addNetListenFun (int command, Action<NetPackage> func)
	{
		mNetEventManager.addNetListenFun (command, func);
	}

	public void removeNetListenFun (int command, Action<NetPackage> func)
	{
		mNetEventManager.removeNetListenFun (command, func);
	}


	private void Receive_ServerSenddata(NetPackage package)
	{
		csChatData mServerSendData = Protocol3Utility.getData<csChatData>(package.buffer);

		Debug.Log ("收到客户端发来的消息: "+ mServerSendData.ChannelId);

		scChatData mSenddata = new scChatData ();
		mSenddata.ChatInfo = new struct_ChatInfo ();
		mSenddata.ChatInfo.ChannelId = mServerSendData.ChannelId;

		sendNetData (package.socketId,(int)ProtoCommand.ProtoChat, mSenddata);
	}
}
