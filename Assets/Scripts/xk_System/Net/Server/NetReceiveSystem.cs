using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.DataStructure;
using xk_System.Event;
using xk_System.Debug;
using System;

namespace xk_System.Net.Server
{
	public class NetReceiveSystem_SocketAsyncEventArgs:NetReceiveSystemInterface
	{
		protected Dictionary<int, CircularBuffer<byte>> mReceivedStreamDic;
		protected Queue<ClientNetBuffer> mReceiveStreamQueue;
		protected ObjectPool<ClientNetBuffer> mCanUsePackageQueue;
		protected DataBind<NetPackage> mDataBindReceivePackage;

		protected Action<NetPackage> mReceiveFun;

		public NetReceiveSystem_SocketAsyncEventArgs (SocketSystem socketSys)
		{
			mReceivedStreamDic = new Dictionary<int, CircularBuffer<byte>> ();
			mReceiveStreamQueue = new Queue<ClientNetBuffer> ();
			mCanUsePackageQueue = new ObjectPool<ClientNetBuffer> ();
			mDataBindReceivePackage = new DataBind<NetPackage> (new NetPackage ());
		}

		public void addListenFun (Action<NetPackage> fun)
		{
			mDataBindReceivePackage.addDataBind (fun);   
		}

		public void removeListenFun (Action<NetPackage> fun)
		{
			mDataBindReceivePackage.removeDataBind (fun);
		}

		public bool isCanReceiveFromSocketStream ()
		{
			return true;
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
						int BufferSize = 2 * ServerConfig.nMaxBufferSize;
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

			var bSuccess = NetEncryptionStream.DeEncryption (msg, mDataBindReceivePackage.bindData);
			if (bSuccess == true) {
				mDataBindReceivePackage.bindData.clientId = clientId;
				mDataBindReceivePackage.DispatchEvent ();
			}

			return bSuccess;
		}

		public void release ()
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

	//不和线程打交道的
	public class NetReceiveSystem_Select:NetReceiveSystemInterface
	{
		protected CircularBuffer<byte> mParseStreamList = null;
		protected DataBind<NetPackage> mBindReceiveNetPackage = null;

		public NetReceiveSystem_Select (SocketSystem socketSys)
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