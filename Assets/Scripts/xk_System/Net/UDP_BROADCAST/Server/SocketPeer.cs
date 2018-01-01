using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.DataStructure;
using System;
using xk_System.Debug;
using xk_System.Event;

namespace xk_System.Net.UDP.BROADCAST.Server
{
	public class SocketPeer:SocketToken
	{
		protected QueueArraySegment<byte> mWaitSendBuffer = null;
		protected NetPackage mNetPackage = null;
		protected CircularBuffer<byte> mParseStreamList = null;
		protected DataBind<NetPackage> mBindReceiveNetPackage = null;

		public SocketPeer ()
		{
			mWaitSendBuffer = new QueueArraySegment<byte> (64, ServerConfig.nMaxBufferSize);
			mNetPackage = new NetPackage ();

			mParseStreamList = new CircularBuffer<byte> (2 * ServerConfig.nMaxBufferSize);
			mBindReceiveNetPackage = new DataBind<NetPackage> (new NetPackage ());

			//mBindReceiveNetPackage.addDataBind (NetSystem.mEventSystem.DeSerialize);
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
			lock (mParseStreamList) {
				mParseStreamList.WriteFrom (data, index, Length);
			}
		}

		public void Update()
		{
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