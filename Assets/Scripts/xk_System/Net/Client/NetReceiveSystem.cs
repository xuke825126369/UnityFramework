using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.DataStructure;
using xk_System.Event;
using xk_System.Debug;
using System;

namespace xk_System.Net.Client
{
	//不和线程打交道的
	public class NetNoLockReceiveSystem :NetReceiveSystemInterface
	{
		protected CircularBuffer<byte> mParseStreamList = null;
		protected DataBind<NetPackage> mBindReceiveNetPackage = null;

		public NetNoLockReceiveSystem (SocketSystem socketSys)
		{
			mParseStreamList = new CircularBuffer<byte> (2 * ClientConfig.receiveBufferSize);
			mBindReceiveNetPackage = new DataBind<NetPackage> (new NetPackage ());
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

		public bool isCanReceiveFromSocketStream()
		{
			return true;
		}

		public void ReceiveSocketStream (byte[] data, int index, int Length)
		{
			mParseStreamList.WriteFrom (data, index, Length);
		}

		public void HandleNetPackage ()
		{
			int PackageCout = 0;
			while (GetPackage ()) {
				PackageCout++;
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

		public void release ()
		{

		}
	}
		
	//和线程打交道
	public class NetLockReceiveSystem:NetReceiveSystemInterface
	{
		protected CircularBuffer<byte> mReceiveStreamList= null;
		protected CircularBuffer<byte> mParseStreamList = null;
		protected DataBind<NetPackage> mBindReceiveNetPackage = null;

		public NetLockReceiveSystem (SocketSystem socketSys)
		{
			mReceiveStreamList = new CircularBuffer<byte> (ClientConfig.nThreadSaveMaxBuffer);
			mParseStreamList = new CircularBuffer<byte> (2 * ClientConfig.receiveBufferSize);
			mBindReceiveNetPackage = new DataBind<NetPackage> (new NetPackage ());
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

		public bool isCanReceiveFromSocketStream()
		{
			return mReceiveStreamList.isCanWriteFrom (ServerConfig.receiveBufferSize);
		}

		public void ReceiveSocketStream (byte[] data, int index, int Length)
		{
			lock (mParseStreamList) {
				mReceiveStreamList.WriteFrom (data, index, Length);
			}
		}

		public void HandleNetPackage ()
		{
			int PackageCout = 0;

			lock (mReceiveStreamList) {
				int readBytes = mParseStreamList.WriteFrom (mReceiveStreamList, ClientConfig.receiveBufferSize);
			}
				
			while (GetPackage ()) {
				PackageCout++;
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

		public virtual void release ()
		{

		}
	}
}