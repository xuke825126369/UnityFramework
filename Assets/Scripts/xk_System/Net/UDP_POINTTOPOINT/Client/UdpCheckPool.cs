using System.Collections;
using System.Collections.Generic;
using System;
using xk_System.Net.UDP.POINTTOPOINT.Protocol;
using xk_System.Debug;
using UdpPointtopointProtocols;

namespace xk_System.Net.UDP.POINTTOPOINT.Client
{
	public class UdpCheckPool
	{
		public struct CheckInfo
		{
			public const int SendCheckCount = 3;
			public const int ReceiveCheckCount = 2;
			public ArraySegment<byte> mArraySegment;
			public int nReceiveCheckResultCount;
		}

		private Dictionary<UInt16, CheckInfo> mWaitCheckSendDic = null;
		private Dictionary<UInt16, CheckInfo> mWaitCheckReceiveDic =  null;
		private ClientPeer mUdpPeer;
		private ObjectPool<System.Timers.Timer> mTimerPool = null;

		public UdpCheckPool(ClientPeer mUdpPeer)
		{
			mTimerPool = new ObjectPool<System.Timers.Timer> ();
			mWaitCheckSendDic = new Dictionary<ushort, CheckInfo> ();
			mWaitCheckReceiveDic = new Dictionary<ushort, CheckInfo> ();
			this.mUdpPeer = mUdpPeer;
			mUdpPeer.addNetListenFun (UdpNetCommand.COMMAND_PACKAGECHECK, ReceiveCheckPackage);
		}

		public void AddSendCheck(UInt16 nOrderId, ArraySegment<byte> sendBuff)
		{
			mWaitCheckSendDic [nOrderId] = sendBuff;

			System.Timers.Timer tm = mTimerPool.Pop ();
			tm.Interval = 2.0;
			tm.AutoReset = true;
			tm.Start ();

			if (tm.Elapsed == null) {
				tm.Elapsed += (object sender, System.Timers.ElapsedEventArgs args) => {
					if (mWaitCheckSendDic.ContainsKey (nOrderId)) {
						ArraySegment<byte> buff = mWaitCheckSendDic [nOrderId];
						this.mUdpPeer.SendNetStream (buff.Array, buff.Offset, buff.Count);
					} else {
						tm.Stop ();
						mTimerPool.recycle (tm);
					}
				};
			}
		}

		private void ReceiveCheckPackage(NetReceivePackage mPackage)
		{
			PackageCheckResult mPackageCheckResult = Protocol3Utility.getData<PackageCheckResult> (mPackage.buffer.Array, mPackage.buffer.Offset, mPackage.buffer.Count);
			UInt16 whoId = (UInt16)(mPackageCheckResult.NWhoOrderId >> 16);
			UInt16 nOrderId = (UInt16)(mPackageCheckResult.NWhoOrderId & 0x0000FFFF);

			if (whoId == 1) {
				this.mUdpPeer.SendNetData (mPackage.nPackageId, mPackageCheckResult);

				if (mWaitCheckSendDic.ContainsKey (nOrderId)) {
					mWaitCheckSendDic.Remove ((UInt16)mPackageCheckResult.NWhoOrderId);
				}
			} else if (whoId == 2) {
				

			}
		}

		private UInt16 nCurrentWaitReceiveOrderId = 1;
		private Dictionary<UInt16, NetReceivePackage> mReceivePackageQueue = new Dictionary<UInt16, NetReceivePackage> ();
		private List<CombinePackage> mReceiveGroupList = new List<CombinePackage> ();

		public class CombinePackage
		{
			public UInt16 groupId;
			public UInt16 nGroupCount;

			public UInt16 nPackageId;
			public Dictionary<UInt16, NetReceivePackage> mReceivePackageDic;

			public bool CheckCombinFinish()
			{
				return mReceivePackageDic.Count == nGroupCount;
			}

			public NetReceivePackage GetCombinePackage()
			{
				byte[] buffer = new byte[nGroupCount * ClientConfig.nMaxBufferSize];
				int Length = 0;
				for (UInt16 i = groupId; i < groupId + nGroupCount; i++) {
					ArraySegment<byte> tempBuf = mReceivePackageDic [i].buffer;
					Array.Copy (tempBuf.Array, tempBuf.Offset, buffer, i * ClientConfig.nMaxBufferSize, ClientConfig.nMaxBufferSize);
					Length += tempBuf.Count;
				}

				NetReceivePackage mNetReceivePackage = new NetReceivePackage ();
				mNetReceivePackage.buffer = new ArraySegment<byte> (buffer, 0, Length);
				mNetReceivePackage.nOrderId = groupId;
				mNetReceivePackage.nPackageId = nPackageId;

				return mNetReceivePackage;
			}
		}

		public void AddReceiveCheck(NetReceivePackage mPackage)
		{
			CheckReceivePackageLoss (mPackage);
		}

		private void CheckReceivePackageLoss(NetReceivePackage mPackage)
		{
			if (mPackage.nOrderId == nCurrentWaitReceiveOrderId) {
				nCurrentWaitReceiveOrderId++;
				CheckCombinePackage (mPackage);

				while (true) {
					if (mReceivePackageQueue.ContainsKey (nCurrentWaitReceiveOrderId)) {
						mPackage = mReceivePackageQueue [nCurrentWaitReceiveOrderId];
						mReceivePackageQueue.Remove (nCurrentWaitReceiveOrderId);
						CheckCombinePackage (mPackage);

						nCurrentWaitReceiveOrderId++;
					} else {
						break;
					}
				}
			} else if (mPackage.nOrderId > nCurrentWaitReceiveOrderId) {
				mReceivePackageQueue [mPackage.nOrderId] = mPackage;
			} else {
				DebugSystem.Log ("接受 过去的 废物包： " + mPackage.nPackageId);
			}
		}

		private void CheckCombinePackage(NetReceivePackage mPackage)
		{
			if (mPackage.nGroupCount > 1) {
				UInt16 groupId = mPackage.nOrderId;

				CombinePackage cc = new CombinePackage ();
				cc.groupId = groupId;
				cc.nGroupCount = mPackage.nGroupCount;
				cc.mReceivePackageDic = new Dictionary<UInt16, NetReceivePackage> ();
				cc.mReceivePackageDic [mPackage.nOrderId] = mPackage;
				mReceiveGroupList.Add (cc);

				return;
			} else {

				for (int i = 0; i < mReceiveGroupList.Count; i++) {
					var currentGroup = mReceiveGroupList [i];
					if (currentGroup.groupId + currentGroup.nGroupCount > mPackage.nOrderId &&
						currentGroup.groupId < mPackage.nOrderId) {

						currentGroup.mReceivePackageDic [mPackage.nOrderId] = mPackage;

						if (currentGroup.CheckCombinFinish ()) {
							mPackage = currentGroup.GetCombinePackage ();
							mUdpPeer.AddLogicHandleQueue (mPackage);
						}

						return;
					}
				}

				mUdpPeer.AddLogicHandleQueue (mPackage);
			}
		}

		public void Update()
		{

		}
			
	}
}