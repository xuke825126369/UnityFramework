using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.DataStructure;
using System;
using xk_System.Debug;
using xk_System.Event;

namespace xk_System.Net.Server
{
	public class NetSystem_SocketAsyncEventArgs:SocketAsyncEventArgs_Token
	{
		protected NetPackage mPackage = null;
		protected ListBuffer<byte> mDicWaitSendBuffer = null;

		protected CircularBuffer<byte> mParseStreamList = null;
		protected NetPackage mReceieNetPackage = null;

		public NetSystem_SocketAsyncEventArgs ()
		{
			mPackage = new NetPackage ();
			mDicWaitSendBuffer = new ListBuffer<byte>();
			mReceieNetPackage = new NetPackage ();
			mParseStreamList = new CircularBuffer<byte> (ServerConfig.nMaxBufferSize * 2);
		}

		public void SendNetData (int command, byte[] buffer)
		{
			mPackage.command = command;
			mPackage.buffer = buffer;

			ArraySegment<byte> stream = NetEncryptionStream.Encryption (mPackage);
			this.SendNetStream (stream.Array, stream.Offset, stream.Count);
		}

		public void ReceiveSocketStream (byte[] data, int index, int Length)
		{
			mParseStreamList.WriteFrom (data, index, Length);
			HandleNetPackage ();
		}

		private void HandleNetPackage ()
		{
			int PackageCout = 0;
			while (GetPackage ()) {
				PackageCout++;
			}

			if (PackageCout == 0) {
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

			bool bSucccess = NetEncryptionStream.DeEncryption (mParseStreamList, mReceieNetPackage);

			if (bSucccess) {

			}

			return bSucccess;
		}
	}
}