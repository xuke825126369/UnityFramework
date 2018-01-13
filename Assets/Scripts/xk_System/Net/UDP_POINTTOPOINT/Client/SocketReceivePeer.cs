using System.Collections;
using System.Collections.Generic;
using xk_System.DataStructure;
using System;
using xk_System.Event;
using xk_System.Debug;
using System.Net.Sockets;
using Google.Protobuf;
using xk_System.Net.UDP.POINTTOPOINT.Protocol;
using System.Collections.Concurrent;

namespace xk_System.Net.UDP.POINTTOPOINT.Client
{
	public abstract class SocketReceivePeer
	{
		protected Dictionary<UInt16, Action<NetPackage>> mLogicFuncDic = null;
		protected ConcurrentQueue<NetPackage> mNeedHandlePackageQueue = null;

		protected ConcurrentQueue<NetUdpFixedSizePackage> mReceiveSocketPackageQueue = null;

		protected SafeObjectPool<NetUdpFixedSizePackage> mReceivePackagePool = null;
		protected SafeObjectPool<NetCombinePackage> mReceiveCombinePackagePool = null;

		protected UdpCheckPool mUdpCheckPool = null;

		public SocketReceivePeer ()
		{
			mLogicFuncDic = new Dictionary<UInt16, Action<NetPackage>> ();

			mReceiveSocketPackageQueue = new ConcurrentQueue<NetUdpFixedSizePackage> ();
			mReceivePackagePool = new SafeObjectPool<NetUdpFixedSizePackage> ();
			mReceiveCombinePackagePool = new SafeObjectPool<NetCombinePackage> ();

			mNeedHandlePackageQueue = new ConcurrentQueue<NetPackage> ();
			mUdpCheckPool = new UdpCheckPool (this as ClientPeer);
		}

		public NetCombinePackage GetNetCombinePackage()
		{
			return mReceiveCombinePackagePool.Pop ();
		}

		public NetUdpFixedSizePackage GetNetUdpFixedPackage()
		{
			return mReceivePackagePool.Pop ();
		}

		public void RecycleReceivePackage(NetUdpFixedSizePackage mPackage)
		{
			mReceivePackagePool.recycle (mPackage);
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

			while (!mReceiveSocketPackageQueue.IsEmpty) {
				NetUdpFixedSizePackage mPackage = null;
				if (!mReceiveSocketPackageQueue.TryDequeue (out mPackage)) {
					DebugSystem.LogError ("Dequeue Error");
				}

				bool bSucccess = NetPackageEncryption.DeEncryption (mPackage);
				if (bSucccess) {
					if (mPackage.nPackageId >= 50) {
						mUdpCheckPool.AddReceiveCheck (mPackage);
					} else {
						AddLogicHandleQueue (mPackage);
					}
				}

			}

			int nPackageCount = 0;
			while (mNeedHandlePackageQueue.Count > 0) {
				NetPackage mNetPackage = null;
				if (!mNeedHandlePackageQueue.TryDequeue (out mNetPackage)) {
					DebugSystem.Log ("Dequeue 失败");
				}

				mLogicFuncDic [mNetPackage.nPackageId] (mNetPackage);

				if (mNetPackage is NetCombinePackage) {
					NetCombinePackage mCombinePackage = mNetPackage as NetCombinePackage;
					var iter = mCombinePackage.mNeedRecyclePackage.GetEnumerator ();
					while (iter.MoveNext ()) {
						RecycleReceivePackage (iter.Current);
					}
					mCombinePackage.mNeedRecyclePackage.Clear ();
					mReceiveCombinePackagePool.recycle (mNetPackage as NetCombinePackage);
				} else if (mNetPackage is NetUdpFixedSizePackage) {
					RecycleReceivePackage (mNetPackage as NetUdpFixedSizePackage);
				}

				nPackageCount++;
			}

			if (nPackageCount > 10) {
				//DebugSystem.Log ("客户端 处理逻辑包的数量： " + nPackageCount);
			}
		}

		protected void ReceiveNetPackage (NetUdpFixedSizePackage mPackage)
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