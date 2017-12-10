using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.Debug;
using System.Net.Sockets;
using xk_System.Net;
using System;
using xk_System.Net.Server.Event;

namespace xk_System.Net.Server
{
	public class NetSystem :NetEventInterface
	{
		private NetSendSystem mNetSendSystem;
		private NetReceiveSystem mNetReceiveSystem;
		private SocketSystem mNetSocketSystem;

		public NetSystem ()
		{
			mNetSocketSystem = new SocketSystem_TCPServer ();
			mNetSendSystem = new NetSendSystem (mNetSocketSystem);
			mNetReceiveSystem = new NetReceiveSystem (mNetSocketSystem);
		}

		public void initNet (string ServerAddr, int ServerPort)
		{
			mNetSocketSystem.init (ServerAddr, ServerPort);
		}

		public void sendNetData (int socketId, int command, byte[] buffer)
		{
			mNetSendSystem.SendNetData (socketId, command, buffer);  
		}

		public void handleNetData ()
		{
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

		public abstract void SendNetStream (int socketId,byte[] msg);

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
		protected ObjectPool<NetPackage> mCanUsePackagePool;

		public NetSendSystem (SocketSystem socketSys)
		{
			this.mSocketSystem = socketSys;
			mNeedHandleNetPackageQueue = new Queue<NetPackage> ();
			mCanUsePackagePool = new ObjectPool<NetPackage> ();
		}

		public virtual void SendNetData (int socketId,int command,byte[] buffer)
		{
			NetPackage mPackage = mCanUsePackagePool.Pop ();
			mPackage.socketId = socketId;
			mPackage.command = command;
			mPackage.buffer = buffer;
			mNeedHandleNetPackageQueue.Enqueue (mPackage);
		}

		public void HandleNetPackage ()
		{
			int handlePackageCount = 0;
			while (mNeedHandleNetPackageQueue.Count > 0) {
				var mPackage = mNeedHandleNetPackageQueue.Dequeue ();
				HandleNetStream (mPackage);
				mCanUsePackagePool.recycle (mPackage);
				handlePackageCount++;
			}

			if (handlePackageCount > 3) {
				//DebugSystem.LogError ("服务器 发送包的数量： " + handlePackageCount);
			}
		}

		public void HandleNetStream (NetPackage mPackage)
		{
			byte[] stream = NetStream.GetOutStream (mPackage.command, mPackage.buffer);
			stream = NetEncryptionStream.Encryption (stream);
			mSocketSystem.SendNetStream (mPackage.socketId, stream);
		}

		public virtual void Destory ()
		{
			mNeedHandleNetPackageQueue.Clear ();
			mCanUsePackagePool.release ();
		}
	}

	//begin~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~网络接受系统~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public class NetReceiveSystem
	{
		protected Dictionary<int,List<byte>> mReceivedStreamDic;

		protected Queue<NetPackage> mReceiveStreamQueue;
		protected ObjectPool<NetPackage> mReceivePackagePool;

		protected Queue<NetPackage> mNeedHandlePackageQueue;
		protected ObjectPool<NetPackage> mCanUsePackageQueue;

		protected Action<NetPackage> mReceiveFun;

		public NetReceiveSystem (SocketSystem socketSys)
		{
			mReceivedStreamDic = new Dictionary<int, List<byte>> ();
			mReceiveStreamQueue = new Queue<NetPackage> ();
			mReceivePackagePool = new ObjectPool<NetPackage> ();
			mNeedHandlePackageQueue = new Queue<NetPackage> ();
			mCanUsePackageQueue = new ObjectPool<NetPackage> ();
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
					mCanUsePackageQueue.recycle (mPackage);
				}
			}
		}

		public void ReceiveSocketStream (int socketId, byte[] buffer)
		{
			NetPackage mPackage = null;
			lock (mReceivePackagePool) {
				mPackage = mReceivePackagePool.Pop ();
				mPackage.socketId = socketId;
				mPackage.buffer = buffer;
			}

			lock (mReceiveStreamQueue) {
				mReceiveStreamQueue.Enqueue (mPackage);
			}
		}

		protected void HandleSocketStream ()
		{
			while (mReceiveStreamQueue.Count > 0) {
				NetPackage mPackage = null;
				lock (mReceivePackagePool) {
					mPackage = mReceiveStreamQueue.Dequeue ();
				}
				if (!mReceivedStreamDic.ContainsKey (mPackage.socketId)) {
					mReceivedStreamDic [mPackage.socketId] = new List<byte> ();
				}
				mReceivedStreamDic [mPackage.socketId].AddRange (mPackage.buffer);
				int socketId = mPackage.socketId;
				lock (mReceivePackagePool) {
					mReceivePackagePool.recycle (mPackage);
				}
				mPackage = null;

				while (true) {
					mPackage = GetPackage (socketId);
					if (mPackage != null) {
						mNeedHandlePackageQueue.Enqueue (mPackage);
					} else {
						break;
					}
				}
			}
		}

		private NetPackage GetPackage (int socketId)
		{
			byte[] msg = mReceivedStreamDic [socketId].ToArray ();
			byte[] data = NetEncryptionStream.DeEncryption (msg);
			if (data == null) {
				return null;
			}

			int Length = data.Length + 8;
			mReceivedStreamDic [socketId].RemoveRange (0, Length);

			NetPackage mPackage = mCanUsePackageQueue.Pop ();
			NetStream.GetInputStream (data, out mPackage.command, out mPackage.buffer);
			mPackage.socketId = socketId;
			return mPackage;
		}

		public virtual void Destory ()
		{
			lock (mReceivePackagePool) {
				mReceivePackagePool.release ();
			}

			mNeedHandlePackageQueue.Clear ();
			mCanUsePackageQueue.release ();
			mReceivedStreamDic.Clear ();
			mNeedHandlePackageQueue.Clear ();
		}
	}
}


