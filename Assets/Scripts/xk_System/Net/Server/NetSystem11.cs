/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.Net;
using System;
using System.Net.Sockets;
using xk_System.Debug;

namespace xk_System.Net.Server
{
	public class NetSystem11
	{
		private NetSendSystem mNetSendSystem;
		private NetReceiveSystem mNetReceiveSystem;
		private SocketSystem mNetSocketSystem;

		public NetSystem11()
		{
			mNetSocketSystem = new SocketSystem_TCPServer ();
			mNetSendSystem = new NetSendSystem(mNetSocketSystem);
			mNetReceiveSystem = new NetReceiveSystem(mNetSocketSystem);
		}

		public void initNet(string ServerAddr, int ServerPort)
		{
			mNetSocketSystem.init (ServerAddr, ServerPort);
		}

		public void sendNetData(int clientId, int command, object package)
		{
			mNetSendSystem.SendNetData(clientId,command, package);  
		}

		public void handleNetData()
		{
			mNetSendSystem.HandleNetPackage ();
			mNetReceiveSystem.HandleNetPackage ();
		}

		public void addNetListenFun(Action<NetPackage> func)
		{
			mNetReceiveSystem.addListenFun (func);
		}

		public void removeNetListenFun(int Action<Package> fun)
		{
			mNetReceiveSystem.removeListenFun(command, fun);
		}

		public void closeNet()
		{
			mNetSocketSystem.CloseNet ();
			mNetSendSystem.Destory ();
			mNetReceiveSystem.Destory ();

			mNetSocketSystem = null;
			mNetSendSystem = null;
			mNetReceiveSystem = null;
		}
	}

	public abstract class SocketSystem
	{
		/// <summary>
		/// 不设置，则系统默认是8192
		/// </summary>
		protected const int receiveInfoPoolCapacity = 8192;
		protected const int sendInfoPoolCapacity = 8192;
		/// <summary>
		/// 毫秒数，不设置，系统默认为0
		/// </summary>
		protected const int receiveTimeOut = 3000;
		protected const int sendTimeOut = 3000;

		protected NetReceiveSystem mNetReceiveSystem;

		protected Socket mSocket;

		public abstract void init(string ServerAddr, int ServerPort);
		public abstract void SendNetStream(int clientId,byte[] msg);

		public bool IsPrepare()
		{
			return mSocket != null;
		}

		public void initSystem(NetReceiveSystem mNetReceiveSystem)
		{
			this.mNetReceiveSystem = mNetReceiveSystem;
		}

		public virtual void CloseNet()
		{
			if (mSocket != null) {
				mSocket.Close ();
				mSocket = null;
			}
			DebugSystem.Log("关闭 服务器 TCP连接");
		}
	}

	//begin~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~网络发送系统~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public class  NetSendSystem
	{
		protected Queue<Package> mNeedHandleNetPackageQueue;//消费者
		protected SocketSystem mSocketSystem;

		public NetSendSystem(SocketSystem mSocketSystem)
		{
			mNeedHandleNetPackageQueue = new Queue<Package> ();
			mPackagePool = new Queue<Package> ();
			this.mSocketSystem = mSocketSystem;
		}

		public virtual void SendNetData(int clientId, int command,object data)
		{
			Package mPackage = null;
			if (mPackagePool.Count == 0) {
				mPackage = new xk_Protobuf ();
			} else {
				mPackage = mPackagePool.Dequeue ();
			}

			mPackage.command = command;
			mPackage.data = data;
			mPackage.clientId = clientId;

			mNeedHandleNetPackageQueue.Enqueue (mPackage);
		}

		public void HandleNetPackage ()
		{
			int handlePackageCount = 0;
			while (mNeedHandleNetPackageQueue.Count > 0) 
			{
				var mPackage = mNeedHandleNetPackageQueue.Dequeue ();
				HandleNetStream (mPackage);
				mPackagePool.Enqueue(mPackage);
				handlePackageCount++;
			}

			if (handlePackageCount > 3) {
				DebugSystem.Log("服务器 发送包的数量： " + handlePackageCount);
			}
		}

		public void HandleNetStream(Package mPackage)
		{
			byte[] stream= mPackage.SerializePackage(mPackage.command, mPackage.data);
			stream = NetEncryptionStream.Encryption (stream);
			mSocketSystem.SendNetStream(mPackage.clientId,stream);
			mPackage.recycle ();
		}

		public virtual void Destory()
		{
			while (mNeedHandleNetPackageQueue.Count > 0) {
				Package mPackage = mNeedHandleNetPackageQueue.Dequeue ();
				mPackage.recycle ();
			}

			mNeedHandleNetPackageQueue.Clear ();

			while (mPackagePool.Count > 0) {
				Package mPackage = mPackagePool.Dequeue ();
				mPackage.recycle ();
			}
			mPackagePool.Clear ();
		}
	}

	//begin~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~网络接受系统~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public class NetReceiveSystem
	{
		
		public NetReceiveSystem(SocketSystem socketSys)
		{
			mReceiveDic = new Dictionary<int, Action<Package>>();
			mNeedHandlePackageQueue = new Queue<Package>();
			mCanUsePackageQueue = new Queue<Package>();
			mReceiveStreamQueue = new Queue<Package> ();
			mReceivedStreamDic = new Dictionary<int, List<byte>> ();
			socketSys.initSystem (this);
		}

		public void addListenFun(int command, Action<Package> fun)
		{
			lock (mReceiveDic)
			{
				if (mReceiveDic.ContainsKey(command))
				{
					if (CheckDataBindFunIsExist(command, fun))
					{
						DebugSystem.LogError("添加监听方法重复");
						return;
					}
					mReceiveDic[command] += fun;
				}
				else
				{
					mReceiveDic[command] = fun;
				}              
			}
		}

		private bool CheckDataBindFunIsExist(int command,Action<Package> fun)
		{
			Action<Package> mFunList = mReceiveDic[command];
			return DelegateUtility.CheckFunIsExist<Package>(mFunList, fun);
		}

		public void removeListenFun(int command, Action<Package> fun)
		{
			lock (mReceiveDic)
			{
				if (mReceiveDic.ContainsKey(command))
				{
					mReceiveDic[command]-=fun;
				}
			}
		}

		public void HandleNetPackage()
		{
			HandleSocketStream ();
			if (mNeedHandlePackageQueue != null) {
				while (mNeedHandlePackageQueue.Count > 0) {
					Package mPackage = mNeedHandlePackageQueue.Dequeue ();
					if (mReceiveDic.ContainsKey (mPackage.command)) {
						mReceiveDic [mPackage.command] (mPackage);
					} else {
						DebugSystem.LogError ("没有找到相关命令的处理函数：" + mPackage.command);
					}
					mPackage.recycle ();
					mCanUsePackageQueue.Enqueue (mPackage);
				}
			}
		}

		public void ReceiveSocketStream(int clientId,byte[] data)
		{
			Package mPackage = null;
			lock (mCanUsePackageQueue) {
				if (mCanUsePackageQueue.Count > 0) {
					mPackage = mCanUsePackageQueue.Dequeue ();
				} else {
					mPackage = new xk_Protobuf ();
				}
			}

			mPackage.clientId = clientId;
			mPackage.stream = data;

			lock (mReceiveStreamQueue) {
				mReceiveStreamQueue.Enqueue (mPackage);
			}
		}

		protected void HandleSocketStream ()
		{
			lock (mReceiveStreamQueue) {
				while (mReceiveStreamQueue.Count > 0) {
					var mPackage = mReceiveStreamQueue.Dequeue();
					if (!mReceivedStreamDic.ContainsKey (mPackage.clientId)) {
						mReceivedStreamDic [mPackage.clientId] = new List<byte> ();
					}
					mReceivedStreamDic [mPackage.clientId].AddRange(mPackage.stream);

					int clientId = mPackage.clientId;
					mPackage.recycle ();
					mCanUsePackageQueue.Enqueue (mPackage);

					while (true) {
						byte[] msg = mReceivedStreamDic [clientId].ToArray ();
						var data = NetEncryptionStream.DeEncryption (msg);
						if (data != null) {
							int Length = data.Length + 8;
							mReceivedStreamDic[clientId].RemoveRange (0, Length);

							Package mPackage1 = null;
							if (mCanUsePackageQueue.Count == 0) {
								mPackage1 = mCanUsePackageQueue.Dequeue();
							} else {
								mPackage1 = new xk_Protobuf ();
							}
							mPackage1.clientId = clientId;
							mPackage1.DeSerializeStream (data);
							mNeedHandlePackageQueue.Enqueue (mPackage1);

						} else {
							break;
						}
					}
				}
			}
		}

		public void RemoveClient(int clientId)
		{
			
		}

		public virtual void Destory()
		{
			lock(mReceiveStreamQueue)
			{
				while (mReceiveStreamQueue.Count > 0)
				{
					Package mPackage = mReceiveStreamQueue.Dequeue();
					mPackage.recycle();
				}
			}

			mReceivedStreamDic.Clear ();
			mReceiveDic.Clear ();

			lock(mNeedHandlePackageQueue)
			{
				while (mNeedHandlePackageQueue.Count > 0)
				{
					Package mPackage = mNeedHandlePackageQueue.Dequeue();
					mPackage.recycle();
				}
			}

			lock (mCanUsePackageQueue)
			{
				while (mCanUsePackageQueue.Count > 0)
				{
					Package mPackage = mCanUsePackageQueue.Dequeue();
					mPackage.recycle();
				}
			}
		}
	}

	//begin~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~网络包体结构系统~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public abstract class Package
	{
		public int clientId = -1;
		public int command = -1;
		public object data = null;
		public byte[] stream = null;

		public Package()
		{

		}

		public abstract byte[] SerializePackage(int command,object data);

		public abstract void DeSerializeStream(byte[] msg);

		public abstract T getData<T>() where T : new();

		public virtual void recycle()
		{
			data = null;
			stream = null;
			command = -1;
			clientId = -1;
		}
	}

}
*/