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
		public UInt16 nCombineGroupId;
		public UInt16 nCombineGroupCount;
		public UInt16 nCombinePackageId;

		private ConcurrentQueue<NetUdpFixedSizePackage> mCombinePackageQueue;
		//private List<NetUdpFixedSizePackage> mTestList = null;
		public NetCombinePackage ()
		{
			base.buffer = new byte[ServerConfig.nUdpCombinePackageFixedSize];
			mCombinePackageQueue = new ConcurrentQueue<NetUdpFixedSizePackage> ();
			//mTestList = new List<NetUdpFixedSizePackage> ();
		}

		public void Add(NetUdpFixedSizePackage mPackage)
		{
			mCombinePackageQueue.Enqueue (mPackage);
			//mTestList.Add (mPackage);
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
			int nSumLength = nCombineGroupCount * ServerConfig.nUdpPackageFixedBodySize + ServerConfig.nUdpPackageFixedHeadSize;
			if (base.buffer.Length < nSumLength) {
				base.buffer = new byte[nSumLength];
			}

			base.Length = ServerConfig.nUdpPackageFixedHeadSize;
			//int nCurrentOrderId = nCombineGroupId; 
			while (!mCombinePackageQueue.IsEmpty) {
				NetUdpFixedSizePackage tempPackage = null;
				if (mCombinePackageQueue.TryDequeue (out tempPackage)) {
					Array.Copy (tempPackage.buffer, ServerConfig.nUdpPackageFixedHeadSize, base.buffer, base.Length, tempPackage.Length - ServerConfig.nUdpPackageFixedHeadSize);
					base.Length += (tempPackage.Length - ServerConfig.nUdpPackageFixedHeadSize);

					/*if (tempPackage.nOrderId != nCurrentOrderId) {
						DebugSystem.LogError ("Server 组包失败 检测开始 ");

						foreach (var v in mTestList) {
							DebugSystem.LogError (v.nOrderId);
						}

						throw new Exception ("Server 组包失败 检查结束: " + mTestList.Count);
					}

					if (nCurrentOrderId == ServerConfig.nUdpMaxOrderId) {
						nCurrentOrderId = ServerConfig.nUdpMinOrderId;
					} else {
						nCurrentOrderId++;
					}*/

					ObjectPoolManager.Instance.mUdpFixedSizePackagePool.recycle (tempPackage);
				}
			}
			//mTestList.Clear ();

			base.nPackageId = nCombinePackageId;
		}

	}

	public class NetEndPointPackage
	{
		public EndPoint mRemoteEndPoint = null;
		public NetPackage mPackage = null;
	}

}

