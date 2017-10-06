using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.Net.Server;
using xk_System.Net;
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
		mNetSendSystem = new NetSendSystem(mNetSocketSystem);
		mNetReceiveSystem = new NetReceiveSystem(mNetSocketSystem);
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

public class TCPServerTest : MonoBehaviour 
{
	public string ip = "192.168.1.123";
	public int port = 7878;
	private void Start()
	{
		TCPServerTestObject.Instance.initNet(ip, port);

		TCPServerTestObject.Instance.addNetListenFun((int)ProtoCommand.ProtoChat, Receive_ServerSenddata);

		StartCoroutine (Run ());
	}


	IEnumerator Run()
	{      
		yield return new WaitForSeconds(2f);
		for (int i = 0; i < 10; i++) {
			gameObject.AddComponent<TCPClientTest> ();
			yield return new WaitForSeconds(1f);
		}
	}

	// Update is called once per frame
	private void Update()
	{
		TCPServerTestObject.Instance.handleNetData();
	}

	private void OnDestroy()
	{
		Debug.LogError ("关闭服务器");
		TCPServerTestObject.Instance.closeNet();
	}

	int nReceiveCount = 0;

	private void Receive_ServerSenddata(Package package)
	{
		csChatData mServerSendData =package.getData<csChatData>();

		Debug.Log ("Server接受数量: "+ ++nReceiveCount);

		scChatData mSenddata = new scChatData ();
		mSenddata.ChatInfo = new struct_ChatInfo ();
		mSenddata.ChatInfo.ChannelId = mServerSendData.ChannelId;
			
		TCPServerTestObject.Instance.sendNetData (package.clientId,(int)ProtoCommand.ProtoChat, mSenddata);
	}
}
