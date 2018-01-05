using System.Collections;
using System.Collections.Generic;
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
			var mPackage = mUdpFixedSizePackagePool.Pop ();

			int readBytes = 0;
			int nBeginIndex = 0;
			UInt16 groupCount = (UInt16)(buffer.Length / ClientConfig.nUdpPackageFixedBodySize + 1);
			while (nBeginIndex < buffer.Length) {
				if (nBeginIndex + ClientConfig.nUdpPackageFixedBodySize > buffer.Length) {
					readBytes = buffer.Length - nBeginIndex;
				} else {
					readBytes = ClientConfig.nUdpPackageFixedBodySize;
				}
							
				mPackage.nOrderId = this.nPackageOrderId;
				mPackage.nGroupCount = groupCount;
				mPackage.nPackageId = id;
				mPackage.Offset = ClientConfig.nUdpPackageFixedHeadSize;
				mPackage.Length = readBytes;

				Array.Copy (buffer, nBeginIndex, mPackage.buffer, mPackage.Offset, readBytes);
				NetPackageEncryption.Encryption (mPackage);
				SendNetStream (mPackage.buffer, mPackage.Offset, mPackage.Length);

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