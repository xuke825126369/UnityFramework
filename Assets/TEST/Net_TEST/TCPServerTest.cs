using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.Net.Server;
using xk_System.Net;
using xk_System.Net.Client.TCP;
using XkProtobufData;
using System;

public class TCPServerTestObject : Singleton<TCPServerTestObject>
{
	private NetSendSystem mNetSendSystem;
	private NetReceiveSystem mNetReceiveSystem;
	private SocketSystem mNetSocketSystem;

	public TCPServerTestObject()
	{
		mNetSocketSystem = new SocketSystem_TCPServer ();
		mNetSendSystem = new NetSendSystem_Protobuf(mNetSocketSystem);
		mNetReceiveSystem = new NetReceiveSystem_Protobuf(mNetSocketSystem);
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

public class TCPServerTest : MonoBehaviour 
{
	public string ip = "192.168.1.123";
	public int port = 7878;
	private void Start()
	{
		TCPServerTestObject.Instance.initNet(ip, port);

		TCPServerTestObject.Instance.addNetListenFun((int)ProtoCommand.ProtoChat, Receive_ServerSenddata);
	}

	// Update is called once per frame
	private void Update()
	{
		TCPServerTestObject.Instance.handleNetData();
	}

	private void OnDestroy()
	{
		TCPServerTestObject.Instance.closeNet();
	}

	private void Receive_ServerSenddata(Package package)
	{
		csChatData mServerSendData =package.getData<csChatData>();
		TCPServerTestObject.Instance.sendNetData ((int)ProtoCommand.ProtoChat, mServerSendData);
	}

}
