using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using System;
using xk_System.Debug;

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

			UInt16 groupCount = 0;
			if (buffer.Length % ClientConfig.nUdpPackageFixedBodySize == 0) {
				groupCount = (UInt16)(buffer.Length / ClientConfig.nUdpPackageFixedBodySize);
			} else {
				groupCount = (UInt16)(buffer.Length / ClientConfig.nUdpPackageFixedBodySize + 1);
			}
				
			//DebugSystem.Log ("Client bufferLength: " + buffer.Length);
			while (nBeginIndex < buffer.Length) {
				if (nBeginIndex + ClientConfig.nUdpPackageFixedBodySize > buffer.Length) {
					readBytes = buffer.Length - nBeginIndex;
				} else {
					readBytes = ClientConfig.nUdpPackageFixedBodySize;
				}

				var mPackage = mUdpFixedSizePackagePool.Pop ();
				mPackage.nOrderId = this.nPackageOrderId;
				mPackage.nGroupCount = groupCount;
				mPackage.nPackageId = id;
				mPackage.Length = readBytes + ClientConfig.nUdpPackageFixedHeadSize;
				Array.Copy (buffer, nBeginIndex, mPackage.buffer, ClientConfig.nUdpPackageFixedHeadSize, readBytes);

				NetPackageEncryption.Encryption (mPackage);
				SendNetStream (mPackage);

				if (id >= 50) {
					mUdpCheckPool.AddSendCheck (mPackage);
					this.nPackageOrderId++;
					if (this.nPackageOrderId == 0) {
						this.nPackageOrderId = 1;
					}
				} else {
					mUdpFixedSizePackagePool.recycle (mPackage);
				}

				nBeginIndex += readBytes;
				groupCount = 1;
			}
		}

		public NetUdpFixedSizePackage GetCheckResultPackage(UInt16 id, object data)
		{
			IMessage data1 = data as IMessage;
			byte[] stream = Protocol3Utility.SerializePackage (data1);

			var mPackage = mUdpFixedSizePackagePool.Pop ();
			mPackage.nOrderId = 0;
			mPackage.nGroupCount = 0;
			mPackage.nPackageId = id;
			mPackage.Length = stream.Length + ClientConfig.nUdpPackageFixedHeadSize;
			Array.Copy (stream, 0, mPackage.buffer, ClientConfig.nUdpPackageFixedHeadSize, stream.Length);

			NetPackageEncryption.Encryption (mPackage);

			return mPackage;
		}

		public void SendNetStream(NetUdpFixedSizePackage mPackage)
		{
			SendNetStream (mPackage.buffer, 0, mPackage.Length);
		}

		public void SendNetData (UInt16 id, object data)
		{
			IMessage data1 = data as IMessage;
			byte[] stream = Protocol3Utility.SerializePackage (data1);
			SendNetData (id, stream);
		}

	}
}