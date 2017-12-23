using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.Debug;
using xk_System.DataStructure;
using System;

namespace xk_System.Net.Server
{
	public class  NetSendSystem_Select
	{
		protected QueueArraySegment<byte> mWaitSendBuffer = null;
		protected NetPackage mNetPackage = null;
		protected SocketSystem_Select mSocketSystem;
		private int clientId;

		public NetSendSystem_Select (SocketSystem_Select socketSys)
		{
			this.mSocketSystem = socketSys;
			mWaitSendBuffer = new QueueArraySegment<byte> (64, ClientConfig.nMaxBufferSize);
			mNetPackage = new NetPackage ();
		}

		public void SendNetData (int clientId, int id, byte[] buffer)
		{
			mNetPackage.command = id;
			mNetPackage.buffer = buffer;
			ArraySegment<byte> stream = NetEncryptionStream.Encryption (mNetPackage);
			mSocketSystem.SendNetStream (clientId, stream.Array, stream.Offset, stream.Count);
		}

		public void HandleNetPackage ()
		{
			
		}

		public void release ()
		{
			mWaitSendBuffer.release ();
			mNetPackage.reset ();
		}
	}
}
