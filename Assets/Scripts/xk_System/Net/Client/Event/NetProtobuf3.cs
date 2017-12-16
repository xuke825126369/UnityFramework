using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf;
using xk_System.Net.Client;
using System;
using xk_System.Net.Protocol;

namespace xk_System.Net.Client.Event
{
	public class Protobuf3Event
	{
		private Dictionary<int, Action<NetPackage>> mLogicFuncDic = new Dictionary<int, Action<NetPackage>>();
		private NetEventInterface mNetEventInterface;

		public Protobuf3Event(NetEventInterface mInterface)
		{
			mNetEventInterface = mInterface;
			mNetEventInterface.addNetListenFun (DeSerialize);
		}

		public void sendNetData (int command, object data)
		{
			mNetEventInterface.sendNetData (command, Protocol3Utility.SerializePackage ((IMessage)data));
		}

		public void DeSerialize (NetPackage mPackage)
		{
			mLogicFuncDic [mPackage.command] (mPackage);
		}

		public void addNetListenFun(int command,Action<NetPackage> func)
		{
			if (!mLogicFuncDic.ContainsKey (command)) {
				mLogicFuncDic [command] = func;
			} else {
				mLogicFuncDic [command] += func;
			}
		}

		public void removeNetListenFun(int command,Action<NetPackage> func)
		{
			if (mLogicFuncDic.ContainsKey (command)) {
				mLogicFuncDic [command] -= func;
			}
		}
	}
}