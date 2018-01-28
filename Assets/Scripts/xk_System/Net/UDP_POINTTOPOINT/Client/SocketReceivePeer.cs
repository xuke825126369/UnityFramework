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

		protected UdpCheckPool mUdpCheckPool = null;

		public SocketReceivePeer ()
		{
			mLogicFuncDic = new Dictionary<UInt16, Action<NetPackage>> ();
		
			mNeedHandlePackageQueue = new ConcurrentQueue<NetPackage> ();
			mUdpCheckPool = new UdpCheckPool (this as ClientPeer);
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

		private void NetPackageExecute(NetPackage mNetPackage)
		{
			mLogicFuncDic [mNetPackage.nPackageId] (mNetPackage);
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
				NetPackage mNetPackage = null;
				if (!mNeedHandlePackageQueue.TryDequeue (out mNetPackage)) {
					break;
				}

				NetPackageExecute (mNetPackage);

				if (mNetPackage is NetCombinePackage) {
					ObjectPoolManager.Instance.mCombinePackagePool.recycle (mNetPackage as NetCombinePackage);
				} else if (mNetPackage is NetUdpFixedSizePackage) {
					ObjectPoolManager.Instance.mUdpFixedSizePackagePool.recycle (mNetPackage as NetUdpFixedSizePackage);
				}

				nPackageCount++;
			}

			if (nPackageCount > 10) {
				DebugSystem.Log ("客户端 处理逻辑包的数量： " + nPackageCount);
			}

		}

		protected void ReceiveNetPackage (NetUdpFixedSizePackage mPackage)
		{
			bool bSucccess = NetPackageEncryption.DeEncryption (mPackage);
			if (bSucccess) {
				if (mPackage.nPackageId > 50) {
					mUdpCheckPool.ReceiveCheckPackage (mPackage);
				} else {
					NetPackageExecute (mPackage);
				}
			}
		}
			
		public virtual void release ()
		{
			
		}

	}
}