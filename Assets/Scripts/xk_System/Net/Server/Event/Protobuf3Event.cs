using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using xk_System.Net.Protocol;
using Google.Protobuf;

namespace xk_System.Net.Server.Event
{
	public interface NetEventInterface
	{
		void sendNetData (int clientId, int command, byte[] buffer);
		void addNetListenFun (Action<NetPackage> fun);
		void removeNetListenFun (Action<NetPackage> fun);
	}

	public class Protobuf3Event : MonoBehaviour
	{
		private Dictionary<int, Action<NetPackage>> mLogicFuncDic = new Dictionary<int, Action<NetPackage>>();
		private NetEventInterface mNetEventInterface;

		public Protobuf3Event(NetEventInterface mInterface)
		{
			mNetEventInterface = mInterface;
			mNetEventInterface.addNetListenFun (DeSerialize);
		}

		public void sendNetData (int clientId, int command, object data)
		{
			mNetEventInterface.sendNetData (clientId, command, Protocol3Utility.SerializePackage ((IMessage)data));
		}

		public void DeSerialize (NetPackage mPackage)
		{
			mLogicFuncDic [mPackage.command] (mPackage);
		}

		public void addNetListenFun(int command,Action<NetPackage> func)
		{
			if (!mLogicFuncDic.ContainsKey (command)) {
				mLogicFuncDic [command] = func;
			}
			mLogicFuncDic [command] += func;
		}

		public void removeNetListenFun(int command,Action<NetPackage> func)
		{
			if (mLogicFuncDic.ContainsKey (command)) {
				mLogicFuncDic [command] -= func;
			}
		}
	}
}