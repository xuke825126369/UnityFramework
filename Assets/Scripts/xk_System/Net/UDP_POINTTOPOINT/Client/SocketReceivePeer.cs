using System.Collections;
using System.Collections.Generic;
using xk_System.DataStructure;
using System;
using xk_System.Event;
using xk_System.Debug;
using System.Net.Sockets;
using Google.Protobuf;
using xk_System.Net.UDP.POINTTOPOINT.Protocol;

namespace xk_System.Net.UDP.POINTTOPOINT.Client
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
			while (mNeedHandlePackageQueue.Count > 0) {
				NetPackage mNetPackage = mNeedHandlePackageQueue.Dequeue ();
				mLogicFuncDic [mNetPackage.nPackageId] (mNetPackage);

				if (mNetPackage is NetCombinePackage) {
					mCanUseSortPackageList.Add (mNetPackage as NetCombinePackage);
				} else if (mNetPackage is NetUdpFixedSizePackage) {
					mUdpFixedSizePackagePool.recycle (mNetPackage as NetUdpFixedSizePackage);
				}
			}
		}

		protected void HandleReceivePackage ()
		{
			bool bSucccess = NetPackageEncryption.DeEncryption (mReceiveStream);
			if (bSucccess) {
				DebugSystem.Log ("客户端解析成功： " + mReceiveStream.Length);
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