using System.Collections;
using System.Collections.Generic;
using System;
using System.Collections.Concurrent;
using Google.Protobuf;
using xk_System.Debug;
using System.Net;

namespace xk_System.Net.UDP.POINTTOPOINT.Server
{
	public class NetPackage
	{
		public UInt16 nOrderId;
		public UInt16 nGroupCount;
		public UInt16 nPackageId;

		public byte[] buffer;
		public int Length;

		public NetPackage ()
		{
			nOrderId = 0;
			nGroupCount = 0;
			nPackageId = 0;

			buffer = null;
			Length = 0;
		}
	}

	public class NetUdpFixedSizePackage : NetPackage
	{
		public NetUdpFixedSizePackage ()
		{
			buffer = new byte[ServerConfig.nUdpPackageFixedSize];
		}
	}

	public class NetCombinePackage : NetPackage
	{
		private int nGetCombineCount;
		public NetCombinePackage ()
		{
			base.buffer = new byte[ServerConfig.nUdpCombinePackageFixedSize];
		}

		public void Init(NetUdpFixedSizePackage mPackage)
		{
			base.nPackageId = mPackage.nPackageId;
			base.nGroupCount = mPackage.nGroupCount;
			base.nOrderId = mPackage.nOrderId;

			int nSumLength = base.nGroupCount * ServerConfig.nUdpPackageFixedBodySize + ServerConfig.nUdpPackageFixedHeadSize;
			if (base.buffer.Length < nSumLength) {
				base.buffer = new byte[nSumLength];
			}

			base.Length = ServerConfig.nUdpPackageFixedHeadSize;

			nGetCombineCount = 0;
			Add (mPackage);
		}

		public void Add(NetUdpFixedSizePackage mPackage)
		{
			Combine (mPackage);
			nGetCombineCount++;

			ObjectPoolManager.Instance.mUdpFixedSizePackagePool.recycle (mPackage);
		}

		public bool CheckCombineFinish ()
		{
			return nGetCombineCount == base.nGroupCount;
		}

		private void Combine (NetUdpFixedSizePackage mPackage)
		{
			int nCopyLength = mPackage.Length - ServerConfig.nUdpPackageFixedHeadSize;
			Array.Copy (mPackage.buffer, ServerConfig.nUdpPackageFixedHeadSize, base.buffer, base.Length, nCopyLength);
			base.Length += nCopyLength;
		}

	}

	public class NetEndPointPackage
	{
		public EndPoint mRemoteEndPoint = null;
		public NetPackage mPackage = null;
	}

}

