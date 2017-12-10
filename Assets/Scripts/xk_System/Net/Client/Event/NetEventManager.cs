using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.Net.Client;
using System;
using xk_System.Net.Protocol;
using Google.Protobuf;
using xk_System.Net.Client.Event;

namespace xk_System.Net.Client
{
	public interface NetEventInterface
	{
		void sendNetData (int command, byte[] buffer);
		void addNetListenFun (Action<NetPackage> fun);
		void removeNetListenFun (Action<NetPackage> fun);
	}

	public class NetEventManager
	{
		private Protobuf3Event mEventManager = null;

		public NetEventManager(NetEventInterface mInterface)
		{
			mEventManager = new Protobuf3Event (mInterface);
		}

		public void sendNetData (int command, object data)
		{
			mEventManager.sendNetData (command, data);
		}

		public void addNetListenFun (int command, Action<NetPackage> func)
		{
			mEventManager.addNetListenFun (command, func);
		}

		public void removeNetListenFun (int command, Action<NetPackage> func)
		{
			mEventManager.removeNetListenFun (command, func);
		}
	}
}