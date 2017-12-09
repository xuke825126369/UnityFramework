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

		NetSystem mNetSystem = null;
		private void Start()
		{
			mNetSystem = new NetSystem ();
			mNetSystem.initNet(ip, port);
		}

		private void Update()
		{
			mNetSystem.handleNetData();
		}

		private void OnDestroy()
		{
			mNetSystem.closeNet();
		}
	}
}