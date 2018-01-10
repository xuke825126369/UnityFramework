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
			public UInt16 nOrderId;
			public NetUdpFixedSizePackage mPackage;
			public Timer mTimer;
			public int nReSendCount;

			public CheckPackageInfo ()
			{
				mTimer = new Timer ();
				mTimer.restart ();
			}
		}

		private const int nMaxReSendCount = 5;
		private const float nReSendTime = 3000.0f;

		private ObjectPool<CheckPackageInfo> mCheckPackagePool = null;

		private Dictionary<UInt16, CheckPackageInfo> mWaitCheckSendDic = null;
		private Dictionary<UInt16, CheckPackageInfo> mWaitCheckReceiveDic = null;

		private List<UInt16> mWaitRemoveCheckSendOrderIdList = null;
		private List<UInt16> mWaitRemoveCheckReceiveOrderIdList = null;

		private List<CheckPackageInfo> mWaitAddCheckSendOrderIdList = null;
		private List<CheckPackageInfo> mWaitAddCheckReceiveOrderIdList = null;

		private ClientPeer mUdpPeer = null;

		private UInt16 nCurrentWaitReceiveOrderId;
		private Dictionary<UInt16, NetUdpFixedSizePackage> mReceivePackageDic = null;
		private List<NetCombinePackage> mReceiveGroupList = null;

		public UdpCheckPool (ClientPeer mUdpPeer)
		{
			mCheckPackagePool = new ObjectPool<CheckPackageInfo> ();
			mWaitCheckSendDic = new Dictionary<ushort, CheckPackageInfo> ();
			mWaitCheckReceiveDic = new Dictionary<ushort, CheckPackageInfo> ();

			mWaitRemoveCheckSendOrderIdList = new List<ushort> ();
			mWaitRemoveCheckReceiveOrderIdList = new List<ushort> ();

			mWaitAddCheckSendOrderIdList = new List<CheckPackageInfo> ();
			mWaitAddCheckReceiveOrderIdList = new List<CheckPackageInfo> ();

			mReceivePackageDic = new Dictionary<ushort, NetUdpFixedSizePackage> ();
			nCurrentWaitReceiveOrderId = 1;
			mReceiveGroupList = new List<NetCombinePackage> ();

			this.mUdpPeer = mUdpPeer;
			mUdpPeer.addNetListenFun (UdpNetCommand.COMMAND_PACKAGECHECK, ReceiveCheckPackage);
		}

		private void ReceiveCheckPackage (NetPackage mPackage)
		{
			PackageCheckResult mPackageCheckResult = Protocol3Utility.getData<PackageCheckResult> (mPackage);
			UInt16 whoId = (UInt16)(mPackageCheckResult.NWhoOrderId >> 16);
			UInt16 nOrderId = (UInt16)(mPackageCheckResult.NWhoOrderId & 0x0000FFFF);

			//DebugSystem.Log ("Client: nWhoId: " + whoId + " | nOrderId: " + nOrderId);
			if (whoId == 1) {
				this.mUdpPeer.SendNetStream (mPackage as NetUdpFixedSizePackage);
				if (mWaitCheckSendDic.ContainsKey (nOrderId)) {
					mUdpPeer.RecycleNetUdpFixedPackage (mWaitCheckSendDic [nOrderId].mPackage);

					if (!mWaitRemoveCheckSendOrderIdList.Contains (nOrderId)) {
						mWaitRemoveCheckSendOrderIdList.Add (nOrderId);
					}
				} else {
					DebugSystem.LogError ("不存在的发送 OrderId: " + nOrderId);
				}
			} else if (whoId == 2) {
				if (mWaitCheckReceiveDic.ContainsKey (nOrderId)) {
					mUdpPeer.RecycleNetUdpFixedPackage (mWaitCheckReceiveDic [nOrderId].mPackage);
					if (!mWaitRemoveCheckReceiveOrderIdList.Contains (nOrderId)) {
						mWaitRemoveCheckReceiveOrderIdList.Add (nOrderId);
					}
				} else {
					DebugSystem.LogError ("不存在的接受 OrderId: " + nOrderId);
				}
			}
		}

		public void AddSendCheck (NetUdpFixedSizePackage mPackage)
		{
			if (!ClientConfig.bNeedCheckPackage) {
				return;
			}

			CheckPackageInfo mCheckInfo = mCheckPackagePool.Pop ();
			mCheckInfo.nReSendCount = 0;
			mCheckInfo.mPackage = mPackage;
			mCheckInfo.mTimer.restart ();
			mCheckInfo.nOrderId = mPackage.nOrderId;

			mWaitAddCheckSendOrderIdList.Add (mCheckInfo);
		}

		public void AddReceiveCheck (NetUdpFixedSizePackage mReceiveLogicPackage)
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
			mCheckInfo.nReSendCount = 0;
			mCheckInfo.mPackage = mCheckResultPackage;
			mCheckInfo.mTimer.restart ();
			mCheckInfo.nOrderId = mReceiveLogicPackage.nOrderId;

			mWaitAddCheckReceiveOrderIdList.Add (mCheckInfo);
		}

		private void CheckReceivePackageLoss (NetUdpFixedSizePackage mPackage)
		{
			if (mPackage.nOrderId == nCurrentWaitReceiveOrderId) {
				nCurrentWaitReceiveOrderId++;
				if (nCurrentWaitReceiveOrderId == 0) {
					nCurrentWaitReceiveOrderId = 1;
				}
				CheckCombinePackage (mPackage);

				while (true) {
					if (mReceivePackageDic.ContainsKey (nCurrentWaitReceiveOrderId)) {
						mPackage = mReceivePackageDic [nCurrentWaitReceiveOrderId];
						mReceivePackageDic.Remove (nCurrentWaitReceiveOrderId);
						CheckCombinePackage (mPackage);

						nCurrentWaitReceiveOrderId++;
						if (nCurrentWaitReceiveOrderId == 0) {
							nCurrentWaitReceiveOrderId = 1;
						}
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

		private void CheckCombinePackage (NetUdpFixedSizePackage mPackage)
		{
			if (mPackage.nGroupCount > 1) {
				NetCombinePackage cc = mUdpPeer.GetNetCombinePackage ();
				cc.mReceivePackageDic.Clear ();
				cc.mNeedRecyclePackage.Clear ();

				cc.nCombineGroupId = mPackage.nOrderId;
				cc.nCombinePackageId = mPackage.nPackageId;
				cc.nCombineGroupCount = mPackage.nGroupCount;

				cc.mReceivePackageDic [mPackage.nOrderId] = mPackage;
				cc.mNeedRecyclePackage.Add (mPackage);

				mReceiveGroupList.Add (cc);
			} else {

				bool bInCombineGroup = false;
				NetCombinePackage mRemoveNetCombinePackage = null;
				for (int i = 0; i < mReceiveGroupList.Count; i++) {
					var currentGroup = mReceiveGroupList [i];

					if (currentGroup.bInCombinePackage (mPackage)) {
						currentGroup.mReceivePackageDic [mPackage.nOrderId] = mPackage;
						currentGroup.mNeedRecyclePackage.Add (mPackage);

						if (currentGroup.CheckCombineFinish ()) {
							mRemoveNetCombinePackage = currentGroup;
						}
						bInCombineGroup = true;
						break;
					}
				}

				if (bInCombineGroup) {
					if (mRemoveNetCombinePackage != null) {
						mUdpPeer.AddLogicHandleQueue (mRemoveNetCombinePackage);
						mReceiveGroupList.Remove (mRemoveNetCombinePackage);
					}
				} else {
					mUdpPeer.AddLogicHandleQueue (mPackage);
				}
			}
		}

		public void Update (double elapsed)
		{
			for (int i = 0; i < mWaitRemoveCheckSendOrderIdList.Count; ++i) {
				mWaitCheckSendDic.Remove (mWaitRemoveCheckSendOrderIdList [i]);
			}
			mWaitRemoveCheckSendOrderIdList.Clear ();

			for (int i = 0; i < mWaitRemoveCheckReceiveOrderIdList.Count; ++i) {
				mWaitCheckReceiveDic.Remove (mWaitRemoveCheckReceiveOrderIdList [i]);
			}
			mWaitRemoveCheckReceiveOrderIdList.Clear ();

			for (int i = 0; i < mWaitAddCheckSendOrderIdList.Count; ++i) {
				CheckPackageInfo mPackageInfo = mWaitAddCheckSendOrderIdList [i];
				mWaitCheckSendDic [mPackageInfo.nOrderId] = mPackageInfo;
			}
			mWaitAddCheckSendOrderIdList.Clear ();

			for (int i = 0; i < mWaitAddCheckReceiveOrderIdList.Count; ++i) {
				CheckPackageInfo mPackageInfo = mWaitAddCheckReceiveOrderIdList [i];
				mWaitCheckReceiveDic [mPackageInfo.nOrderId] = mPackageInfo;
			}
			mWaitAddCheckReceiveOrderIdList.Clear ();

			var iter1 = mWaitCheckSendDic.GetEnumerator ();
			while (iter1.MoveNext ()) {
				CheckPackageInfo mCheckInfo = iter1.Current.Value;
				if (mCheckInfo.mTimer.elapsed () > nReSendTime) {
					mCheckInfo.nReSendCount++;
					if (mCheckInfo.nReSendCount > nMaxReSendCount) {
						DebugSystem.LogError ("Client 发送超时");
						return;
					}

					this.mUdpPeer.SendNetStream (mCheckInfo.mPackage);
					mCheckInfo.mTimer.restart ();
				}
			}

			var iter2 = mWaitCheckReceiveDic.GetEnumerator ();
			while (iter2.MoveNext ()) {
				CheckPackageInfo mCheckInfo = iter2.Current.Value;
				if (mCheckInfo.mTimer.elapsed () > nReSendTime) {
					mCheckInfo.nReSendCount++;
					if (mCheckInfo.nReSendCount > nMaxReSendCount) {
						DebugSystem.LogError ("Client 发送超时");
						return;
					}

					this.mUdpPeer.SendNetStream (mCheckInfo.mPackage);
					mCheckInfo.mTimer.restart ();
				}
			}
		}


		public void release ()
		{

		}
	}




}