using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf;
using System;
using xk_System.Debug;
using xk_System.Net.UDP.Protocol;

namespace xk_System.Net.UDP.Client.Event
{
	public interface NetEventInterface
	{
		void sendNetData (int command, byte[] buffer);
		void addNetListenFun (Action<NetPackage> fun);
		void removeNetListenFun (Action<NetPackage> fun);
	}

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
			if (mLogicFuncDic.ContainsKey (mPackage.command)) {
				mLogicFuncDic [mPackage.command] (mPackage);
			} else {
				DebugSystem.LogError ("不存在的 协议ID: " + mPackage.command);
			}
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