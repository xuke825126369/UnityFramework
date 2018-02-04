using System;
using System.Collections;
using System.Collections.Concurrent;
using xk_System.Debug;

namespace xk_System.Net.UDP.POINTTOPOINT.Server
{
	public abstract class SocketReceivePeer
	{
		protected ConcurrentQueue<NetCombinePackage> mNeedHandleCombinePackageQueue = null;
		protected ConcurrentQueue<NetPackage> mNeedHandlePackageQueue = null;

		protected UdpCheckPool mUdpCheckPool = null;

		public SocketReceivePeer ()
		{
			mNeedHandleCombinePackageQueue = new ConcurrentQueue<NetCombinePackage> ();
			mNeedHandlePackageQueue = new ConcurrentQueue<NetPackage> ();
			mUdpCheckPool = new UdpCheckPool (this as ClientPeer);
		}

		public void AddLogicHandleQueue(NetPackage mPackage)
		{
			mNeedHandlePackageQueue.Enqueue (mPackage);
		}

		public virtual void Update (double elapsed)
		{
			mUdpCheckPool.Update (elapsed);

			int nPackageCount = 0;
			while (!mNeedHandlePackageQueue.IsEmpty) {
				NetPackage mNetPackage = null;
				if (!mNeedHandlePackageQueue.TryDequeue (out mNetPackage)) {
					break;
				}

				PackageManager.Instance.Execute (this as ClientPeer, mNetPackage);

				if (mNetPackage is NetCombinePackage) {
					NetCombinePackage mCombinePackage = mNetPackage as NetCombinePackage;
					ObjectPoolManager.Instance.mCombinePackagePool.recycle (mNetPackage as NetCombinePackage);
				} else if (mNetPackage is NetUdpFixedSizePackage) {
					ObjectPoolManager.Instance.mUdpFixedSizePackagePool.recycle (mNetPackage as NetUdpFixedSizePackage);
				}

				nPackageCount++;
			}

			if (nPackageCount > 20) {
				DebugSystem.Log ("服务器 处理逻辑的数量： " + nPackageCount);
			}
		}

		public void ReceiveUdpSocketFixedPackage (NetUdpFixedSizePackage mPackage)
		{
			bool bSucccess = NetPackageEncryption.DeEncryption (mPackage);
			if (bSucccess) {
				if (mPackage.nPackageId > 50) {
					mUdpCheckPool.ReceiveCheckPackage (mPackage);
				} else {
					PackageManager.Instance.Execute (this as ClientPeer, mPackage);
				}
			} else {
				throw new Exception ("解码失败 !!!");
			}

		}


		public virtual void release ()
		{

		}

	}
}