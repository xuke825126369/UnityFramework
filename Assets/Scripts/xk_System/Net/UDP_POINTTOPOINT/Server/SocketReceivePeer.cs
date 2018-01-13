using System.Collections;
using System.Collections.Generic;
using xk_System.DataStructure;
using System;
using System.Collections.Concurrent;
using xk_System.Event;
using xk_System.Debug;
using System.Net.Sockets;
using Google.Protobuf;
using xk_System.Net.UDP.POINTTOPOINT.Protocol;

namespace xk_System.Net.UDP.POINTTOPOINT.Server
{
	public abstract class SocketReceivePeer
	{
		protected ConcurrentQueue<NetPackage> mNeedHandlePackageQueue = null;
		protected SafeObjectPool<NetCombinePackage> mCombinePackagePool = null;

		protected UdpCheckPool mUdpCheckPool = null;

		public SocketReceivePeer ()
		{
			mNeedHandlePackageQueue = new ConcurrentQueue<NetPackage> ();
			mCombinePackagePool = new SafeObjectPool<NetCombinePackage> ();

			mUdpCheckPool = new UdpCheckPool (this as ClientPeer);
		}

		public NetCombinePackage GetNetCombinePackage()
		{
			return mCombinePackagePool.Pop ();
		}

		public void RecycleNetUdpFixedPackage(NetUdpFixedSizePackage mPackage)
		{
			ObjectPoolManager.Instance.mUdpFixedSizePackagePool.recycle (mPackage);
		}

		public void AddLogicHandleQueue (NetPackage mPackage)
		{
			mNeedHandlePackageQueue.Enqueue (mPackage);
		}

		public virtual void Update (double elapsed)
		{
			mUdpCheckPool.Update (elapsed);

			int nPackageCount = 0;
			while (mNeedHandlePackageQueue.Count > 0) {
				NetPackage mNetPackage = null;
				if (!mNeedHandlePackageQueue.TryDequeue (out mNetPackage)) {
					break;
				}
				DebugSystem.Assert (mNetPackage != null, "网络包 is Null ");
				PackageManager.Instance.Execute (this as ClientPeer, mNetPackage);

				if (mNetPackage is NetCombinePackage) {
					NetCombinePackage mCombinePackage = mNetPackage as NetCombinePackage;
					var iter = mCombinePackage.mNeedRecyclePackage.GetEnumerator ();
					while (iter.MoveNext ()) {
						RecycleNetUdpFixedPackage (iter.Current);
					}
					mCombinePackage.mNeedRecyclePackage.Clear ();
					mCombinePackagePool.recycle (mNetPackage as NetCombinePackage);
				} else if (mNetPackage is NetUdpFixedSizePackage) {
					RecycleNetUdpFixedPackage (mNetPackage as NetUdpFixedSizePackage);
				}

				nPackageCount++;
			}

			if (nPackageCount > 0) {
				//DebugSystem.Log ("服务器 处理逻辑的数量： " + nPackageCount);
			}
		}

		public void ReceiveUdpSocketFixedPackage (NetUdpFixedSizePackage mPackage)
		{
			bool bSucccess = NetPackageEncryption.DeEncryption (mPackage);
			if (bSucccess) {
				if (mPackage.nPackageId >= 50) {
					mUdpCheckPool.AddReceiveCheck (mPackage);
				} else {
					AddLogicHandleQueue (mPackage);
				}
			}
		}

		public virtual void release ()
		{

		}

	}
}