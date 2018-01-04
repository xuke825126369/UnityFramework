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
			
		protected ListBuffer<byte> mReceiveStream = null;
		protected ObjectPool<NetUdpFixedSizePackage> mUdpFixedSizePackagePool = null;
		protected List<NetCombinePackage> mCanUseSortPackageList = null;

		protected UdpCheckPool mUdpCheckPool = null;

		public SocketReceivePeer ()
		{
			mReceiveStream = new ListBuffer<byte> (ClientConfig.nUdpPackageFixedSize);
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
			int PackageCout = 0;

			while (GetPackage ()) {
				PackageCout++;
			}

			if (PackageCout == 0) {
				if (mReceiveStream.Length > 0) {
					DebugSystem.LogError ("客户端 正在解包: " + mReceiveStream.Length);
				}
			}
		}

		private bool GetPackage ()
		{
			if (mReceiveStream.Length <= 0) {
				return false;
			}

			NetUdpFixedSizePackage mNetPackage = mUdpFixedSizePackagePool.Pop ();
			bool bSucccess = NetPackageEncryption.DeEncryption (mReceiveStream, mNetPackage);

			if (bSucccess) {
				
				if (mNetPackage.nPackageId >= 50) {
					mUdpCheckPool.AddReceiveCheck (mNetPackage);
				} else {
					AddLogicHandleQueue (mNetPackage);
				}
			}

			return bSucccess;
		}
			
		public virtual void release ()
		{
			
		}
	}
}