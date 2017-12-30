using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.DataStructure;
using System;
using xk_System.Event;
using xk_System.Debug;
using System.Net.Sockets;
using Google.Protobuf;
using xk_System.Net.UDP.Protocol;

namespace xk_System.Net.UDP.Client
{
	public abstract class SocketPeer: SocketUdp_Basic
	{
		protected CircularBuffer<byte> mParseStreamList = null;
		protected Dictionary<int, Action<NetPackage>> mLogicFuncDic = null;
		protected Queue<NetPackage> mNeedHandleQueue = null;
		protected UdpCheck mUdpCheckPool = null;
		private UInt32 nPackageOrderId;

		public SocketPeer()
		{
			m_state = NetState.disconnected;
			mParseStreamList = new CircularBuffer<byte> (2 * ClientConfig.nMaxBufferSize);
			mLogicFuncDic = new Dictionary<int, Action<NetPackage>> ();
			mNeedHandleQueue = new Queue<NetPackage> ();
			mUdpCheckPool = new UdpCheck ();
			nPackageOrderId = 1;
		}

		public void SendNetData (int id, byte[] buffer)
		{
			NetPackage mNetPackage = new NetPackage ();
			mNetPackage.command = id;
			mNetPackage.buffer = buffer;
			mNetPackage.orderId = nPackageOrderId;
			ArraySegment<byte> stream = NetEncryptionStream.Encryption (mNetPackage);
			SendNetStream (stream.Array, 0, stream.Count);
			nPackageOrderId++;
		}

		public void SendNetData(int id, object data)
		{
			IMessage data1 = data as IMessage;
			byte[] stream = Protocol3Utility.SerializePackage (data1);
			SendNetData (id, stream);
		}

		protected override void DeSerialize (NetPackage mPackage)
		{
			if (mLogicFuncDic.ContainsKey (mPackage.command)) {
				mNeedHandleQueue.Enqueue (mPackage);
			} else {
				DebugSystem.LogError ("不存在的 协议ID: " + mPackage.command);
			}
		}

		public void addNetListenFun(int command, Action<NetPackage> func)
		{
			if (!mLogicFuncDic.ContainsKey (command)) {
				mLogicFuncDic [command] = func;
			} else {
				mLogicFuncDic [command] += func;
			}
		}

		public void removeNetListenFun(int command,Action<NetPackage> func)
		{
			if (mLogicFuncDic.ContainsKey (command)) {
				mLogicFuncDic [command] -= func;
			}
		}

		public override void ReceiveSocketStream (byte[] data, int index, int Length)
		{
			lock (mParseStreamList) {
				mParseStreamList.WriteFrom (data, index, Length);
			}
		}

		public virtual void Update()
		{
			HandleReceivePackage ();
			while (mNeedHandleQueue.Count > 0) {
				NetPackage mNetPackage = mNeedHandleQueue.Dequeue ();
				mLogicFuncDic [mNetPackage.command] (mNetPackage);
			}
		}

		private void HandleReceivePackage ()
		{
			int PackageCout = 0;

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

			NetPackage mNetPackage = new NetPackage ();
			bool bSucccess = NetEncryptionStream.DeEncryption (mParseStreamList, mNetPackage);

			if (bSucccess) {
				this.DeSerialize ();
			}

			return bSucccess;
		}
			
		private void HandleFindedServer(NetPackage mPackage)
		{
			if (m_state != NetState.disconnected) {
				return;
			}

			if (mPackage.command != 1) {
				return;
			}

			ip = BitConverter.ToString (mPackage.buffer);
			connectServer ();
		}

		private void HandleConnect(NetPackage mPackage)
		{
			if (m_state == NetState.connected) {
				return;
			}
			
			if (mPackage.command != 2) {
				return;
			}

			System.Timers.Timer tm = new System.Timers.Timer ();
			tm.Interval = 3.0;
			tm.AutoReset = false;
			tm.Elapsed += (object sender, System.Timers.ElapsedEventArgs args) => {

			};
		}

		public void release ()
		{
			
		}
	}
}