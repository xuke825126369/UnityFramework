using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.Net;

namespace xk_System.Net.Client
{

public class NetManager : MonoBehaviour
{
	public string ip = "192.168.1.109";
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
}