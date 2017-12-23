using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.Debug;
using System.Net.Sockets;
using xk_System.Net;
using System;
using xk_System.Net.Server.Event;
using xk_System.DataStructure;

namespace xk_System.Net.Server
{
	public class NetSystem :SocketSystem_SocketAsyncEventArgs, NetEventInterface
	{
		public void initNet (string ServerAddr, int ServerPort)
		{
			base.InitNet (ServerAddr, ServerPort);
		}

		public void sendNetData (int clientId, int command, byte[] buffer)
		{
			//base.mNetSendSystem.SendNetData (clientId, command, buffer);  
		}

		public void handleNetData ()
		{
			//base.HandleNetPackage ();
		}

		public void addNetListenFun (Action<NetPackage> fun)
		{
			//base.mNetReceiveSystem.addListenFun (fun);
		}

		public void removeNetListenFun (Action<NetPackage> fun)
		{
			//base.mNetReceiveSystem.removeListenFun (fun);
		}

		public void closeNet ()
		{
			base.CloseNet ();
		}
	}
}


