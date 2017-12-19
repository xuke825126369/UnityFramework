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
	public class NetSystem :SocketSystem_TCPServer, NetEventInterface
	{
		public void initNet (string ServerAddr, int ServerPort)
		{
			base.InitNet (ServerAddr, ServerPort);
		}

		public void sendNetData (int clientId, int command, byte[] buffer)
		{
			base.mNetSendSystem.SendNetData (clientId, command, buffer);  
		}

		public void handleNetData ()
		{
			base.HandleNetPackage ();
		}

		public void addNetListenFun (Action<NetPackage> fun)
		{
			base.mNetReceiveSystem.addListenFun (fun);
		}

		public void removeNetListenFun (Action<NetPackage> fun)
		{
			base.mNetReceiveSystem.removeListenFun (fun);
		}

		public void closeNet ()
		{
			base.CloseNet ();
		}
	}

	public class SocketConfig
	{
		protected const int receiveInfoPoolCapacity = 10;
		protected const int sendInfoPoolCapacity = 10;
		protected const int receiveTimeOut = 5000;
		protected const int sendTimeOut = 5000;

		public virtual void ConfigureSocket (Socket mSocket)
		{
			mSocket.ExclusiveAddressUse = true;
			mSocket.LingerState = new LingerOption (true, 10);
			mSocket.NoDelay = false;

			mSocket.ReceiveBufferSize = 8192;
			mSocket.ReceiveTimeout = 1000;
			mSocket.SendBufferSize = 8192;
			mSocket.SendTimeout = 1000;

			mSocket.Blocking = false;
			mSocket.Ttl = 42;

			mSocket.SetSocketOption (SocketOptionLevel.Tcp, SocketOptionName.MaxConnections, 100);
		}

		public void PrintSocketConfigInfo (Socket mSocket)
		{
			DebugSystem.Log ("------------------- Socket Config ------------------------ ");
			DebugSystem.Log ("ExclusiveAddressUse :" + mSocket.ExclusiveAddressUse);
			DebugSystem.Log ("LingerState: " + mSocket.LingerState.Enabled + " | " + mSocket.LingerState.LingerTime);
			DebugSystem.Log ("Ttl: " + mSocket.Ttl);
			DebugSystem.Log ("NoDelay: " + mSocket.NoDelay);

			DebugSystem.Log ("Block: " + mSocket.Blocking);
			DebugSystem.Log ("ReceiveTimeout: " + mSocket.ReceiveTimeout);
			DebugSystem.Log ("SendTimeout: " + mSocket.SendTimeout);

			DebugSystem.Log ("ReceiveBufferSize: " + mSocket.ReceiveBufferSize);
			DebugSystem.Log ("SendBufferSize: " + mSocket.SendBufferSize);
			DebugSystem.Log ("---------------- Finish -------------------");
		}

		public void PrintSocketState(Socket mSocket)
		{
			DebugSystem.Log ("------------------- Socket State ------------------------ ");
			DebugSystem.Log ("IsBound: " + mSocket.IsBound);
			DebugSystem.Log ("Connected: " + mSocket.Connected);
			DebugSystem.Log ("---------------- Finish -------------------");
		}

	}

	public class SocketSystem: SocketConfig
	{
		protected NetReceiveSystem mNetReceiveSystem;
		protected NetSendSystem mNetSendSystem;

		public virtual void InitNet (string ServerAddr, int ServerPort)
		{

		}

		public virtual void SendNetStream (int clientId,ArraySegment<byte> buffer)
		{

		}

		public virtual void HandleNetPackage()
		{
			mNetSendSystem.HandleNetPackage ();
			mNetReceiveSystem.HandleNetPackage ();
		}

		public virtual void CloseNet ()
		{
			mNetSendSystem.Destory ();
			mNetReceiveSystem.Destory ();
		}
	}

	//begin~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~网络发送系统~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public class  NetSendSystem
	{
		protected SocketSystem mSocketSystem;
		protected NetPackage mPackage = null;

		public NetSendSystem (SocketSystem socketSys)
		{
			this.mSocketSystem = socketSys;
			mPackage = new NetPackage ();
		}

		public virtual void SendNetData (int clientId, int command, byte[] buffer)
		{
			mPackage.clientId = clientId;
			mPackage.command = command;
			mPackage.buffer = buffer;

			ArraySegment<byte> stream = NetEncryptionStream.Encryption (mPackage);
			mSocketSystem.SendNetStream (mPackage.clientId, stream);
		}

		public void HandleNetPackage ()
		{
			
		}

		public virtual void Destory ()
		{
			
		}
	}

	//begin~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~网络接受系统~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public class NetReceiveSystem
	{
		protected Dictionary<int, CircularBuffer<byte>> mReceivedStreamDic;
		protected Queue<ClientNetBuffer> mReceiveStreamQueue;
		protected ObjectPool<ClientNetBuffer> mCanUsePackageQueue;
		protected NetPackage mReceivePackage;

		protected Action<NetPackage> mReceiveFun;

		public NetReceiveSystem (SocketSystem socketSys)
		{
			mReceivedStreamDic = new Dictionary<int, CircularBuffer<byte>> ();
			mReceiveStreamQueue = new Queue<ClientNetBuffer> ();
			mCanUsePackageQueue = new ObjectPool<ClientNetBuffer> ();
			mReceivePackage = new NetPackage ();
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
			lock (mReceiveStreamQueue) {
				while (mReceiveStreamQueue.Count > 0) {
					ClientNetBuffer mPackage = null;
					lock (mReceiveStreamQueue) {
						mPackage = mReceiveStreamQueue.Peek ();
					}

					if (!mReceivedStreamDic.ContainsKey (mPackage.ClientId)) {
						int BufferSize = 2 * ServerConfig.receiveBufferSize;
						mReceivedStreamDic [mPackage.ClientId] = new CircularBuffer<byte> (BufferSize);
					}

					if (mReceivedStreamDic [mPackage.ClientId].isCanWriteFrom (mPackage.Length)) {
						mPackage = mReceiveStreamQueue.Dequeue ();
						mReceivedStreamDic [mPackage.ClientId].WriteFrom (mPackage.Buffer, 0, mPackage.Length);

						while (GetPackage (mPackage.ClientId)) {
							nPackageCount++;
						}

						lock (mCanUsePackageQueue) {
							mCanUsePackageQueue.recycle (mPackage);
						}
					} else {
						//DebugSystem.Log ("PackageSize111: " + mPackage.Length + " | " + mReceivedStreamDic [mPackage.ClientId].Capacity);
						break;
					}
				}

				if (nPackageCount > 10) {
					DebugSystem.Log ("Server 处理的网络包数量：" + nPackageCount);
				}
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


