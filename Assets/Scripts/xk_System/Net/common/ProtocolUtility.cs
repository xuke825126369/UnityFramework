using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf;
using game.protobuf.data;
using System.IO;

namespace xk_System.Net.Protocol
{
	public class Protocol3Utility
	{
		public static byte[] SerializePackage (IMessage data)
		{
			Google.Protobuf.IMessage data1 = data as Google.Protobuf.IMessage;
			byte[] stream = data1.ToByteArray ();
			return stream;
		}

		public static T getData<T> (byte[] stream,int index,int Length) where T:IMessage, new()
		{
			T t = new T ();
			Google.Protobuf.CodedInputStream mStream = new CodedInputStream (stream, index, Length);
			t.MergeFrom (mStream);
			return t;
		}

		public static IMessage getData (IMessage t, byte[] stream,int index,int Length)
		{
			Google.Protobuf.CodedInputStream mStream = new CodedInputStream (stream, index, Length);
			t.MergeFrom (mStream);
			return t;
		}
	}

	public class Protobuf2Utility
	{
		private static GameProtocols serializer = new GameProtocols ();
		private static MemoryStream mst = new MemoryStream ();

		public static byte[] SerializePackage (IMessage data)
		{
			mst.Position = 0;
			serializer.Serialize (mst, data);
			return mst.ToArray ();
		}

		public static T  getData<T> (byte[] buffer)
		{
			byte[] stream = buffer;
			mst.SetLength (stream.Length);
			mst.Position = 0;
			mst.Write (stream, 0, stream.Length);
			mst.Position = 0;
			return (T)serializer.Deserialize (mst, null, typeof(T)); //反序列化    
		}
	}
}