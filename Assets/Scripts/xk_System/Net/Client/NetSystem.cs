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

		public void sendNetData(int command, byte[] package)
		{
			NetPackage mPackage = NetObjectPool.Instance.mNetPackagePool.Pop ();
			mPackage.command = command;
			mPackage.buffer = package;
			mNetSendSystem.SendNetData(command, package);  
		}

		public void handleNetData()
		{
			mNetSendSystem.HandleNetPackage ();
			mNetReceiveSystem.HandleNetPackage ();
		}

		public void addNetListenFun(Action<NetPackage> fun)
		{
			mNetReceiveSystem.addListenFun(fun);
		}

		public void removeNetListenFun(Action<NetPackage> fun)
		{
			mNetReceiveSystem.removeListenFun(fun);
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
		protected SocketSystem mSocketSystem;

		public NetSendSystem(SocketSystem socketSys)
		{
			this.mSocketSystem = socketSys;
			mNeedHandleNetPackageQueue = new Queue<NetPackage> ();
		}

		public virtual void SendNetData(NetPackage data)
		{
			mNeedHandleNetPackageQueue.Enqueue (data);
		}

		public void HandleNetPackage ()
		{
			int handlePackageCount = 0;
			while (mNeedHandleNetPackageQueue.Count > 0) 
			{
				var mPackage = mNeedHandleNetPackageQueue.Dequeue ();
				HandleNetStream (mPackage);
				handlePackageCount++;
			}

			if (handlePackageCount > 3) {
				DebugSystem.LogError ("客户端 发送包的数量： " + handlePackageCount);
			}
		}

		public void HandleNetStream(NetPackage mPackage)
		{
			byte[] stream = NetStream.GetOutStream (mPackage.command,mPackage.buffer);
			stream = NetEncryptionStream.Encryption (stream);
			mSocketSystem.SendNetStream (stream);
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
		protected Action<NetPackage> mReceiveFun;

		public NetReceiveSystem(SocketSystem socketSys)
		{
			mNeedHandlePackageQueue = new Queue<NetPackage>();
			mReceiveStreamList = new List<byte> ();
			socketSys.initSystem (this);
		}

		public void addListenFun(int protocolType, Action<NetPackage> fun)
		{
			lock (mReceiveFun)
			{
				if (mReceiveFun != null)
				{
					if (CheckDataBindFunIsExist(protocolType, fun))
					{
						DebugSystem.LogError("添加监听方法重复");
						return;
					}
					mReceiveFun[protocolType] += fun;
				}
				else
				{
					mReceiveFun[protocolType] = fun;
				}              
			}
		}

		private bool CheckDataBindFunIsExist(int command,Action<byte[]> fun)
		{
			Action<NetPackage> mFunList = mReceiveFun[command];
			return DelegateUtility.CheckFunIsExist<byte[]>(mFunList, fun);
		}

		public void removeListenFun(Action<NetPackage> fun)
		{
			lock (mReceiveFun)
			{
				mReceiveFun-=fun;
			}
		}

		public void HandleNetPackage()
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
					NetPackage mPackage = GetPackage ();
					if (mPackage != null) {
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

		private NetPackage GetPackage()
		{
			byte[] msg = mReceiveStreamList.ToArray ();
			byte[] data = NetEncryptionStream.DeEncryption (msg);
			if (data == null) {
				return null;
			}

			int Length = data.Length + 8;
			mReceiveStreamList.RemoveRange (0, Length);

			NetPackage mPackage = NetObjectPool.Instance.mNetPackagePool.Pop ();
			NetStream.GetInputStream (data, out mPackage.command, out mPackage.buffer);
			return mPackage;
		}

		public virtual void Destory()
		{
			lock(mNeedHandlePackageQueue)
			{
				mNeedHandlePackageQueue.Clear ();
			}
		}
	}
}


