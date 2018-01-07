using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using System;
using xk_System.Net.UDP.POINTTOPOINT.Protocol;

namespace xk_System.Net.UDP.POINTTOPOINT.Server
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
			UInt16 groupCount = (UInt16)(buffer.Length / ServerConfig.nUdpPackageFixedBodySize + 1);
			while (nBeginIndex < buffer.Length) {
				if (nBeginIndex + ServerConfig.nUdpPackageFixedBodySize > buffer.Length) {
					readBytes = buffer.Length - nBeginIndex;
				} else {
					readBytes = ServerConfig.nUdpPackageFixedBodySize;
				}

				var mPackage = mUdpFixedSizePackagePool.Pop ();
				mPackage.nOrderId = this.nPackageOrderId;
				mPackage.nGroupCount = groupCount;
				mPackage.nPackageId = id;
				mPackage.Offset = ServerConfig.nUdpPackageFixedHeadSize;
				mPackage.Length = readBytes;
				Array.Copy (buffer, nBeginIndex, mPackage.buffer, mPackage.Offset, readBytes);

				NetPackageEncryption.Encryption (mPackage);
				SendNetStream (mPackage.buffer, mPackage.Offset, mPackage.Length);

				if (id >= 50) {
					mUdpCheckPool.AddSendCheck (this.nPackageOrderId, mPackage);
					nBeginIndex += readBytes;
					this.nPackageOrderId++;
				} else {
					mUdpFixedSizePackagePool.recycle (mPackage);
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