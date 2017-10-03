using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.Net;
using XkProtobufData;
using System;
using xk_System.Net.Client;
using xk_System.Net.Client.Udp;

public class UDPClientTestObject : Singleton<UDPClientTestObject>
{
	private NetSendSystem mNetSendSystem;
	private NetReceiveSystem mNetReceiveSystem;
	private SocketSystem mNetSocketSystem;

	public UDPClientTestObject()
	{
		mNetSocketSystem = new  SocketSystem_Udp ();
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

public class UDPClientTest : MonoBehaviour 
{
	public string ip = "192.168.1.123";
	public int port = 7878;
	private void Start()
	{
		UDPClientTestObject.Instance.initNet(ip, port);
		UDPClientTestObject.Instance.addNetListenFun((int)ProtoCommand.ProtoChat, Receive_ServerSenddata);

		StartCoroutine (Run ());
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
		UDPClientTestObject.Instance.handleNetData();
	}

	private void OnDestroy()
	{
		UDPClientTestObject.Instance.closeNet();
	}

	public void request_ClientSendData(uint channelId, string sendName, string content)
	{
		csChatData mClientSendData = new csChatData();
		mClientSendData.ChannelId = channelId;
		mClientSendData.TalkMsg = content;
		UDPClientTestObject.Instance.sendNetData((int)ProtoCommand.ProtoChat, mClientSendData);
	}

	private void Receive_ServerSenddata(Package package)
	{
		scChatData mServerSendData = package.getData<scChatData>();

		Debug.Log("Client 接受 渠道ID "+mServerSendData.ChatInfo.ChannelId);
	}

}
