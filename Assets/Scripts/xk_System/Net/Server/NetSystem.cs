using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.Debug;
using System.Net.Sockets;
using xk_System.Net;
using System;
using xk_System.Net.Server.Event;
using xk_System.DataStructure;

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

		public void sendNetData (int clientId, int command, byte[] buffer)
		{
			mNetSendSystem.SendNetData (clientId, command, buffer);  
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

		public abstract void SendNetStream (int clientId,byte[] msg);

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
			DebugSystem.Log ("关闭 服务器 TCP连接");
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

		public virtual void SendNetData (int clientId, int command, byte[] buffer)
		{
			NetPackage mPackage = mCanUsePackagePool.Pop ();
			mPackage.clientId = clientId;
			mPackage.command = command;
			mPackage.buffer = buffer;
			mPackage.Length = buffer.Length;
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

				if (handlePackageCount >= ServerConfig.nPerFrameHandlePackageCount) {
					break;
				}
			}

			if (handlePackageCount > 10) {
				//DebugSystem.Log ("服务器 发送包的数量： " + handlePackageCount);
			}
		}

		public void HandleNetStream (NetPackage mPackage)
		{
			byte[] stream = NetEncryptionStream.Encryption (mPackage);
			mSocketSystem.SendNetStream (mPackage.clientId, stream);
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
		protected Dictionary<int,CircularBuffer<byte>> mReceivedStreamDic;
		protected Queue<ClientNetBuffer> mReceiveStreamQueue;
		protected ObjectPool<ClientNetBuffer> mCanUsePackageQueue;
		protected NetPackage mReceivePackage;

		protected Action<NetPackage> mReceiveFun;

		public NetReceiveSystem (SocketSystem socketSys)
		{
			mReceivedStreamDic = new Dictionary<int, CircularBuffer<byte>> ();
			mReceiveStreamQueue = new Queue<ClientNetBuffer> ();;
			mCanUsePackageQueue = new ObjectPool<ClientNetBuffer>();

			mReceivePackage = new NetPackage ();
			socketSys.initSystem (this);
		}

		public void addListenFun (Action<NetPackage> fun)
		{
			if (mReceiveFun != null) {
				if (CheckDataBindFunIsExist (fun)) {
					DebugSystem.Log ("添加监听方法重复");
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

		public void ReceiveSocketStream (int clientId, byte[] buffer,int offset,int Length)
		{
			ClientNetBuffer mPackage = null;
			lock (mCanUsePackageQueue) {
				mPackage = mCanUsePackageQueue.Pop ();
			}

			lock (mReceiveStreamQueue) {
				mPackage.WriteFrom (clientId, buffer, offset, Length);
				mReceiveStreamQueue.Enqueue (mPackage);
			}
		}

		public void HandleNetPackage ()
		{
			int nPackageCount = 0;
			while (mReceiveStreamQueue.Count > 0) {
				ClientNetBuffer mPackage = null;
				lock (mReceiveStreamQueue) {
					mPackage = mReceiveStreamQueue.Dequeue ();
				}

				if (!mReceivedStreamDic.ContainsKey (mPackage.ClientId)) {
					int BufferSize = ServerConfig.nMaxPackageSize * ServerConfig.nPerFrameHandlePackageCount;
					mReceivedStreamDic [mPackage.ClientId] = new CircularBuffer<byte> (BufferSize);
				}

				mReceivedStreamDic [mPackage.ClientId].WriteFrom (mPackage.Buffer, 0, mPackage.Length);

				while (GetPackage (mPackage.ClientId)) {
					nPackageCount++;
					if (nPackageCount >= ServerConfig.nPerFrameHandlePackageCount) {
						break;
					}
				}

				lock (mCanUsePackageQueue) {
					mCanUsePackageQueue.recycle (mPackage);
				}

				if (nPackageCount >= ServerConfig.nPerFrameHandlePackageCount) {
					break;
				}
			}

			if (nPackageCount > 10) {
				//DebugSystem.Log ("Server 处理的网络包数量：" + nPackageCount);
			}
		}

		private bool GetPackage (int clientId)
		{
			CircularBuffer<byte> msg = mReceivedStreamDic [clientId];
			if (msg.Length <= 0) {
				return false;
			}
				
			var bSuccess = NetEncryptionStream.DeEncryption (msg, mReceivePackage);
			if (bSuccess == true) {
				mReceivePackage.clientId = clientId;
				if (mReceiveFun != null) {
					mReceiveFun (mReceivePackage);
				}
			}
				
			return bSuccess;
		}

		public virtual void Destory ()
		{
			lock (mCanUsePackageQueue) {
				mCanUsePackageQueue.release ();
			}
			lock (mReceiveStreamQueue) {
				mReceiveStreamQueue.Clear ();
			}
			mReceivedStreamDic.Clear ();
		}
	}
}


