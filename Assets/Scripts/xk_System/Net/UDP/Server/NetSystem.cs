using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.Debug;
using System.Net.Sockets;
using xk_System.Net;
using System;
using xk_System.Net.UDP.Server.Event;
using xk_System.DataStructure;

namespace xk_System.Net.UDP.Server
{
	public class NetSystem :SocketSystem_UdpServer
	{
		public static Protobuf3Event mEventSystem = new Protobuf3Event ();

		public void initNet (string ServerAddr, int ServerPort)
		{
			base.InitNet (ServerAddr, ServerPort);
		}

		public void Update ()
		{
			foreach (var v in  ClientFactory.Instance.mClientPool) {
				v.Value.Update ();
			}
		}

		public void sendNetData (int clientId,int command, object data)
		{
			byte[] stream = mEventSystem.Serialize (data);
			ClientFactory.Instance.GetClient (clientId).SendNetData (command, stream);
		}

		public void addNetListenFun (int command, Action<NetPackage> func)
		{
			mEventSystem.addNetListenFun (command, func);
		}

		public void closeNet ()
		{
			base.CloseNet ();
		}
	}
}


