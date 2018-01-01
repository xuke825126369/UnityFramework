using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.DataStructure;
using System;
using xk_System.Debug;
using xk_System.Event;

namespace xk_System.Net.UDP.BROADCAST.Server
{
	public class SocketReceivePeer
	{
		protected QueueArraySegment<byte> mWaitSendBuffer = null;
		protected NetPackage mNetPackage = null;
		protected CircularBuffer<byte> mParseStreamList = null;

		protected Dictionary<UInt16, Action<NetPackage>> mLogicFuncDic = null;

		public SocketReceivePeer ()
		{
			mWaitSendBuffer = new QueueArraySegment<byte> (64, ServerConfig.nMaxBufferSize);
			mNetPackage = new NetPackage ();

			mParseStreamList = new CircularBuffer<byte> (2 * ServerConfig.nMaxBufferSize);

			mLogicFuncDic = new Dictionary<ushort, Action<NetPackage>> ();

		}

		public void addNetListenFun (UInt16 command, Action<NetPackage> func)
		{
			if (!mLogicFuncDic.ContainsKey (command)) {
				mLogicFuncDic [command] = func;
			} else {
				mLogicFuncDic [command] += func;
			}
		}

		public void removeNetListenFun (UInt16 command, Action<NetPackage> func)
		{
			if (mLogicFuncDic.ContainsKey (command)) {
				mLogicFuncDic [command] -= func;
			}
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

			NetPackage mNetPackage = new NetPackage ();

			bool bSucccess = false;
			lock (mParseStreamList) {
				bSucccess = NetEncryptionStream.DeEncryption (mParseStreamList, mNetPackage);
			}

			if (bSucccess) {
				mLogicFuncDic [(UInt16)mNetPackage.command] (mNetPackage);
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