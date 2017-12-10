using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf;
using xk_System.Net.Client;
using System;

namespace xk_System.Net.Protocol.Protobuf3
{
	public class ProtobufUtility
	{
		public static byte[] SerializePackage (IMessage data)
		{
			Google.Protobuf.IMessage data1 = data as Google.Protobuf.IMessage;
			byte[] stream = data1.ToByteArray ();
			return stream;
		}

		public static T getData<T>(NetPackage mPackage) where T:IMessage,new()
		{
			T t = new T ();
			byte[] stream = mPackage.buffer;
			Google.Protobuf.CodedInputStream mStream = new CodedInputStream (stream);
			t.MergeFrom (mStream);
			return t;
		}

		public static IMessage getData(IMessage t, NetPackage mPackage)
		{
			byte[] stream = mPackage.buffer;
			Google.Protobuf.CodedInputStream mStream = new CodedInputStream (stream);
			t.MergeFrom (mStream);
			return t;
		}
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
			mNetEventInterface.sendNetData (command, ProtobufUtility.SerializePackage ((IMessage)data));
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