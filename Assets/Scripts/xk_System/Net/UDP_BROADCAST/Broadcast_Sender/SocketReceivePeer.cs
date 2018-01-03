using System.Collections;
using System.Collections.Generic;
using xk_System.DataStructure;
using System;
using xk_System.Event;
using xk_System.Debug;
using System.Net.Sockets;
using Google.Protobuf;
using xk_System.Net.UDP.BROADCAST.Protocol;

namespace xk_System.Net.UDP.BROADCAST.Client
{
	public class SocketReceivePeer
	{
		protected CircularBuffer<byte> mParseStreamList = null;
		protected Dictionary<UInt16, Action<NetReceivePackage>> mLogicFuncDic = null;

		protected ObjectPool<NetReceivePackage> mReceivePackagePool = null;
		protected Queue<NetReceivePackage> mReceivePackageQueue = null;

		private UInt16 nPackageOrderId;

		public SocketReceivePeer ()
		{
			mParseStreamList = new CircularBuffer<byte> (2 * ClientConfig.nMaxBufferSize);
			mLogicFuncDic = new Dictionary<UInt16, Action<NetReceivePackage>> ();

			mReceivePackagePool = new ObjectPool<NetReceivePackage> ();
			mReceivePackageQueue = new Queue<NetReceivePackage> ();

			nPackageOrderId = 1;
		}

		private void AddLogicHandleQueue (NetReceivePackage mPackage)
		{
			if (mLogicFuncDic.ContainsKey (mPackage.nUniqueId)) {
				mReceivePackageQueue.Enqueue (mPackage);
			} else {
				DebugSystem.LogError ("不存在的 协议ID: " + mPackage.nUniqueId);
			}
		}

		public void addNetListenFun (UInt16 command, Action<NetReceivePackage> func)
		{
			if (!mLogicFuncDic.ContainsKey (command)) {
				mLogicFuncDic [command] = func;
			} else {
				mLogicFuncDic [command] += func;
			}
		}

		public void removeNetListenFun (UInt16 command, Action<NetReceivePackage> func)
		{
			if (mLogicFuncDic.ContainsKey (command)) {
				mLogicFuncDic [command] -= func;
			}
		}

		public void ReceiveSocketStream (byte[] data, int index, int Length)
		{
			lock (mParseStreamList) {
				mParseStreamList.WriteFrom (data, index, Length);
			}
		}

		public virtual void Update (double elapsed)
		{
			HandleReceivePackage ();
			while (mReceivePackageQueue.Count > 0) {
				NetReceivePackage mNetReceivePackage = mReceivePackageQueue.Dequeue ();
				mLogicFuncDic [mNetReceivePackage.nUniqueId] (mNetReceivePackage);
				mReceivePackagePool.recycle (mNetReceivePackage);
			}
		}

		private void HandleReceivePackage ()
		{
			int PackageCout = 0;

			while (GetPackage ()) {
				PackageCout++;
			}

			if (PackageCout == 0) {
				if (mParseStreamList.Length > 0) {
					DebugSystem.LogError ("客户端 正在解包: " + mParseStreamList.Length + " | " + mParseStreamList.Capacity);
				}
			}
		}

		private bool GetPackage ()
		{
			if (mParseStreamList.Length <= 0) {
				return false;
			}

			NetReceivePackage mNetReceivePackage = mReceivePackagePool.Pop ();
			bool bSucccess = false;
			lock (mParseStreamList) {
				bSucccess = NetEncryptionStream.DeEncryption (mParseStreamList, mNetReceivePackage);
			}

			if (bSucccess) {
				AddLogicHandleQueue (mNetReceivePackage);
			}

			return bSucccess;
		}
			
		public void release ()
		{
				
		}
	}
}