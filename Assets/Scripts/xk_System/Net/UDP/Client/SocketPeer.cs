using System.Collections;
using System.Collections.Generic;
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
		protected Dictionary<UInt16, Action<NetReceivePackage>> mLogicFuncDic = null;


		protected ObjectPool<NetReceivePackage> mReceivePackagePool = null;
		protected Queue<NetReceivePackage> mReceivePackageQueue = null;

		protected UdpCheckPool mUdpCheckPool = null;
		private UInt16 nPackageOrderId;

		public SocketPeer ()
		{
			m_state = NetState.disconnected;
			mParseStreamList = new CircularBuffer<byte> (2 * ClientConfig.nMaxBufferSize);
			mLogicFuncDic = new Dictionary<UInt16, Action<NetReceivePackage>> ();

			mReceivePackagePool = new ObjectPool<NetReceivePackage> ();
			mReceivePackageQueue = new Queue<NetReceivePackage> ();
			mUdpCheckPool = new UdpCheckPool (this);

			nPackageOrderId = 1;
		}

		public void SendNetData (UInt16 id, byte[] buffer)
		{
			int readBytes = 0;
			int nBeginIndex = 0;
			UInt16 groupCount = (UInt16)(buffer.Length / ClientConfig.nMaxBufferSize + 1);
			while (nBeginIndex < buffer.Length) {
				if (nBeginIndex + ClientConfig.nMaxBufferSize - 12 > buffer.Length) {
					readBytes = buffer.Length - nBeginIndex;
				} else {
					readBytes = ClientConfig.nMaxBufferSize - 12;
				}

				UInt16 nPackageId = id;
				UInt16 nOrderId = this.nPackageOrderId;
				UInt32 uniqueId = NetPackageUtility.getUniqueId (nPackageId, nOrderId, groupCount);
				groupCount = 1;

				ArraySegment<byte> stream = NetEncryptionStream.EncryptionGroup (uniqueId, buffer, nBeginIndex, readBytes);
				SendNetStream (stream.Array, stream.Offset, stream.Count);
				mUdpCheckPool.AddSendCheck (nOrderId, stream);

				nBeginIndex += readBytes;
				this.nPackageOrderId++;
			}
		}

		public void SendNetData (UInt16 id, object data)
		{
			IMessage data1 = data as IMessage;
			byte[] stream = Protocol3Utility.SerializePackage (data1);
			SendNetData (id, stream);
		}
			
		public void AddLogicHandleQueue (NetReceivePackage mPackage)
		{
			if (mLogicFuncDic.ContainsKey (mPackage.nPackageId)) {
				mReceivePackageQueue.Enqueue (mPackage);
			} else {
				DebugSystem.LogError ("不存在的 协议ID: " + mPackage.nPackageId);
			}
		}

		public void addNetListenFun (UInt16 command, Action<NetReceivePackage> func)
		{
			if (!mLogicFuncDic.ContainsKey (command)) {
				mLogicFuncDic [command] = func;
			} else {
				mLogicFuncDic [command] += func;
			}
		}

		public void removeNetListenFun (UInt16 command, Action<NetReceivePackage> func)
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

		public virtual void Update ()
		{
			HandleReceivePackage ();
			while (mReceivePackageQueue.Count > 0) {
				NetReceivePackage mNetReceivePackage = mReceivePackageQueue.Dequeue ();
				mLogicFuncDic [mNetReceivePackage.nPackageId] (mNetReceivePackage);
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

			NetReceivePackage mNetReceivePackage = mReceivePackagePool.Pop ();
			bool bSucccess = false;
			lock (mParseStreamList) {
				bSucccess = NetEncryptionStream.DeEncryption (mParseStreamList, mNetReceivePackage);
			}

			if (bSucccess) {
				mNetReceivePackage.nPackageId = NetPackageUtility.getPackageId (mNetReceivePackage.nUniqueId);
				mNetReceivePackage.nOrderId = NetPackageUtility.getOrderId (mNetReceivePackage.nUniqueId);
				mNetReceivePackage.nGroupCount = NetPackageUtility.getGroupCount (mNetReceivePackage.nUniqueId);

				mUdpCheckPool.AddReceiveCheck (mNetReceivePackage);
			}

			return bSucccess;
		}

		private void HandleFindedServer (NetReceivePackage mPackage)
		{
			if (m_state != NetState.disconnected) {
				return;
			}

			if (mPackage.nPackageId != 1) {
				return;
			}

			connectServer ();
		}

		private void HandleConnect (NetReceivePackage mPackage)
		{
			if (m_state == NetState.connected) {
				return;
			}
			
			if (mPackage.nPackageId != 2) {
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