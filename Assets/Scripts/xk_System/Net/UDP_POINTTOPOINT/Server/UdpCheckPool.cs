using System.Collections;
using System.Collections.Generic;
using System;
using xk_System.Net.UDP.POINTTOPOINT.Protocol;
using xk_System.Debug;
using UdpPointtopointProtocols;

namespace xk_System.Net.UDP.POINTTOPOINT.Server
{
	public class UdpCheckPool
	{
		class CheckSendPackageInfo
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
				byte[] buffer = new byte[nGroupCount * ServerConfig.nMaxBufferSize];
				int Length = 0;
				for (UInt16 i = groupId; i < groupId + nGroupCount; i++) {
					byte[] tempBuf = mReceivePackageDic [i].buffer;
					Array.Copy (tempBuf, mReceivePackageDic [i].Offset, buffer, i * ServerConfig.nMaxBufferSize, ServerConfig.nMaxBufferSize);
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

		private const int nMaxReSendCount = 5;
		private const float nReSendTime = 3.0f;

		private Dictionary<UInt16, CheckSendPackageInfo> mWaitCheckSendDic = null;
		private Dictionary<UInt16, CheckSendPackageInfo> mWaitCheckReceiveDic =  null;
		private ClientPeer mUdpPeer;

		private UInt16 nCurrentWaitReceiveOrderId;
		private Dictionary<UInt16, NetUdpFixedSizePackage> mReceivePackageDic = null;
		private List<CheckCombinePackageInfo> mReceiveGroupList = null;

		public UdpCheckPool(ClientPeer mUdpPeer)
		{
			mWaitCheckSendDic = new Dictionary<ushort, CheckSendPackageInfo> ();
			mWaitCheckReceiveDic = new Dictionary<ushort, CheckSendPackageInfo> ();
			this.mUdpPeer = mUdpPeer;
			mUdpPeer.addNetListenFun (UdpNetCommand.COMMAND_PACKAGECHECK, ReceiveCheckPackage);

			mReceivePackageDic = new Dictionary<ushort, NetUdpFixedSizePackage> ();
			nCurrentWaitReceiveOrderId = 1;
			mReceiveGroupList = new List<CheckCombinePackageInfo> ();
		}

		private void ReceiveCheckPackage(NetPackage mPackage)
		{
			PackageCheckResult mPackageCheckResult = Protocol3Utility.getData<PackageCheckResult> (mPackage.buffer, 0, mPackage.Length);
			UInt16 whoId = (UInt16)(mPackageCheckResult.NWhoOrderId >> 16);
			UInt16 nOrderId = (UInt16)(mPackageCheckResult.NWhoOrderId & 0x0000FFFF);

			if (whoId == 2) {
				this.mUdpPeer.SendNetData (mPackage.nPackageId, mPackageCheckResult);

				if (mWaitCheckSendDic.ContainsKey (nOrderId)) {
					mWaitCheckSendDic.Remove (nOrderId);
				}
			} else if (whoId == 1) {
				mWaitCheckReceiveDic [nOrderId].nReceiveCheckResultCount++;

				if (mWaitCheckReceiveDic [nOrderId].nReceiveCheckResultCount >= 2) {
					if (mWaitCheckReceiveDic.ContainsKey (nOrderId)) {
						mWaitCheckReceiveDic.Remove (nOrderId);
					}
				}

			}
		}

		public void AddSendCheck(UInt16 nOrderId, NetUdpFixedSizePackage sendBuff)
		{
			CheckSendPackageInfo mCheckInfo = new CheckSendPackageInfo ();
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
					NetUdpFixedSizePackage mPackage = mWaitCheckSendDic [nOrderId].mPackage;
					this.mUdpPeer.SendNetStream (mPackage.buffer, mPackage.Offset, mPackage.Length);
					mCheckInfo.nReSendCount++;
					if (mCheckInfo.nReSendCount >= nMaxReSendCount) {
						mCheckInfo.mTimer.Stop ();
						DebugSystem.LogError ("发送超时");
					}
				} else {
					mCheckInfo.mTimer.Stop ();
				}
			};
		}

		public void AddReceiveCheck(NetUdpFixedSizePackage mPackage)
		{
			CheckReceivePackageLoss (mPackage);

			PackageCheckResult mResult = new PackageCheckResult ();
			mResult.NWhoOrderId = (UInt32)(1 << 16 | mPackage.nOrderId);
			mUdpPeer.SendNetData (UdpNetCommand.COMMAND_PACKAGECHECK, mResult);
		}

		private void CheckReceivePackageLoss(NetUdpFixedSizePackage mPackage)
		{
			if (mPackage.nOrderId == nCurrentWaitReceiveOrderId) {
				nCurrentWaitReceiveOrderId++;
				CheckCheckCombinePackageInfo (mPackage);

				while (true) {
					if (mReceivePackageDic.ContainsKey (nCurrentWaitReceiveOrderId)) {
						mPackage = mReceivePackageDic [nCurrentWaitReceiveOrderId];
						mReceivePackageDic.Remove (nCurrentWaitReceiveOrderId);
						CheckCheckCombinePackageInfo (mPackage);

						nCurrentWaitReceiveOrderId++;
					} else {
						break;
					}
				}
			} else if (mPackage.nOrderId > nCurrentWaitReceiveOrderId) {
				mReceivePackageDic [mPackage.nOrderId] = mPackage;
			} else {
				DebugSystem.Log ("接受 过去的 废物包： " + mPackage.nPackageId);
			}
		}

		private void CheckCheckCombinePackageInfo(NetUdpFixedSizePackage mPackage)
		{
			if (mPackage.nGroupCount > 1) {
				UInt16 groupId = mPackage.nOrderId;

				CheckCombinePackageInfo cc = new CheckCombinePackageInfo ();
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
							NetCombinePackage mPackage1 = currentGroup.GetCheckCombinePackageInfo ();
							mUdpPeer.AddLogicHandleQueue (mPackage1);
						}

						return;
					}
				}

				mUdpPeer.AddLogicHandleQueue (mPackage);
			}
		}
	}


}