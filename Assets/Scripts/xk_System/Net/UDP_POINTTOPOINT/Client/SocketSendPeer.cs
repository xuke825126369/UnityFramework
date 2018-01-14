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
			nPackageOrderId = ClientConfig.nUdpMinOrderId;
		}

		private void AddPackageOrderId()
		{
			if (nPackageOrderId == ClientConfig.nUdpMaxOrderId) {
				nPackageOrderId = ClientConfig.nUdpMinOrderId;
			} else {
				nPackageOrderId++;
			}
		}

		public void SendNetData (UInt16 id, byte[] buffer)
		{
			DebugSystem.Assert (id > 50, "Udp 系统内置命令 此逻辑不处理");

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

				var mPackage = SafeGetNetUdpFixedPackage ();
				mPackage.nOrderId = this.nPackageOrderId;
				mPackage.nGroupCount = groupCount;
				mPackage.nPackageId = id;
				mPackage.Length = readBytes + ClientConfig.nUdpPackageFixedHeadSize;
				Array.Copy (buffer, nBeginIndex, mPackage.buffer, ClientConfig.nUdpPackageFixedHeadSize, readBytes);

				NetPackageEncryption.Encryption (mPackage);

				mUdpCheckPool.AddSendCheck (mPackage);

				AddPackageOrderId ();
				groupCount = 1;

				nBeginIndex += readBytes;
			}
		}

		public NetUdpFixedSizePackage GetUdpSystemPackage(UInt16 id, object data)
		{
			DebugSystem.Assert (id <= 50, "不是 Udp 系统内置命令");

			IMessage data1 = data as IMessage;
			byte[] stream = Protocol3Utility.SerializePackage (data1);

			var mPackage = SafeGetNetUdpFixedPackage ();
			mPackage.nOrderId = 0;
			mPackage.nGroupCount = 0;
			mPackage.nPackageId = id;
			mPackage.Length = stream.Length + ClientConfig.nUdpPackageFixedHeadSize;
			Array.Copy (stream, 0, mPackage.buffer, ClientConfig.nUdpPackageFixedHeadSize, stream.Length);

			NetPackageEncryption.Encryption (mPackage);

			return mPackage;
		}

		public void SendNetStream(NetPackage mPackage)
		{
			SendNetStream (mPackage.buffer, 0, mPackage.Length);
		}

		public void SendNetData (UInt16 id, object data)
		{
			DebugSystem.Assert (id > 50, "Udp 系统内置命令 此逻辑不处理");

			IMessage data1 = data as IMessage;
			byte[] stream = Protocol3Utility.SerializePackage (data1);
			SendNetData (id, stream);
		}
	}

}