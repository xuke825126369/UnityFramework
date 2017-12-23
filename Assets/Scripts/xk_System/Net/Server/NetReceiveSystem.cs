using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.DataStructure;
using xk_System.Event;
using xk_System.Debug;
using System;

namespace xk_System.Net.Server
{
	//不和线程打交道的
	public class NetReceiveSystem_Select
	{
		protected CircularBuffer<byte> mParseStreamList = null;
		protected DataBind<NetPackage> mBindReceiveNetPackage = null;

		public NetReceiveSystem_Select (SocketSystem_Select socketSys)
		{
			mParseStreamList = new CircularBuffer<byte> (2 * ClientConfig.nMaxBufferSize);
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

		public void ReceiveSocketStream (int clientId,byte[] data, int index, int Length)
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
}