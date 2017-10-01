using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.Debug;
using xk_System.Net.Client;
using System.Net.Sockets;
using xk_System.Net.Client.TCP;
using xk_System.Net;
using System;

namespace xk_System.Net
{
	public class NetSystem : Singleton<NetSystem>
	{
		protected NetSendSystem mNetSendSystem;
		protected NetReceiveSystem mNetReceiveSystem;

		public NetSystem()
		{
			mNetSendSystem = new NetSendSystem_Protobuf();
			mNetReceiveSystem = new NetReceiveSystem_Protobuf();
		}

		public void init(string ServerAddr, int ServerPort)
		{
			SocketSystem.Instance.init (ServerAddr, ServerPort);
		}

		public void SendInfo(byte[] msg)
		{
			SocketSystem.Instance.SendInfo (msg);
		}

		public void SendData(int command, object package)
		{
			mNetSendSystem.Send(this, command, package);  
		}

		public void ReceiveData()
		{
			mNetReceiveSystem.HandleData ();
		}

		public void addListenFun(int command, Action<Package> fun)
		{
			mNetReceiveSystem.addListenFun(command,fun);
		}

		public void removeListenFun(int command, Action<Package> fun)
		{
			mNetReceiveSystem.removeListenFun(command, fun);
		}

		public void  CloseNet()
		{
			SocketSystem.Instance.CloseNet ();
		}
	}

	public abstract class SocketSystem : Singleton<SocketSystem_Thread>
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

		protected NetSendSystem mNetSendSystem;
		protected NetReceiveSystem mNetReceiveSystem;

		protected Socket mSocket;
		protected Queue<string> mNetErrorQueue;

		public abstract void init(string ServerAddr, int ServerPort);
		public abstract void SendInfo(byte[] msg);

		public virtual void CloseNet()
		{
			//Instance = null;
			// mSocket.Shutdown(SocketShutdown.Receive);
			mSocket.Close();
			mSocket = null;
			mNetSendSystem.Destory();
			mNetReceiveSystem.Destory();
			DebugSystem.Log("关闭客户端TCP连接");
		}

	}

	//begin~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~网络发送系统~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public abstract class  NetSendSystem
	{
		private static NetSendSystem single;
		static NetSendSystem_Protobuf single_Protobuf = new NetSendSystem_Protobuf();
		protected PackageSendPool mSendPool = new PackageSendPool();
		protected Package mPackage=new xk_Protobuf();  
		protected NetSendSystem()
		{

		}

		public static T getSingle<T>() where T:NetSendSystem,new()
		{
			if(single==null)
			{
				single = new T();
			}
			return (T)single;
		}

		public static NetSendSystem_Protobuf getSingle()
		{           
			return single_Protobuf;
		}

		public abstract void Send(NetSystem mNetSystem,int command,object data);

		public virtual void Destory()
		{
			mPackage.Destory();
		}
	}

	//begin~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~网络接受系统~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public abstract class NetReceiveSystem
	{
		protected Dictionary<int, Action<Package>> mReceiveDic;
		protected Queue<Package> mNeedHandlePackageQueue;
		protected Queue<Package> mCanUsePackageQueue;
		private static NetReceiveSystem single;
		private static NetReceiveSystem_Protobuf single_Protobuf=new NetReceiveSystem_Protobuf();

		protected NetReceiveSystem()
		{
			mReceiveDic = new Dictionary<int, Action<Package>>();
			mNeedHandlePackageQueue = new Queue<Package>();
			mCanUsePackageQueue = new Queue<Package>();
		}

		public static T getSingle<T>() where T : NetReceiveSystem, new()
		{
			if (single == null)
			{
				single = new T();
			}
			return (T)single;
		}

		public static NetReceiveSystem_Protobuf getSingle()
		{
			return single_Protobuf;
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
		public abstract void Receive(byte[] msg);

		public void HandleData()
		{
			if (mNeedHandlePackageQueue != null)
			{
				lock (mNeedHandlePackageQueue)
				{
					while (mNeedHandlePackageQueue.Count > 0)
					{
						Package mPackage = mNeedHandlePackageQueue.Dequeue();
						if (mReceiveDic.ContainsKey(mPackage.command))
						{
							mReceiveDic[mPackage.command](mPackage);
						}
						else
						{
							DebugSystem.LogError("没有找到相关命令的处理函数：" + mPackage.command);
						}
						lock(mCanUsePackageQueue)
						{
							mCanUsePackageQueue.Enqueue(mPackage);
						}
					}
				}
			}
		}

		public virtual void Destory()
		{
			lock(mNeedHandlePackageQueue)
			{
				while (mNeedHandlePackageQueue.Count > 0)
				{
					Package mPackage = mNeedHandlePackageQueue.Dequeue();
					mPackage.Destory();
				}
			}

			lock (mCanUsePackageQueue)
			{
				while (mCanUsePackageQueue.Count > 0)
				{
					Package mPackage = mCanUsePackageQueue.Dequeue();
					mPackage.Destory();
				}
			}
		}
	}

	//begin~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~网络包体结构系统~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public abstract class Package
	{
		public int command;
		protected byte[] data_byte=null;

		public abstract byte[] SerializePackage(int command,object data);

		public abstract void DeSerializeStream(byte[] msg);

		public abstract T getData<T>() where T : new();

		public virtual void Destory()
		{

		}
	}
}


