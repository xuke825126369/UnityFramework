using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using xk_System.Net.UDP.BROADCAST.Protocol;
using Google.Protobuf;

namespace xk_System.Net.UDP.BROADCAST.Server
{
	public class SocketSendPeer : SocketSystem_UdpServer
	{
		public void SendNetData(UInt16 id, object data)
		{
			byte[] stream = Protocol3Utility.SerializePackage ((IMessage)data);
			SendNetData (id, stream);
		}

		public void SendNetData (UInt16 id, byte[] buffer)
		{
			mNetPackage.command = id;
			mNetPackage.buffer = buffer;
			ArraySegment<byte> stream = NetEncryptionStream.Encryption (id,mNetPackage.buffer, 0, mNetPackage.buffer.Length);
			SendNetStream (stream.Array, stream.Offset, stream.Count);
		}
	}
}