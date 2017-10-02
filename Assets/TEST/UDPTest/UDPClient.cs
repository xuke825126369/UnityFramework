using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.Net;

public class UDPClient : MonoBehaviour {

	public string ip = "127.0.0.1";
	public int port = 7878;
	private void Start()
	{
		NetSystem.Instance.initNet(ip, port);
	}

	// Update is called once per frame
	private void Update()
	{
		NetSystem.Instance.handleNetData();
	}

	private void OnDestroy()
	{
		NetSystem.Instance.closeNet();
	}
}
