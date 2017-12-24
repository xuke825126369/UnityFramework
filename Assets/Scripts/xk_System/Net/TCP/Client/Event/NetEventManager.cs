using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Google.Protobuf;
using xk_System.Net.TCP.Client.Event;

namespace xk_System.Net.TCP.Client
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