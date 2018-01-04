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

		List<NetPackage> mCanUseSortPackageList;
		List<NetPackage> mUsedPackageList;

		public SocketSendPeer()
		{
			nPackageOrderId = 1;
			mCanUseSortPackageList = new List<NetPackage> ();
			mUsedPackageList = new List<NetPackage> ();
		}

		public void SendNetData (UInt16 id, byte[] buffer)
		{
			var mPackage = mUdpFixedSizePackagePool.Pop ();

			int readBytes = 0;
			int nBeginIndex = 0;
			UInt16 groupCount = (UInt16)(buffer.Length / ClientConfig.nMaxBufferSize + 1);
			while (nBeginIndex < buffer.Length) {
				if (nBeginIndex + ClientConfig.nMaxBufferSize - 12 > buffer.Length) {
					readBytes = buffer.Length - nBeginIndex;
				} else {
					readBytes = ClientConfig.nMaxBufferSize - 12;
				}
					
				mPackage.nOrderId = this.nPackageOrderId;
				mPackage.nGroupCount = groupCount;
				mPackage.nPackageId = id;
				mPackage.buffer = buffer;
				mPackage.Offset = nBeginIndex;
				mPackage.Length = readBytes;
				ArraySegment<byte> stream = NetPackageEncryption.Encryption (mPackage);
				SendNetStream (stream.Array, stream.Offset, stream.Count);

				if (id >= 50) {
					mUdpCheckPool.AddSendCheck (this.nPackageOrderId, stream);
					nBeginIndex += readBytes;
					this.nPackageOrderId++;
				} else {
					
				}

				nBeginIndex += readBytes;
				groupCount = 1;
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