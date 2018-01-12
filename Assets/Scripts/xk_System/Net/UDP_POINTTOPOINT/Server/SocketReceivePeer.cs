using System.Collections;
using System.Collections.Generic;
using xk_System.DataStructure;
using System;
using xk_System.Event;
using xk_System.Debug;
using System.Net.Sockets;
using Google.Protobuf;
using xk_System.Net.UDP.POINTTOPOINT.Protocol;

namespace xk_System.Net.UDP.POINTTOPOINT.Server
{
	public abstract class SocketReceivePeer
	{
		protected Dictionary<UInt16, Action<NetPackage>> mLogicFuncDic = null;
		protected Queue<NetPackage> mNeedHandlePackageQueue = null;

		protected NetUdpFixedSizePackage mReceiveStream = null;
		protected SafeObjectPool<NetUdpFixedSizePackage> mUdpFixedSizePackagePool = null;
		protected SafeObjectPool<NetCombinePackage> mCombinePackagePool = null;

		protected UdpCheckPool mUdpCheckPool = null;

		public SocketReceivePeer ()
		{
			mLogicFuncDic = new Dictionary<UInt16, Action<NetPackage>> ();
			mNeedHandlePackageQueue = new Queue<NetPackage> ();

			mUdpFixedSizePackagePool = new SafeObjectPool<NetUdpFixedSizePackage> ();
			mCombinePackagePool = new SafeObjectPool<NetCombinePackage> ();

			mUdpCheckPool = new UdpCheckPool (this as ClientPeer);
		}

		public NetCombinePackage GetNetCombinePackage()
		{
			return mCombinePackagePool.Pop ();
		}

		public void RecycleNetUdpFixedPackage(NetUdpFixedSizePackage mPackage)
		{
			mUdpFixedSizePackagePool.recycle (mPackage);
		}

		public void AddLogicHandleQueue (NetPackage mPackage)
		{
			if (mLogicFuncDic.ContainsKey (mPackage.nPackageId)) {
				mNeedHandlePackageQueue.Enqueue (mPackage);
			} else {
				DebugSystem.LogError ("不存在的 协议ID: " + mPackage.nPackageId);
			}
		}

		public void addNetListenFun (UInt16 command, Action<NetPackage> func)
		{
			if (!mLogicFuncDic.ContainsKey (command)) {
				mLogicFuncDic [command] = func;
			} else {
				mLogicFuncDic [command] += func;
			}
		}

		public void removeNetListenFun (UInt16 command, Action<NetPackage> func)
		{
			if (mLogicFuncDic.ContainsKey (command)) {
				mLogicFuncDic [command] -= func;
			}
		}

		public virtual void Update (double elapsed)
		{
			mUdpCheckPool.Update (elapsed);

			int nPackageCount = 0;
			while (mNeedHandlePackageQueue.Count > 0) {
				NetPackage mNetPackage = mNeedHandlePackageQueue.Dequeue ();
				DebugSystem.Assert (mNetPackage != null, "网络包 is Null ");
				mLogicFuncDic [mNetPackage.nPackageId] (mNetPackage);

				if (mNetPackage is NetCombinePackage) {
					NetCombinePackage mCombinePackage = mNetPackage as NetCombinePackage;
					var iter = mCombinePackage.mNeedRecyclePackage.GetEnumerator ();
					while (iter.MoveNext ()) {
						mUdpFixedSizePackagePool.recycle (iter.Current);
					}
					mCombinePackage.mNeedRecyclePackage.Clear ();
					mCombinePackagePool.recycle (mNetPackage as NetCombinePackage);
				} else if (mNetPackage is NetUdpFixedSizePackage) {
					mUdpFixedSizePackagePool.recycle (mNetPackage as NetUdpFixedSizePackage);
				}

				nPackageCount++;
			}

			if (nPackageCount > 0) {
				//DebugSystem.Log ("服务器 处理逻辑的数量： " + nPackageCount);
			}
		}

		protected void HandleReceivePackage ()
		{
			bool bSucccess = NetPackageEncryption.DeEncryption (mReceiveStream);
			if (bSucccess) {
				if (mReceiveStream.nPackageId >= 50) {
					mUdpCheckPool.AddReceiveCheck (mReceiveStream);
				} else {
					AddLogicHandleQueue (mReceiveStream);
				}
			}
		}

		public virtual void release ()
		{

		}

	}
}