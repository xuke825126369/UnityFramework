using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf;
using System.IO;

namespace xk_System.Net.UDP.BROADCAST.Protocol
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
}