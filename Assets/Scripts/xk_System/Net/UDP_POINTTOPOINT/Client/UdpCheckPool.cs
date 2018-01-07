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
		class CheckPackageInfo
		{
			public NetUdpFixedSizePackage mPackage;
			public int nReceiveCheckResultCount;
			public System.Timers.Timer mTimer;
			public int nReSendCount;
		}

		class CheckCombinePackageInfo
		{
			public UInt16 groupId;
			public UInt16 nGroupCount;

			public UInt16 nPackageId;
			public Dictionary<UInt16, NetUdpFixedSizePackage> mReceivePackageDic;

			public bool CheckCombinFinish()
			{
				return mReceivePackageDic.Count == nGroupCount;
			}

			public NetCombinePackage GetCheckCombinePackageInfo()
			{
				NetCombinePackage mNetReceivePackage = new NetCombinePackage ();
				int nSumLength = nGroupCount * ClientConfig.nUdpPackageFixedBodySize + ClientConfig.nUdpPackageFixedHeadSize;
				if (mNetReceivePackage.buffer.Length < nSumLength) {
					mNetReceivePackage.buffer = new byte[nSumLength];
				}

				mNetReceivePackage.Length = ClientConfig.nUdpPackageFixedHeadSize;
				for (UInt16 i = groupId; i < groupId + nGroupCount; i++) {
					byte[] tempBuf = mReceivePackageDic [i].buffer;
					Array.Copy (tempBuf, ClientConfig.nUdpPackageFixedHeadSize, mNetReceivePackage.buffer, mNetReceivePackage.Length, (mReceivePackageDic [i].Length - ClientConfig.nUdpPackageFixedHeadSize));
					mNetReceivePackage.Length += (mReceivePackageDic [i].Length - ClientConfig.nUdpPackageFixedHeadSize);
				}

				mNetReceivePackage.nOrderId = groupId;
				mNetReceivePackage.nPackageId = nPackageId;

				return mNetReceivePackage;
			}
		}

		private const int nMaxReSendCount = 5;
		private const float nReSendTime = 3000.0f;

		private ObjectPool<CheckPackageInfo> mCheckPackagePool;
		private Dictionary<UInt16, CheckPackageInfo> mWaitCheckSendDic = null;
		private Dictionary<UInt16, CheckPackageInfo> mWaitCheckReceiveDic =  null;
		private ClientPeer mUdpPeer;

		private UInt16 nCurrentWaitReceiveOrderId;
		private Dictionary<UInt16, NetUdpFixedSizePackage> mReceivePackageDic = null;
		private List<CheckCombinePackageInfo> mReceiveGroupList = null;

		public UdpCheckPool(ClientPeer mUdpPeer)
		{
			mCheckPackagePool = new ObjectPool<CheckPackageInfo> ();
			mWaitCheckSendDic = new Dictionary<ushort, CheckPackageInfo> ();
			mWaitCheckReceiveDic = new Dictionary<ushort, CheckPackageInfo> ();
			this.mUdpPeer = mUdpPeer;
			mUdpPeer.addNetListenFun (UdpNetCommand.COMMAND_PACKAGECHECK, ReceiveCheckPackage);

			mReceivePackageDic = new Dictionary<ushort, NetUdpFixedSizePackage> ();
			nCurrentWaitReceiveOrderId = 1;
			mReceiveGroupList = new List<CheckCombinePackageInfo> ();
		}

		private void ReceiveCheckPackage(NetPackage mPackage)
		{
			PackageCheckResult mPackageCheckResult = Protocol3Utility.getData<PackageCheckResult> (mPackage);
			UInt16 whoId = (UInt16)(mPackageCheckResult.NWhoOrderId >> 16);
			UInt16 nOrderId = (UInt16)(mPackageCheckResult.NWhoOrderId & 0x0000FFFF);

			DebugSystem.Log ("Client: nWhoId: " + whoId + " | nOrderId: " + nOrderId);
			if (whoId == 1) {
				this.mUdpPeer.SendNetStream (mPackage as NetUdpFixedSizePackage);
				mWaitCheckSendDic.Remove (nOrderId);
			} else if (whoId == 2) {
				mWaitCheckReceiveDic.Remove (nOrderId);
			}
		}

		public void AddSendCheck(UInt16 nOrderId, NetUdpFixedSizePackage sendBuff)
		{
			if (!ClientConfig.bNeedCheckPackage) {
				return;
			}

			CheckPackageInfo mCheckInfo = mCheckPackagePool.Pop ();
			mCheckInfo.nReceiveCheckResultCount = 0;
			mCheckInfo.nReSendCount = 0;
			mCheckInfo.mPackage = sendBuff;
			mWaitCheckSendDic [nOrderId] = mCheckInfo;

			mCheckInfo.mTimer = new System.Timers.Timer ();
			mCheckInfo.mTimer.Interval = nReSendTime;
			mCheckInfo.mTimer.AutoReset = true;
			mCheckInfo.mTimer.Start ();

			mCheckInfo.mTimer.Elapsed += (object sender, System.Timers.ElapsedEventArgs args) => {
				if (mWaitCheckSendDic.ContainsKey (nOrderId)) {
					mCheckInfo.nReSendCount++;
					if (mCheckInfo.nReSendCount > nMaxReSendCount) {
						mCheckInfo.mTimer.Stop ();
						DebugSystem.LogError ("Client 发送超时");
						return;
					}
					NetUdpFixedSizePackage mPackage = mWaitCheckSendDic [nOrderId].mPackage;
					this.mUdpPeer.SendNetStream (mPackage.buffer, 0, mPackage.Length);
				} else {
					mCheckInfo.mTimer.Stop ();
				}
			};
		}

		public void AddReceiveCheck(NetUdpFixedSizePackage mReceiveLogicPackage)
		{
			if (!ClientConfig.bNeedCheckPackage) {
				CheckCombinePackage (mReceiveLogicPackage);
				return;
			}

			CheckReceivePackageLoss (mReceiveLogicPackage);

			PackageCheckResult mResult = new PackageCheckResult ();
			mResult.NWhoOrderId = (UInt32)(2 << 16 | mReceiveLogicPackage.nOrderId);
			NetUdpFixedSizePackage mCheckResultPackage = mUdpPeer.GetCheckResultPackage (UdpNetCommand.COMMAND_PACKAGECHECK, mResult);
			mUdpPeer.SendNetStream (mCheckResultPackage);

			CheckPackageInfo mCheckInfo = mCheckPackagePool.Pop ();
			mCheckInfo.nReceiveCheckResultCount = 0;
			mCheckInfo.nReSendCount = 0;
			mCheckInfo.mPackage = mCheckResultPackage;
			mWaitCheckReceiveDic [mReceiveLogicPackage.nOrderId] = mCheckInfo;

			mCheckInfo.mTimer = new System.Timers.Timer ();
			mCheckInfo.mTimer.Interval = nReSendTime;
			mCheckInfo.mTimer.AutoReset = true;
			mCheckInfo.mTimer.Start ();

			mCheckInfo.mTimer.Elapsed += (object sender, System.Timers.ElapsedEventArgs args) => {
				if (mWaitCheckSendDic.ContainsKey (mReceiveLogicPackage.nOrderId)) {
					NetUdpFixedSizePackage mPackage = mWaitCheckSendDic [mReceiveLogicPackage.nOrderId].mPackage;
					this.mUdpPeer.SendNetStream (mPackage.buffer, 0, mPackage.Length);
					mCheckInfo.nReSendCount++;
					if (mCheckInfo.nReSendCount >= nMaxReSendCount) {
						mCheckInfo.mTimer.Stop ();
						DebugSystem.LogError ("Client 发送超时");
					}
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
					if (mReceivePackageDic.ContainsKey (nCurrentWaitReceiveOrderId)) {
						mPackage = mReceivePackageDic [nCurrentWaitReceiveOrderId];
						mReceivePackageDic.Remove (nCurrentWaitReceiveOrderId);
						CheckCombinePackage (mPackage);

						nCurrentWaitReceiveOrderId++;
					} else {
						break;
					}
				}
			} else if (mPackage.nOrderId > nCurrentWaitReceiveOrderId) {
				mReceivePackageDic [mPackage.nOrderId] = mPackage;
			} else {
				DebugSystem.Log ("Client 接受 过去的 废物包： " + mPackage.nPackageId);
			}
		}

		private void CheckCombinePackage(NetUdpFixedSizePackage mPackage)
		{
			if (mPackage.nGroupCount > 1) {
				UInt16 groupId = mPackage.nOrderId;

				CheckCombinePackageInfo cc = new CheckCombinePackageInfo ();
				cc.groupId = groupId;
				cc.nPackageId = mPackage.nPackageId;
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
							NetCombinePackage mPackage1 = currentGroup.GetCheckCombinePackageInfo ();
							mUdpPeer.AddLogicHandleQueue (mPackage1);
						}

						return;
					}
				}

				mUdpPeer.AddLogicHandleQueue (mPackage);
			}
		}


		public void Update(double elapsed)
		{
			
		}
	}




}