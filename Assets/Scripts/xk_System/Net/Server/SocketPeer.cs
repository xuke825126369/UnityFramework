using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.DataStructure;
using System;
using xk_System.Debug;
using xk_System.Event;
using xk_System.Net.Server.Event;

namespace xk_System.Net.Server
{
	public class SocketPeer_SocketAsyncEventArgs:SocketAsyncEventArgs_Token
	{
		private NetPackage mPackage = null;
		private ListBuffer<byte> mDicWaitSendBuffer = null;

		private CircularBuffer<byte> mParseStreamList = null;
		private DataBind<NetPackage> mBindReceieNetPackage = null;

		public SocketPeer_SocketAsyncEventArgs ()
		{
			mPackage = new NetPackage ();
			mDicWaitSendBuffer = new ListBuffer<byte>();
			mBindReceieNetPackage = new DataBind<NetPackage> (new NetPackage());
			mBindReceieNetPackage.addDataBind (NetSystem.mEventSystem.DeSerialize);
			mParseStreamList = new CircularBuffer<byte> (ServerConfig.nMaxBufferSize * 2);
		}

		public void SendNetData(int command, byte[] buffer)
		{
			mPackage.command = command;
			mPackage.buffer = buffer;

			ArraySegment<byte> stream = NetEncryptionStream.Encryption (mPackage);
			this.SendNetStream (stream.Array, stream.Offset, stream.Count);
		}

		public void ReceiveSocketStream (byte[] data, int index, int Length)
		{
			lock(mParseStreamList)
			{
				mParseStreamList.WriteFrom (data, index, Length);
			}
		}

		public void HandleNetPackage ()
		{
			int PackageCout = 0;
			while (GetPackage ()) {
				PackageCout++;
			}

			if (PackageCout == 0) {
				if (mParseStreamList.Length > 0) {
					DebugSystem.LogError ("服务端ClientPeer 正在解包 ");
				}
			}
		}

		private bool GetPackage ()
		{
			if (mParseStreamList.Length <= 0) {
				return false;
			}

			bool bSucccess = NetEncryptionStream.DeEncryption (mParseStreamList, mBindReceieNetPackage.bindData);
			if (bSucccess) {
				mBindReceieNetPackage.bindData.clientId = this.getId ();
				mBindReceieNetPackage.DispatchEvent ();
			}

			return bSucccess;
		}
	}

	public class SocketPeer_Select: Select_Token
	{
		protected QueueArraySegment<byte> mWaitSendBuffer = null;
		protected NetPackage mNetPackage = null;
		protected CircularBuffer<byte> mParseStreamList = null;
		protected DataBind<NetPackage> mBindReceiveNetPackage = null;

		public SocketPeer_Select ()
		{
			mWaitSendBuffer = new QueueArraySegment<byte> (64, ClientConfig.nMaxBufferSize);
			mNetPackage = new NetPackage ();

			mParseStreamList = new CircularBuffer<byte> (2 * ClientConfig.nMaxBufferSize);
			mBindReceiveNetPackage = new DataBind<NetPackage> (new NetPackage ());

			mBindReceiveNetPackage.addDataBind (NetSystem_Select.mEventSystem.DeSerialize);
		}

		public void SendNetData (int id, byte[] buffer)
		{
			mNetPackage.command = id;
			mNetPackage.buffer = buffer;
			ArraySegment<byte> stream = NetEncryptionStream.Encryption (mNetPackage);
			this.SendNetStream (stream.Array, stream.Offset, stream.Count);
		}

		public void ReceiveSocketStream (byte[] data, int index, int Length)
		{
			mParseStreamList.WriteFrom (data, index, Length);
			int PackageCout = 0;
			while (GetPackage ()) {
				PackageCout++;
			}

			if (PackageCout == 0) {
				if (mParseStreamList.Length > 0) {
					DebugSystem.LogError ("服务端 ClientPeer 正在解包 ");
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
				mBindReceiveNetPackage.bindData.clientId = getId ();
				mBindReceiveNetPackage.DispatchEvent ();
			} else {
				DebugSystem.LogError ("服务器端 解码失败");
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