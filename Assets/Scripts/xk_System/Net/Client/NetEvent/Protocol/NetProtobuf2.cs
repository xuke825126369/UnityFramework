using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.Net.Client;
using System;
using game.protobuf.data;
using System.IO;
using System.Runtime.Remoting.Messaging;

namespace xk_System.Net.Protocol.Protobuf2
{
	public class ProtobufUtility
	{
		private static GameProtocols serializer = new GameProtocols ();
		private static MemoryStream mst = new MemoryStream ();

		public static byte[] SerializePackage (IMessage data)
		{
			mst.Position = 0;
			serializer.Serialize (mst, data);
			return mst.ToArray ();
		}

		public static T  getData<T> (NetPackage mPackage)
		{
			byte[] stream = mPackage.buffer;
			mst.SetLength (stream.Length);
			mst.Position = 0;
			mst.Write (stream, 0, stream.Length);
			mst.Position = 0;
			return (T)serializer.Deserialize (mst, null, typeof(T)); //反序列化    
		}
	}

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