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
	public class NetSystem
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

		public void handleNetData()
		{
			mNetSendSystem.HandleNetPackage ();
			mNetReceiveSystem.HandleNetPackage ();
		}

		public void addNetListenFun(int command, Action<NetPackage> fun)
		{
			mNetReceiveSystem.addListenFun(command,fun);
		}

		public void removeNetListenFun(int command, Action<NetPackage> fun)
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
		protected const int receiveInfoPoolCapacity = 8192;
		protected const int sendInfoPoolCapacity = 8192;
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
		protected Queue<NetPackage> mNeedHandleNetPackageQueue;
		protected ObjectPool<NetPackage> mCanUseNetPackageQueue;
		protected SocketSystem mSocketSystem;

		public NetSendSystem(SocketSystem socketSys)
		{
			this.mSocketSystem = socketSys;
			mNeedHandleNetPackageQueue = new Queue<NetPackage> ();
			mCanUseNetPackageQueue = new ObjectPool<NetPackage> ();
		}

		public virtual void SendNetData(int command,object data)
		{
			NetPackage mPackage = mCanUseNetPackageQueue.Pop ();
			mPackage.reset ();
			mPackage.setCommand (command);
			mPackage.setObjectData(data);

			mNeedHandleNetPackageQueue.Enqueue (mPackage);
		}

		public void HandleNetPackage ()
		{
			int handlePackageCount = 0;
			while (mNeedHandleNetPackageQueue.Count > 0) 
			{
				var mPackage = mNeedHandleNetPackageQueue.Dequeue ();
				HandleNetStream (mPackage);
				mCanUseNetPackageQueue.recycle (mPackage);
				handlePackageCount++;
			}

			if (handlePackageCount > 3) {
				DebugSystem.LogError ("客户端 发送包的数量： " + handlePackageCount);
			}
		}

		public void HandleNetStream(NetPackage mPackage)
		{
			byte[] stream= mPackage.SerializePackage();
			stream = NetEncryptionStream.Encryption (stream);
			mSocketSystem.SendNetStream (stream);
			mPackage.reset ();
		}
			
		public virtual void Destory()
		{
			lock(mNeedHandleNetPackageQueue)
			{
				mNeedHandleNetPackageQueue.Clear ();
			}

			lock (mCanUseNetPackageQueue)
			{
				mCanUseNetPackageQueue.release ();
			}
		}
	}

	//begin~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~网络接受系统~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public class NetReceiveSystem
	{
		protected List<byte> mReceiveStreamList;
		protected Queue<NetPackage> mNeedHandlePackageQueue;
		protected ObjectPool<NetPackage> mCanUsePackageQueue;
		protected Dictionary<int, Action<NetPackage>> mReceiveDic;

		public NetReceiveSystem(SocketSystem socketSys)
		{
			mReceiveDic = new Dictionary<int, Action<NetPackage>>();
			mNeedHandlePackageQueue = new Queue<NetPackage>();
			mCanUsePackageQueue = new ObjectPool<NetPackage> ();
			mReceiveStreamList = new List<byte> ();
			socketSys.initSystem (this);
		}

		public void addListenFun(int command, Action<NetPackage> fun)
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

		private bool CheckDataBindFunIsExist(int command,Action<NetPackage> fun)
		{
			Action<NetPackage> mFunList = mReceiveDic[command];
			return DelegateUtility.CheckFunIsExist<NetPackage>(mFunList, fun);
		}

		public void removeListenFun(int command, Action<NetPackage> fun)
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
					NetPackage mPackage = mNeedHandlePackageQueue.Dequeue ();
					if (mReceiveDic.ContainsKey (mPackage.getCommand())) {
						mReceiveDic [mPackage.getCommand()] (mPackage);
					} else {
						DebugSystem.LogError ("没有找到相关命令的处理函数：" + mPackage.getCommand());
					}
						
					mCanUsePackageQueue.recycle (mPackage);
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
						NetPackage mPackage = mCanUsePackageQueue.Pop();
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
				mNeedHandlePackageQueue.Clear ();
			}

			lock (mCanUsePackageQueue)
			{
				mCanUsePackageQueue.release ();
			}
		}
	}
}


