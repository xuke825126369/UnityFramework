using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using System;
using xk_System.Debug;
using System.Collections.Concurrent;

namespace xk_System.Net.UDP.POINTTOPOINT.Server
{
	public class SocketSendPeer : SocketUdp_Basic
	{
		private ConcurrentQueue<IMessage> mSendPackageQueue = null;
		private UInt16 nCurrentWaitSendOrderId;

		public SocketSendPeer()
		{
			mSendPackageQueue = new ConcurrentQueue<IMessage> ();
			nCurrentWaitSendOrderId = ServerConfig.nUdpMinOrderId;
		}

		private void AddSendPackageOrderId()
		{
			if (nCurrentWaitSendOrderId == ServerConfig.nUdpMaxOrderId) {
				nCurrentWaitSendOrderId = ServerConfig.nUdpMinOrderId;
			} else {
				nCurrentWaitSendOrderId++;
			}
		}

		public void SendNetData (UInt16 id, byte[] buffer)
		{
			DebugSystem.Assert (id > 50, "Udp 系统内置命令 此逻辑不处理");

			int readBytes = 0;
			int nBeginIndex = 0;

			UInt16 groupCount = 0;
			if (buffer.Length % ServerConfig.nUdpPackageFixedBodySize == 0) {
				groupCount = (UInt16)(buffer.Length / ServerConfig.nUdpPackageFixedBodySize);
			} else {
				groupCount = (UInt16)(buffer.Length / ServerConfig.nUdpPackageFixedBodySize + 1);
			}

			while (nBeginIndex < buffer.Length) {
				if (nBeginIndex + ServerConfig.nUdpPackageFixedBodySize > buffer.Length) {
					readBytes = buffer.Length - nBeginIndex;
				} else {
					readBytes = ServerConfig.nUdpPackageFixedBodySize;
				}
					
				NetUdpFixedSizePackage mPackage = ObjectPoolManager.Instance.mUdpFixedSizePackagePool.Pop ();
				mPackage.nGroupCount = groupCount;
				mPackage.nPackageId = id;
				mPackage.nOrderId = nCurrentWaitSendOrderId;
				mPackage.Length = readBytes + ServerConfig.nUdpPackageFixedHeadSize;
				Array.Copy (buffer, nBeginIndex, mPackage.buffer, ServerConfig.nUdpPackageFixedHeadSize, readBytes);

				NetPackageEncryption.Encryption (mPackage);
				mUdpCheckPool.AddSendCheck (mPackage);

				AddSendPackageOrderId ();
				nBeginIndex += readBytes;
				groupCount = 1;
			}
		}

		public NetUdpFixedSizePackage GetUdpSystemPackage(UInt16 id, object data)
		{
			DebugSystem.Assert (id <= 50, "不是 Udp 系统内置命令");

			IMessage data1 = data as IMessage;
			byte[] stream = Protocol3Utility.SerializePackage (data1);

			var mPackage = ObjectPoolManager.Instance.mUdpFixedSizePackagePool.Pop ();
			mPackage.nOrderId = 0;
			mPackage.nGroupCount = 0;
			mPackage.nPackageId = id;
			mPackage.Length = stream.Length + ServerConfig.nUdpPackageFixedHeadSize;
			Array.Copy (stream, 0, mPackage.buffer, ServerConfig.nUdpPackageFixedHeadSize, stream.Length);

			NetPackageEncryption.Encryption (mPackage);

			return mPackage;
		}

		public void SendNetStream(NetPackage mPackage)
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