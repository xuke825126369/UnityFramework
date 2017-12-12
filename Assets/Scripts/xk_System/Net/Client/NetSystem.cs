using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.Debug;
using xk_System.Net.Client;
using System.Net.Sockets;
using xk_System.Net.Client.TCP;
using xk_System.Net;
using System;
using xk_System.DataStructure;

namespace xk_System.Net.Client
{
	public class NetSystem:NetEventInterface
	{
		private NetSendSystem mNetSendSystem;
		private NetReceiveSystem mNetReceiveSystem;
		private SocketSystem mNetSocketSystem;

		public NetSystem ()
		{
			mNetSocketSystem = new SocketSystem_Thread ();
			mNetSendSystem = new NetSendSystem (mNetSocketSystem);
			mNetReceiveSystem = new NetReceiveSystem (mNetSocketSystem);
		}

		public void initNet (string ServerAddr, int ServerPort)
		{
			mNetSocketSystem.init (ServerAddr, ServerPort);
		}

		public void sendNetData (int command, byte[] buffer)
		{
			NetPackage mPackage = NetObjectPool.Instance.mNetPackagePool.Pop ();
			mPackage.command = command;
			mPackage.buffer = buffer;
			mNetSendSystem.SendNetData (mPackage);  
		}

		public void handleNetData ()
		{
			mNetSocketSystem.Update ();
			mNetSendSystem.HandleNetPackage ();
			mNetReceiveSystem.HandleNetPackage ();
		}

		public void addNetListenFun (Action<NetPackage> fun)
		{
			mNetReceiveSystem.addListenFun (fun);
		}

		public void removeNetListenFun (Action<NetPackage> fun)
		{
			mNetReceiveSystem.removeListenFun (fun);
		}

		public void closeNet ()
		{
			mNetSocketSystem.CloseNet ();
			mNetSendSystem.Destory ();
			mNetReceiveSystem.Destory ();
		}
	}

	public abstract class SocketSystem
	{
		protected const int receiveInfoPoolCapacity = 8192;
		protected const int sendInfoPoolCapacity = 8192;
		protected const int receiveTimeOut = 10000;
		protected const int sendTimeOut = 5000;
		protected NetReceiveSystem mNetReceiveSystem;

		protected Socket mSocket;

		public abstract void init (string ServerAddr, int ServerPort);

		public abstract void SendNetStream (byte[] msg);

		public abstract void Update ();

		public bool IsPrepare ()
		{
			return mSocket != null;
		}

		public void initSystem (NetReceiveSystem mNetReceiveSystem)
		{
			this.mNetReceiveSystem = mNetReceiveSystem;
		}

		public virtual void CloseNet ()
		{
			if (mSocket != null) {
				mSocket.Close ();
				mSocket = null;
			}
			DebugSystem.Log ("关闭客户端TCP连接");
		}
	}

	//begin~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~网络发送系统~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public class  NetSendSystem
	{
		protected Queue<NetPackage> mNeedHandleNetPackageQueue;
		protected SocketSystem mSocketSystem;

		public NetSendSystem (SocketSystem socketSys)
		{
			this.mSocketSystem = socketSys;
			mNeedHandleNetPackageQueue = new Queue<NetPackage> ();
		}

		public virtual void SendNetData (NetPackage data)
		{
			mNeedHandleNetPackageQueue.Enqueue (data);
		}

		public void HandleNetPackage ()
		{
			int handlePackageCount = 0;
			while (mNeedHandleNetPackageQueue.Count > 0) {
				var mPackage = mNeedHandleNetPackageQueue.Dequeue ();
				HandleNetStream (mPackage);
				NetObjectPool.Instance.mNetPackagePool.recycle (mPackage);
				handlePackageCount++;

				if (handlePackageCount > 3) {
					//DebugSystem.LogError ("客户端 发送包的数量： " + handlePackageCount);
				}
			}
		}

		public void HandleNetStream (NetPackage mPackage)
		{
			byte[] stream = NetStream.GetOutStream (mPackage.command, mPackage.buffer);
			stream = NetEncryptionStream.Encryption (stream);
			mSocketSystem.SendNetStream (stream);
		}

		public virtual void Destory ()
		{
			lock (mNeedHandleNetPackageQueue) {
				mNeedHandleNetPackageQueue.Clear ();
			}
		}
	}

	//begin~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~网络接受系统~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public class NetReceiveSystem
	{
		protected CircularBuffer<byte> mReceiveStreamList;
		protected CircularBuffer<byte> mParseStreamList;
		protected Queue<NetPackage> mNeedHandlePackageQueue;
		protected Action<NetPackage> mReceiveFun;

		public NetReceiveSystem (SocketSystem socketSys)
		{
			mReceiveStreamList = new CircularBuffer<byte> (1024);
			mParseStreamList = new CircularBuffer<byte> (1024);
			mNeedHandlePackageQueue = new Queue<NetPackage> ();
			socketSys.initSystem (this);
		}

		public void addListenFun (Action<NetPackage> fun)
		{
			if (mReceiveFun != null) {
				if (CheckDataBindFunIsExist (fun)) {
					DebugSystem.LogError ("添加监听方法重复");
					return;
				}
				mReceiveFun += fun;
			} else {
				mReceiveFun = fun;
			}              
		}

		private bool CheckDataBindFunIsExist (Action<NetPackage> fun)
		{
			return DelegateUtility.CheckFunIsExist<NetPackage> (mReceiveFun, fun);
		}

		public void removeListenFun (Action<NetPackage> fun)
		{
			mReceiveFun -= fun;
		}

		public void HandleNetPackage ()
		{
			HandleSocketStream ();
			if (mNeedHandlePackageQueue != null) {
				while (mNeedHandlePackageQueue.Count > 0) {
					NetPackage mPackage = mNeedHandlePackageQueue.Dequeue ();
					if (mReceiveFun != null) {
						mReceiveFun (mPackage);
					}
					NetObjectPool.Instance.mNetPackagePool.recycle (mPackage);
				}
			}
		}

		public void ReceiveSocketStream (byte[] data)
		{
			lock (mReceiveStreamList) {
				mReceiveStreamList.WriteBuffer (data);
			}
		}

		protected void HandleSocketStream ()
		{
			lock (mReceiveStreamList) {
				mParseStreamList.WriteBuffer (mReceiveStreamList, 256);
			}

			int PackageCout = 0;
			while (mParseStreamList.Length > 0) {
				NetPackage mPackage = GetPackage ();
				if (mPackage != null) {
					mNeedHandlePackageQueue.Enqueue (mPackage);
					PackageCout++;

					if (PackageCout > 3) {
						DebugSystem.LogError ("客户端 解析包的数量： " + PackageCout);
					}
				} else {
					break;
				}
			}
		}

		private NetPackage GetPackage ()
		{
			NetPackage mPackage = NetObjectPool.Instance.mNetPackagePool.Pop ();
			bool bSucccess = NetEncryptionStream.DeEncryption (mParseStreamList, mPackage);
			if (bSucccess) {
				return mPackage;
			}

			return null;
		}

		public virtual void Destory ()
		{
			lock (mNeedHandlePackageQueue) {
				mNeedHandlePackageQueue.Clear ();
			}
		}
	}
}


