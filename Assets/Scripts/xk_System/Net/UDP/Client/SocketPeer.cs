using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.DataStructure;
using System;
using xk_System.Event;
using xk_System.Debug;

namespace xk_System.Net.UDP.Client
{
	public class SocketPeer
	{
		protected QueueArraySegment<byte> mWaitSendBuffer = null;
		protected NetPackage mNetPackage = null;
		protected SocketSystem_Udp mSocketSystem;

		protected CircularBuffer<byte> mReceiveStreamList= null;
		protected CircularBuffer<byte> mParseStreamList = null;
		protected DataBind<NetPackage> mBindReceiveNetPackage = null;

		public SocketPeer (SocketSystem_Udp socketSys)
		{
			this.mSocketSystem = socketSys;
			mWaitSendBuffer = new QueueArraySegment<byte> (64, ClientConfig.nMaxBufferSize);
			mNetPackage = new NetPackage ();

			mReceiveStreamList = new CircularBuffer<byte> (ClientConfig.nMaxBufferSize);
			mParseStreamList = new CircularBuffer<byte> (2 * ClientConfig.nMaxBufferSize);
			mBindReceiveNetPackage = new DataBind<NetPackage> (new NetPackage ());
		}

		public void SendNetData (int id, byte[] buffer)
		{
			mNetPackage.command = id;
			mNetPackage.buffer = buffer;
			ArraySegment<byte> stream = NetEncryptionStream.Encryption (mNetPackage);
			mWaitSendBuffer.WriteFrom (stream.Array, stream.Offset, stream.Count);
		}

		private void HandleSendPackage ()
		{
			if (mWaitSendBuffer.Length > 0) {
				byte[] tempBuffer = mWaitSendBuffer.ToArray ();
				if (tempBuffer != null) {
					mSocketSystem.SendNetStream (tempBuffer, 0, tempBuffer.Length);
				}

				mWaitSendBuffer.reset ();

				if (tempBuffer.Length > ClientConfig.nMaxBufferSize) {
					//DebugSystem.LogError ("客户端 发送字节数： " + tempBuffer.Length);
				}
			}
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
			return mReceiveStreamList.isCanWriteFrom (ClientConfig.nMaxBufferSize);
		}

		public void ReceiveSocketStream (byte[] data, int index, int Length)
		{
			lock (mParseStreamList) {
				mReceiveStreamList.WriteFrom (data, index, Length);
			}
		}

		public void HandleNetPackage()
		{
			HandleSendPackage ();
			HandleReceievPackage ();
		}

		private void HandleReceievPackage ()
		{
			int PackageCout = 0;

			lock (mReceiveStreamList) {
				int readBytes = mParseStreamList.WriteFrom (mReceiveStreamList, ClientConfig.nMaxBufferSize);
			}

			while (GetPackage ()) {
				PackageCout++;
			}

			if (PackageCout == 0) {
				if (mParseStreamList.Length > 0) {
					DebugSystem.LogError ("客户端 正在解包: " + mParseStreamList.Length + " | " + mParseStreamList.Capacity);
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
			mWaitSendBuffer.release ();
			mNetPackage.reset ();
		}
	}
}