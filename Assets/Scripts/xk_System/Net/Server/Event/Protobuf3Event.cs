using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using xk_System.Net.Protocol;
using Google.Protobuf;
using xk_System.Debug;

namespace xk_System.Net.Server.Event
{
	public class Protobuf3Event
	{
		private Dictionary<int, Action<NetPackage>> mLogicFuncDic = new Dictionary<int, Action<NetPackage>>();

		public Protobuf3Event()
		{
			
		}

		public void DeSerialize (NetPackage mPackage)
		{
			if (mLogicFuncDic.ContainsKey (mPackage.command)) {
				mLogicFuncDic [mPackage.command] (mPackage);
			} else {
				DebugSystem.LogError ("不存在的 协议ID: " + mPackage.command);
			}
		}

		public byte[] Serialize (object data)
		{
			return Protocol3Utility.SerializePackage ((IMessage)data);
		}

		public void addNetListenFun(int command,Action<NetPackage> func)
		{
			if (!mLogicFuncDic.ContainsKey (command)) {
				mLogicFuncDic [command] = func;
			} else {
				mLogicFuncDic [command] += func;
			}
		}

	}
}

