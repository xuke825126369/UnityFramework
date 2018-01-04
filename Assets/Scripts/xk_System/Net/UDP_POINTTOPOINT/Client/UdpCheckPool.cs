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
		class CheckInfo
		{
			public const int SendCheckCount = 3;
			public const int ReceiveCheckCount = 2;
			public ArraySegment<byte> mArraySegment;
			public int nReceiveCheckResultCount;
			public System.Timers.Timer mTimer;
		}

		class CombinePackage
		{
			public UInt16 groupId;
			public UInt16 nGroupCount;

			public UInt16 nPackageId;
			public Dictionary<UInt16, NetUdpFixedSizePackage> mReceivePackageDic;

			public bool CheckCombinFinish()
			{
				return mReceivePackageDic.Count == nGroupCount;
			}

			public NetCombinePackage GetCombinePackage()
			{
				byte[] buffer = new byte[nGroupCount * ClientConfig.nMaxBufferSize];
				int Length = 0;
				for (UInt16 i = groupId; i < groupId + nGroupCount; i++) {
					byte[] tempBuf = mReceivePackageDic [i].buffer;
					Array.Copy (tempBuf, mReceivePackageDic [i].Offset, buffer, i * ClientConfig.nMaxBufferSize, ClientConfig.nMaxBufferSize);
					Length += mReceivePackageDic [i].Length;
				}

				NetCombinePackage mNetReceivePackage = new NetCombinePackage ();
				mNetReceivePackage.buffer = buffer;
				mNetReceivePackage.Offset = 0;
				mNetReceivePackage.Length = Length;

				mNetReceivePackage.nOrderId = groupId;
				mNetReceivePackage.nPackageId = nPackageId;

				return mNetReceivePackage;
			}
		}


		private Dictionary<UInt16, CheckInfo> mWaitCheckSendDic = null;
		private Dictionary<UInt16, CheckInfo> mWaitCheckReceiveDic =  null;
		private ClientPeer mUdpPeer;

		private UInt16 nCurrentWaitReceiveOrderId = 1;
		private Dictionary<UInt16, NetUdpFixedSizePackage> mReceivePackageQueue = new Dictionary<UInt16, NetUdpFixedSizePackage> ();
		private List<CombinePackage> mReceiveGroupList = new List<CombinePackage> ();

		public UdpCheckPool(ClientPeer mUdpPeer)
		{
			mWaitCheckSendDic = new Dictionary<ushort, CheckInfo> ();
			mWaitCheckReceiveDic = new Dictionary<ushort, CheckInfo> ();
			this.mUdpPeer = mUdpPeer;
			mUdpPeer.addNetListenFun (UdpNetCommand.COMMAND_PACKAGECHECK, ReceiveCheckPackage);
		}

		private void ReceiveCheckPackage(NetPackage mPackage)
		{
			PackageCheckResult mPackageCheckResult = Protocol3Utility.getData<PackageCheckResult> (mPackage.buffer, 0, mPackage.Length);
			UInt16 whoId = (UInt16)(mPackageCheckResult.NWhoOrderId >> 16);
			UInt16 nOrderId = (UInt16)(mPackageCheckResult.NWhoOrderId & 0x0000FFFF);

			if (whoId == 1) {
				this.mUdpPeer.SendNetData (mPackage.nPackageId, mPackageCheckResult);

				if (mWaitCheckSendDic.ContainsKey (nOrderId)) {
					mWaitCheckSendDic.Remove (nOrderId);
				}
			} else if (whoId == 2) {
				mWaitCheckReceiveDic [nOrderId].nReceiveCheckResultCount++;

				if (mWaitCheckReceiveDic [nOrderId].nReceiveCheckResultCount >= 2) {
					if (mWaitCheckReceiveDic.ContainsKey (nOrderId)) {
						mWaitCheckReceiveDic.Remove (nOrderId);
					}
				}

			}
		}

		public void AddSendCheck(UInt16 nOrderId, ArraySegment<byte> sendBuff)
		{
			CheckInfo mCheckInfo = new CheckInfo ();
			mCheckInfo.nReceiveCheckResultCount = 0;
			BufferManager.Instance.SetBuffer (out mCheckInfo.mArraySegment);
			Array.Copy (sendBuff.Array, sendBuff.Offset, mCheckInfo.mArraySegment.Array, mCheckInfo.mArraySegment.Offset, sendBuff.Count);
			mWaitCheckSendDic [nOrderId] = mCheckInfo;

			mCheckInfo.mTimer = new System.Timers.Timer ();
			mCheckInfo.mTimer.Interval = 1.0;
			mCheckInfo.mTimer.AutoReset = true;
			mCheckInfo.mTimer.Start ();

			mCheckInfo.mTimer.Elapsed += (object sender, System.Timers.ElapsedEventArgs args) => {
				if (mWaitCheckSendDic.ContainsKey (nOrderId)) {
					ArraySegment<byte> buff = mWaitCheckSendDic [nOrderId].mArraySegment;
					this.mUdpPeer.SendNetStream (buff.Array, buff.Offset, buff.Count);
				} else {
					mCheckInfo.mTimer.Stop ();
				}
			};
		}

		public void AddReceiveCheck(NetUdpFixedSizePackage mPackage)
		{
			CheckReceivePackageLoss (mPackage);

			CheckInfo mCheckInfo = new CheckInfo ();
			mCheckInfo.nReceiveCheckResultCount = 1;
			BufferManager.Instance.SetBuffer (out mCheckInfo.mArraySegment);
			mWaitCheckReceiveDic [mPackage.nOrderId] = mCheckInfo;

			mCheckInfo.mTimer = new System.Timers.Timer ();
			mCheckInfo.mTimer.Interval = 1.0;
			mCheckInfo.mTimer.AutoReset = true;
			mCheckInfo.mTimer.Start ();

			mCheckInfo.mTimer.Elapsed += (object sender, System.Timers.ElapsedEventArgs args) => {
				if (mWaitCheckReceiveDic.ContainsKey (mPackage.nOrderId)) {
					ArraySegment<byte> buff = mWaitCheckReceiveDic [mPackage.nOrderId].mArraySegment;
					this.mUdpPeer.SendNetStream (buff.Array, buff.Offset, buff.Count);
				} else {
					mCheckInfo.mTimer.Stop ();
				}
			};
		}

		private void CheckReceivePackageLoss(NetUdpFixedSizePackage mPackage)
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

		private void CheckCombinePackage(NetUdpFixedSizePackage mPackage)
		{
			if (mPackage.nGroupCount > 1) {
				UInt16 groupId = mPackage.nOrderId;

				CombinePackage cc = new CombinePackage ();
				cc.groupId = groupId;
				cc.nGroupCount = mPackage.nGroupCount;
				cc.mReceivePackageDic = new Dictionary<UInt16, NetUdpFixedSizePackage> ();
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
			
	}
}