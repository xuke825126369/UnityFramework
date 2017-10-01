using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.Net;

public class NetManager : MonoBehaviour
{
	public string ip = "192.168.1.109";
	public int port = 7878;
	private void Start()
	{
		NetSystem.Instance.init(ip, port);
	}

	// Update is called once per frame
	private void Update()
	{
		NetSystem.Instance.ReceiveData();
	}

	private void OnDestroy()
	{
		NetSystem.Instance.CloseNet();
	}
}
