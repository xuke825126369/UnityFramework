using System.Collections;
using System.Collections.Generic;
using System;
using Google.Protobuf;

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
		public UInt16 nCombineGroupId;
		public UInt16 nCombineGroupCount;
		public UInt16 nCombinePackageId;

		public Dictionary<UInt16, NetUdpFixedSizePackage> mReceivePackageDic;
		public List<NetUdpFixedSizePackage> mNeedRecyclePackage;

		public NetCombinePackage ()
		{
			base.buffer = new byte[ServerConfig.nUdpCombinePackageFixedSize];

			mReceivePackageDic = new Dictionary<ushort, NetUdpFixedSizePackage> ();
			mNeedRecyclePackage = new List<NetUdpFixedSizePackage> ();
		}

		public bool bInCombinePackage(NetUdpFixedSizePackage mPackage)
		{
			UInt16 nOrderId = mPackage.nOrderId;
			if (this.nCombineGroupId + this.nCombineGroupCount > nOrderId && this.nCombineGroupId < nOrderId) {
				return true;
			} else {
				return false;
			}

		}

		public bool CheckCombineFinish ()
		{
			if (mReceivePackageDic.Count == nCombineGroupCount) {
				SetPackage ();

				return true;
			} else {
				return false;
			}
		}

		private void SetPackage ()
		{
			int nSumLength = nCombineGroupCount * ServerConfig.nUdpPackageFixedBodySize + ServerConfig.nUdpPackageFixedHeadSize;
			if (base.buffer.Length < nSumLength) {
				base.buffer = new byte[nSumLength];
			}

			base.Length = ServerConfig.nUdpPackageFixedHeadSize;
			for (UInt16 i = nCombineGroupId; i < nCombineGroupId + nCombineGroupCount; i++) {
				byte[] tempBuf = mReceivePackageDic [i].buffer;
				Array.Copy (tempBuf, ServerConfig.nUdpPackageFixedHeadSize, base.buffer, base.Length, (mReceivePackageDic [i].Length - ServerConfig.nUdpPackageFixedHeadSize));
				base.Length += (mReceivePackageDic [i].Length - ServerConfig.nUdpPackageFixedHeadSize);
			}

			base.nPackageId = nCombinePackageId;
		}
	}
}

