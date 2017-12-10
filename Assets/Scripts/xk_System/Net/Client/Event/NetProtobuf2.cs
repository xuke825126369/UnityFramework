using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.Net.Client;
using System;
using game.protobuf.data;
using System.IO;
using System.Runtime.Remoting.Messaging;

namespace xk_System.Net.Client.Event
{
	public class Protobuf2Event
	{
		private Dictionary<int, Action<NetPackage>> mLogicFuncDic = new Dictionary<int, Action<NetPackage>> ();

		public Protobuf2Event ()
		{
			//NetManager.Instance.addNetListenFun (DeSerialize);
		}

		public void sendNetData (int command, IMessage data)
		{
			//NetManager.Instance.sendNetData (command, ProtobufUtility.SerializePackage (data));
		}

		public void DeSerialize (NetPackage mPackage)
		{
			mLogicFuncDic [mPackage.command] (mPackage);
		}
	}
}