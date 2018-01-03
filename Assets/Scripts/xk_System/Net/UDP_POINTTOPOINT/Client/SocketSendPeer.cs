using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf;
using System;
using xk_System.Net.UDP.POINTTOPOINT.Protocol;

namespace xk_System.Net.UDP.POINTTOPOINT.Client
{
	public class SocketSendPeer : SocketUdp_Basic
	{
		private UInt16 nPackageOrderId;

		public SocketSendPeer()
		{
			nPackageOrderId = 1;
		}

		public void SendNetData (UInt16 id, byte[] buffer)
		{
			int readBytes = 0;
			int nBeginIndex = 0;
			UInt16 groupCount = (UInt16)(buffer.Length / ClientConfig.nMaxBufferSize + 1);
			while (nBeginIndex < buffer.Length) {
				if (nBeginIndex + ClientConfig.nMaxBufferSize - 12 > buffer.Length) {
					readBytes = buffer.Length - nBeginIndex;
				} else {
					readBytes = ClientConfig.nMaxBufferSize - 12;
				}

				UInt16 nPackageId = id;
				UInt16 nOrderId = this.nPackageOrderId;
				UInt32 uniqueId = NetPackageUtility.getUniqueId (nPackageId, nOrderId, groupCount);
				groupCount = 1;

				ArraySegment<byte> stream = NetEncryptionStream.EncryptionGroup (uniqueId, buffer, nBeginIndex, readBytes);
				SendNetStream (stream.Array, stream.Offset, stream.Count);

				if (nPackageId >= 50) {
					mUdpCheckPool.AddSendCheck (nOrderId, stream);
				}

				nBeginIndex += readBytes;
				this.nPackageOrderId++;
			}
		}

		public void SendNetData (UInt16 id, object data)
		{
			IMessage data1 = data as IMessage;
			byte[] stream = Protocol3Utility.SerializePackage (data1);
			SendNetData (id, stream);
		}

	}
}