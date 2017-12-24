using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.Debug;
using System.Net.Sockets;
using System;
using xk_System.DataStructure;
using xk_System.Event;
using xk_System.Net.UDP.Client.Event;

namespace xk_System.Net.UDP.Client
{
	public class NetSystem:SocketSystem_Udp,NetEventInterface
	{
		public void initNet (string ServerAddr, int ServerPort)
		{
			base.InitNet (ServerPort);
		}

		public void sendNetData (int command, byte[] buffer)
		{
			base.mSocketPeer.SendNetData (command, buffer);  
		}

		public void handleNetData ()
		{
			base.mSocketPeer.HandleNetPackage ();
		}

		public void addNetListenFun (Action<NetPackage> fun)
		{
			base.mSocketPeer.addListenFun (fun);
		}

		public void removeNetListenFun (Action<NetPackage> fun)
		{
			base.mSocketPeer.removeListenFun (fun);
		}

		public void closeNet ()
		{
			base.CloseNet ();
		}
	}
}


