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
using xk_System.Event;

namespace xk_System.Net.Client
{
	public class NetSystem:NetEventInterface
	{
		private NetSendSystem mNetSendSystem;
		private NetReceiveSystem mNetReceiveSystem;
		private SocketSystem mNetSocketSystem;

		public NetSystem ()
		{
			mNetSocketSystem = new SocketSystem_Select ();
			mNetSendSystem = new NetSendSystem (mNetSocketSystem);
			mNetReceiveSystem = new NetReceiveSystem (mNetSocketSystem);
		}

		public void initNet (string ServerAddr, int ServerPort)
		{
			mNetSocketSystem.init (ServerAddr, ServerPort);
		}

		public void sendNetData (int command, byte[] buffer)
		{
			mNetSendSystem.SendNetData (command, buffer);  
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
		//protected const int receiveInfoPoolCapacity = 8192;
		//protected const int sendInfoPoolCapacity = 8192;
		//protected const int receiveTimeOut = 10000;
		//protected const int sendTimeOut = 5000;

		protected const int receiveInfoPoolCapacity = 10;
		protected const int sendInfoPoolCapacity = 10;
		protected const int receiveTimeOut = 5000;
		protected const int sendTimeOut = 5000;

		public abstract void init (string ServerAddr, int ServerPort);

		public abstract void SendNetStream (byte[] msg, int offset, int Length);

		public abstract void Update ();

		public abstract void CloseNet ();

		protected NetSendSystem mNetSendSystem;
		protected NetReceiveSystem mNetReceiveSystem;

		public void initReceieSystem (NetReceiveSystem mNetReceiveSystem)
		{
			this.mNetReceiveSystem = mNetReceiveSystem;
		}

		public void initSendSystem (NetSendSystem mNetSendSystem)
		{
			this.mNetSendSystem = mNetSendSystem;
		}

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

	//begin~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~网络发送系统~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public class  NetSendSystem
	{
		protected ObjectPool<NetPackage> mCanUseNetPackageQueue;
		protected Queue<NetPackage> mNeedHandleNetPackageQueue;
		protected SocketSystem mSocketSystem;

		public NetSendSystem (SocketSystem socketSys)
		{
			this.mSocketSystem = socketSys;
			this.mSocketSystem.initSendSystem (this);
			mNeedHandleNetPackageQueue = new Queue<NetPackage> ();
			mCanUseNetPackageQueue = new ObjectPool<NetPackage> ();
		}

		public virtual void SendNetData (int id, byte[] buffer)
		{
			NetPackage mNetPackage = mCanUseNetPackageQueue.Pop ();
			mNetPackage.command = id;
			mNetPackage.buffer = buffer;
			mNetPackage.Length = buffer.Length;
			mNeedHandleNetPackageQueue.Enqueue (mNetPackage);
		}

		public void HandleNetPackage ()
		{
			int handlePackageCount = 0;
			while (mNeedHandleNetPackageQueue.Count > 0) {
				var mPackage = mNeedHandleNetPackageQueue.Dequeue ();
				HandleNetStream (mPackage);
				mCanUseNetPackageQueue.recycle (mPackage);
				handlePackageCount++;
			}

			if (handlePackageCount > 5) {
				//DebugSystem.Log ("客户端 发送包的数量： " + handlePackageCount);
			}
		}

		private void HandleNetStream (NetPackage mPackage)
		{
			byte[] stream = NetEncryptionStream.Encryption (mPackage);
			mSocketSystem.SendNetStream (stream, 0, stream.Length);
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
		protected CircularBuffer<byte> mParseStreamList = null;
		protected DataBind<NetPackage> mBindReceiveNetPackage = null;

		public NetReceiveSystem (SocketSystem socketSys)
		{
			mParseStreamList = new CircularBuffer<byte> (ClientConfig.nMaxPackageSize * ClientConfig.nPerFrameHandlePackageCount);
			mBindReceiveNetPackage = new DataBind<NetPackage> (new NetPackage ());
			socketSys.initReceieSystem (this);
		}

		//Add More Protocol Interface
		public void addListenFun (Action<NetPackage> fun)
		{
			mBindReceiveNetPackage.addDataBind (fun);
		}

		public void removeListenFun (Action<NetPackage> fun)
		{
			mBindReceiveNetPackage.removeDataBind (fun);
		}

		public void ReceiveSocketStream (byte[] data, int index, int Length)
		{
			lock (mParseStreamList) {
				mParseStreamList.WriteFrom (data, index, Length);
			}
		}

		public void HandleNetPackage ()
		{
			int PackageCout = 0;
			lock (mParseStreamList) {
				while (GetPackage ()) {
					PackageCout++;

					if (PackageCout > 100) {
						break;
					}
				}
			}

			if (PackageCout > 5) {
				DebugSystem.Log ("客户端 解析包的数量： " + PackageCout);
			} else if (PackageCout == 0) {
				if (mParseStreamList.Length > 0) {
					DebugSystem.LogError ("客户端 正在解包 ");
				}
			}
		}

		private bool GetPackage ()
		{
			if (mParseStreamList.Length <= 0) {
				return false;
			}

			bool bSucccess = NetEncryptionStream.DeEncryption (mParseStreamList, mBindReceiveNetPackage.bindData);
			if (bSucccess) {
				mBindReceiveNetPackage.DispatchEvent ();
			}
		
			return bSucccess;
		}

		public virtual void Destory ()
		{
			
		}
	}
}


