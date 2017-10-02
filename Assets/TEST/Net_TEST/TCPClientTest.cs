using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.Net.Server;
using xk_System.Net;
using xk_System.Net.Client.TCP;
using XkProtobufData;
using System;

public class TCPClientTestObject : Singleton<TCPClientTestObject>
{
	private NetSendSystem mNetSendSystem;
	private NetReceiveSystem mNetReceiveSystem;
	private SocketSystem mNetSocketSystem;

	public TCPClientTestObject()
	{
		mNetSocketSystem = new SocketSevice ();
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

public class TCPClientTest : MonoBehaviour 
{
	public string ip = "192.168.1.123";
	public int port = 7878;
	private void Start()
	{
		TCPClientTestObject.Instance.initNet(ip, port);
		TCPClientTestObject.Instance.addNetListenFun((int)ProtoCommand.ProtoChat, Receive_ServerSenddata);
	}

	IEnumerator Run()
	{           
		while (true)
		{
			request_ClientSendData(1,"xuke","I love you");
			yield return new WaitForSeconds(2f);
		}
	}

	// Update is called once per frame
	private void Update()
	{
		TCPClientTestObject.Instance.handleNetData();
	}

	private void OnDestroy()
	{
		TCPClientTestObject.Instance.closeNet();
	}

	public void request_ClientSendData(uint channelId, string sendName, string content)
	{
		csChatData mClientSendData = new csChatData();
		mClientSendData.ChannelId = channelId;
		mClientSendData.TalkMsg = content;
		TCPClientTestObject.Instance.sendNetData((int)ProtoCommand.ProtoChat, mClientSendData);
	}

	private void Receive_ServerSenddata(Package package)
	{
		scChatData mServerSendData =package.getData<scChatData>();
		Debug.Log("Client 接受 渠道ID "+mServerSendData.ChatInfo.ChannelId);
	}

}
