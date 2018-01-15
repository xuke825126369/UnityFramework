using System.Collections;
using System.Collections.Generic;
using System;
using Google.Protobuf;

namespace xk_System.Net.UDP.POINTTOPOINT.Client
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
			buffer = new byte[ClientConfig.nUdpPackageFixedSize];
		}
	}

	public class NetCombinePackage : NetPackage
	{
		public UInt16 nCombineGroupId;
		public UInt16 nCombineGroupCount;
		public UInt16 nCombinePackageId;

		public Queue<NetUdpFixedSizePackage> mCombinePackageQueue;

		public NetCombinePackage ()
		{
			base.buffer = new byte[ClientConfig.nUdpCombinePackageFixedSize];
			mCombinePackageQueue = new Queue<NetUdpFixedSizePackage> ();
		}

		public bool CheckCombineFinish ()
		{
			if (mCombinePackageQueue.Count == nCombineGroupCount) {
				SetPackage ();

				return true;
			} else {
				return false;
			}
		}

		private void SetPackage ()
		{
			int nSumLength = nCombineGroupCount * ClientConfig.nUdpPackageFixedBodySize + ClientConfig.nUdpPackageFixedHeadSize;
			if (base.buffer.Length < nSumLength) {
				base.buffer = new byte[nSumLength];
			}

			base.Length = ClientConfig.nUdpPackageFixedHeadSize;
			while (mCombinePackageQueue.Count > 0) {
				NetUdpFixedSizePackage tempPackage = mCombinePackageQueue.Dequeue ();
				Array.Copy (tempPackage.buffer, ClientConfig.nUdpPackageFixedHeadSize, base.buffer, base.Length, tempPackage.Length - ClientConfig.nUdpPackageFixedHeadSize);
				base.Length += (tempPackage.Length - ClientConfig.nUdpPackageFixedHeadSize);

				ObjectPoolManager.Instance.mUdpFixedSizePackagePool.recycle (tempPackage);
			}

			base.nPackageId = nCombinePackageId;
		}
	}
}

