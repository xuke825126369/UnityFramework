using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.Debug;
using xk_System.Net.Client;
using System.Net.Sockets;
using xk_System.Net.Client.TCP;
using xk_System.Net;
using System;

namespace xk_System.Net.Client
{
	public class NetSystem : Singleton<NetSystem>
	{
		private NetSendSystem mNetSendSystem;
		private NetReceiveSystem mNetReceiveSystem;
		private SocketSystem mNetSocketSystem;

		public NetSystem()
		{
			mNetSocketSystem = new SocketSystem_Thread ();

			mNetSendSystem = new NetSendSystem(mNetSocketSystem);
			mNetReceiveSystem = new NetReceiveSystem(mNetSocketSystem);
		}

		public void initNet(string ServerAddr, int ServerPort)
		{
			mNetSocketSystem.init (ServerAddr, ServerPort);
		}

		public void sendNetData(int command, object package)
		{
			mNetSendSystem.SendNetData(command, package);  
		}

		//每帧处理一些事件
		public void handleNetData()
		{
			mNetSendSystem.HandleNetPackage ();
			mNetReceiveSystem.HandleNetPackage ();
		}

		public void addNetListenFun(int command, Action<Package> fun)
		{
			mNetReceiveSystem.addListenFun(command,fun);
		}

		public void removeNetListenFun(int command, Action<Package> fun)
		{
			mNetReceiveSystem.removeListenFun(command, fun);
		}

		public void closeNet()
		{
			mNetSocketSystem.CloseNet ();
			mNetSendSystem.Destory ();
			mNetReceiveSystem.Destory ();
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
		protected const int receiveTimeOut = 10000;
		protected const int sendTimeOut = 5000;

		protected NetReceiveSystem mNetReceiveSystem;

		protected Socket mSocket;

		public abstract void init(string ServerAddr, int ServerPort);
		public abstract void SendNetStream(byte[] msg);

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
			DebugSystem.Log ("关闭客户端TCP连接");
		}

	}

	//begin~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~网络发送系统~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public class  NetSendSystem
	{
		protected Queue<Package> mNeedHandleNetPackageQueue;
		protected Queue<Package> mCanUseNetPackageQueue;
		protected SocketSystem mSocketSystem;

		public NetSendSystem(SocketSystem socketSys)
		{
			this.mSocketSystem = socketSys;
			mNeedHandleNetPackageQueue = new Queue<Package> ();
			mCanUseNetPackageQueue = new Queue<Package> ();
		}

		public virtual void SendNetData(int command,object data)
		{
			if (!mSocketSystem.IsPrepare ()) {
				DebugSystem.Log ("发送消息 被 挡住");
				return;
			}

			Package mPackage = null;
			if (mCanUseNetPackageQueue.Count> 0 )
			{
				mPackage = mCanUseNetPackageQueue.Dequeue ();
			}
			else
			{
				mPackage = new xk_Protobuf ();
			}

			mPackage.command = command;
			mPackage.data = data;

			mNeedHandleNetPackageQueue.Enqueue (mPackage);
		}

		public void HandleNetPackage ()
		{
			int handlePackageCount = 0;
			while (mNeedHandleNetPackageQueue.Count > 0) 
			{
				var mPackage = mNeedHandleNetPackageQueue.Dequeue ();
				HandleNetStream (mPackage);
				mCanUseNetPackageQueue.Enqueue (mPackage);
				handlePackageCount++;
			}

			if (handlePackageCount > 3) {
				DebugSystem.LogError ("客户端 发送包的数量： " + handlePackageCount);
			}
		}

		public void HandleNetStream(Package mPackage)
		{
			byte[] stream= mPackage.SerializePackage(mPackage.command, mPackage.data);
			stream = NetEncryptionStream.Encryption (stream);
			mSocketSystem.SendNetStream (stream);
			mPackage.reset ();
		}
			
		public virtual void Destory()
		{
			lock(mNeedHandleNetPackageQueue)
			{
				while (mNeedHandleNetPackageQueue.Count > 0)
				{
					Package mPackage = mNeedHandleNetPackageQueue.Dequeue();
					mPackage.reset();
				}
			}

			lock (mCanUseNetPackageQueue)
			{
				while (mCanUseNetPackageQueue.Count > 0)
				{
					Package mPackage = mCanUseNetPackageQueue.Dequeue();
					mPackage.reset();
				}
			}
		}
	}

	//begin~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~网络接受系统~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public class NetReceiveSystem
	{
		protected List<byte> mReceiveStreamList;
		protected Queue<Package> mNeedHandlePackageQueue;
		protected Queue<Package> mCanUsePackageQueue;
		protected Dictionary<int, Action<Package>> mReceiveDic;

		public NetReceiveSystem(SocketSystem socketSys)
		{
			mReceiveDic = new Dictionary<int, Action<Package>>();
			mNeedHandlePackageQueue = new Queue<Package>();
			mCanUsePackageQueue = new Queue<Package>();
			mReceiveStreamList = new List<byte> ();
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
						
					mCanUsePackageQueue.Enqueue (mPackage);
				}
			}
		}

		public void ReceiveSocketStream(byte[] data)
		{
			lock (mReceiveStreamList) {
				mReceiveStreamList.AddRange (data);
			}
		}

		protected void HandleSocketStream ()
		{
			int PackageCout = 0;
			lock (mReceiveStreamList) {
				while (mReceiveStreamList.Count > 0) {
					byte[] mPackageByteArray = GetPackage ();
					if (mPackageByteArray != null) {
						Package mPackage = null;
						if (mCanUsePackageQueue.Count == 0) {
							mPackage = new xk_Protobuf ();
						} else {
							mPackage = mCanUsePackageQueue.Dequeue ();
						}

						mPackage.DeSerializeStream (mPackageByteArray);
						mNeedHandlePackageQueue.Enqueue (mPackage);
	
						PackageCout++;
					} else {
						break;
					}
				}
			}

			if (PackageCout > 3) {
				DebugSystem.LogError ("客户端 解析包的数量： " + PackageCout);
			}
		}

		private byte[] GetPackage()
		{
			byte[] msg = mReceiveStreamList.ToArray ();
			var data = NetEncryptionStream.DeEncryption(msg);
			if(data == null)
			{
				return null;
			}

			int Length = data.Length + 8;
			mReceiveStreamList.RemoveRange(0, Length);
			return data;
		}

		public virtual void Destory()
		{
			lock(mNeedHandlePackageQueue)
			{
				while (mNeedHandlePackageQueue.Count > 0)
				{
					Package mPackage = mNeedHandlePackageQueue.Dequeue();
					mPackage.reset();
				}
			}

			lock (mCanUsePackageQueue)
			{
				while (mCanUsePackageQueue.Count > 0)
				{
					Package mPackage = mCanUsePackageQueue.Dequeue();
					mPackage.reset();
				}
			}
		}
	}

	//begin~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~网络包体结构系统~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public abstract class Package
	{
		public int command = -1;

		public object data = null;
		protected byte[] stream = null;

		public abstract byte[] SerializePackage(int command,object data);

		public abstract void DeSerializeStream(byte[] msg);

		public abstract T getData<T>() where T : new();

		public virtual void reset()
		{
			data = null;
			stream = null;
			command = -1;
		}
	}
}


