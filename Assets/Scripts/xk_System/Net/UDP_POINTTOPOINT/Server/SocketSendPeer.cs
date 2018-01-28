using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using System;
using xk_System.Debug;
using System.Collections.Concurrent;
using System.Threading;

namespace xk_System.Net.UDP.POINTTOPOINT.Server
{
	public class SocketSendPeer : SocketUdp_Basic
	{
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

		public void SendNetData (UInt16 id, object data)
		{
			IMessage data1 = data as IMessage;
			byte[] stream = Protocol3Utility.SerializePackage (data1);
			mUdpCheckPool.SendCheckPackage (id, stream);
		}
	}

}