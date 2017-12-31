using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using xk_System.Debug;
using xk_System.Net.UDP.Server;
using xk_System.Net.UDP.Protocol;

public class UDPServerTest : MonoBehaviour 
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
		mNetSystem.addNetListenFun (UdpNetCommand.COMMAND_TESTCHAT, Receive_ServerSenddata);
		yield return Run ();
	}

	IEnumerator Run()
	{    
		for (int i = 0; i < 5; i++) {
			GameObject obj = new GameObject ();
			obj.AddComponent<UDPClientTest> ();
			yield return new WaitForSeconds (1f);
		}
	}

	public static int nReceiveCount = 0;

	private void Receive_ServerSenddata(NetPackage package)
	{
		TestProtocols.csChatData mServerSendData = Protocol3Utility.getData<TestProtocols.csChatData> (package.buffer, 0, package.buffer.Length);

		mNetSystem.sendNetData (package.clientId, UdpNetCommand.COMMAND_TESTCHAT, mServerSendData);

		nReceiveCount++;
		//Debug.Log ("Server接受数量: " + mServerSendData.ChannelId + "| " + ++nReceiveCount);
	}
}


