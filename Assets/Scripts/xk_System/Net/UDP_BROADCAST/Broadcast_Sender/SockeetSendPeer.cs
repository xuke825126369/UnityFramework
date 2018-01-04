using System.Collections;
using System.Collections.Generic;
using System;
using xk_System.Net.UDP.BROADCAST.Protocol;
using Google.Protobuf;

namespace xk_System.Net.UDP.BROADCAST.Client
{
	public class SocketSendPeer:SocketUdp_Basic
	{
		public void SendNetData(UInt16 id, object data)
		{
			byte[] stream = Protocol3Utility.SerializePackage ((IMessage)data);
			SendNetData (id, stream);
		}

		public virtual void Update(double elapsed)
		{

		}

		public void SendNetData (UInt16 id, byte[] buffer)
		{
			ArraySegment<byte> stream = NetEncryptionStream.EncryptionGroup (id, buffer, 0, buffer.Length);
			SendNetStream (stream.Array, stream.Offset, stream.Count);
		}
	}
}