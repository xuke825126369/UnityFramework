using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.Debug;
using xk_System.DataStructure;
using System;

namespace xk_System.Net.Server
{
	public class  NetSendSystem
	{
		protected SocketSystem mSocketSystem;
		protected NetPackage mPackage = null;

		public NetSendSystem (SocketSystem socketSys)
		{
			this.mSocketSystem = socketSys;
			mPackage = new NetPackage ();
		}

		public virtual void SendNetData (int clientId, int command, byte[] buffer)
		{
			mPackage.clientId = clientId;
			mPackage.command = command;
			mPackage.buffer = buffer;

			ArraySegment<byte> stream = NetEncryptionStream.Encryption (mPackage);
			mSocketSystem.SendNetStream (mPackage.clientId, stream);
		}

		public void HandleNetPackage ()
		{

		}

		public virtual void Destory ()
		{

		}
	}

	public class  NetSendSystem1
	{
		protected QueueArraySegment<byte> mWaitSendBuffer = null;
		protected NetPackage mNetPackage = null;
		protected SocketSystem mSocketSystem;

		public NetSendSystem1 (SocketSystem socketSys)
		{
			this.mSocketSystem = socketSys;
			mWaitSendBuffer = new QueueArraySegment<byte> (64, ClientConfig.sendBufferSize);
			mNetPackage = new NetPackage ();
		}

		public void SendNetData (int id, byte[] buffer)
		{
			mNetPackage.command = id;
			mNetPackage.buffer = buffer;
			ArraySegment<byte> stream = NetEncryptionStream.Encryption (mNetPackage);
			mWaitSendBuffer.WriteFrom (stream.Array, stream.Offset, stream.Count);
		}

		public void HandleNetPackage ()
		{
			if (mWaitSendBuffer.Length > 0) {
				byte[] tempBuffer = mWaitSendBuffer.ToArray ();
				if (tempBuffer != null) {
					//mSocketSystem.SendNetStream (tempBuffer, 0, tempBuffer.Length);
				}

				mWaitSendBuffer.reset ();
			
				if (tempBuffer.Length > ClientConfig.sendBufferSize) {
					//DebugSystem.LogError ("客户端 发送字节数： " + tempBuffer.Length);
				}
			}
		}

		public void release ()
		{
			mWaitSendBuffer.release ();
			mNetPackage.reset ();
		}
	}
}
