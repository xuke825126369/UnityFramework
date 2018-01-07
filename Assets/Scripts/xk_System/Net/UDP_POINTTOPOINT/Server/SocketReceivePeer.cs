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
		protected ObjectPool<NetUdpFixedSizePackage> mUdpFixedSizePackagePool = null;
		protected List<NetCombinePackage> mCanUseSortPackageList = null;

		protected UdpCheckPool mUdpCheckPool = null;

		public SocketReceivePeer ()
		{
			mReceiveStream = new NetUdpFixedSizePackage ();
			mLogicFuncDic = new Dictionary<UInt16, Action<NetPackage>> ();

			mUdpFixedSizePackagePool = new ObjectPool<NetUdpFixedSizePackage> ();
			mCanUseSortPackageList = new List<NetCombinePackage> ();
			mNeedHandlePackageQueue = new Queue<NetPackage> ();
			mUdpCheckPool = new UdpCheckPool (this as ClientPeer);
		}

		public void AddLogicHandleQueue (NetPackage mPackage)
		{
			lock (mNeedHandlePackageQueue) {
				mNeedHandlePackageQueue.Enqueue (mPackage);
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
			int nPackageCount = 0;
			while (mNeedHandlePackageQueue.Count > 0) {
				NetPackage mNetPackage = mNeedHandlePackageQueue.Dequeue ();
				if (mLogicFuncDic.ContainsKey (mNetPackage.nPackageId)) {
					mLogicFuncDic [mNetPackage.nPackageId] (mNetPackage);
				} else {
					DebugSystem.LogError ("nPackageId 不在字典中： " + mNetPackage.nPackageId);
				}

				if (mNetPackage is NetCombinePackage) {
					mCanUseSortPackageList.Add (mNetPackage as NetCombinePackage);
				} else if (mNetPackage is NetUdpFixedSizePackage) {
					mUdpFixedSizePackagePool.recycle (mNetPackage as NetUdpFixedSizePackage);
				}

				nPackageCount++;
			}
		}

		protected void HandleReceivePackage ()
		{
			bool bSucccess = NetPackageEncryption.DeEncryption (mReceiveStream);

			if (bSucccess) {
				DebugSystem.Log ("Server 包： " + mReceiveStream.nGroupCount + " | " + mReceiveStream.nOrderId + " | " + mReceiveStream.nPackageId);
				if (mReceiveStream.nPackageId >= 50) {
					mUdpCheckPool.AddReceiveCheck (mReceiveStream);
				} else {
					AddLogicHandleQueue (mReceiveStream);
				}
			}

			mReceiveStream = mUdpFixedSizePackagePool.Pop ();
		}
			
		public virtual void release ()
		{
			
		}
	
	}
}