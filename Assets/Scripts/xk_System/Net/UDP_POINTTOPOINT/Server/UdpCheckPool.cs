using System.Collections;
using System.Collections.Generic;
using System;
using System.Collections.Concurrent;
using xk_System.Net.UDP.POINTTOPOINT.Protocol;
using xk_System.Debug;
using UdpPointtopointProtocols;

namespace xk_System.Net.UDP.POINTTOPOINT.Server
{
	public class UdpCheckPool
	{
		class CheckPackageInfo
		{
			public NetUdpFixedSizePackage mPackage;
			public Timer mTimer;
			public int nReSendCount;

			public CheckPackageInfo ()
			{
				mTimer = new Timer ();
			}
		}

		private const int nMaxReSendCount = 5;
		private const float nReSendTime = 3000.0f;

		private SafeObjectPool<CheckPackageInfo> mCheckPackagePool = null;
		private ConcurrentDictionary<UInt16, CheckPackageInfo> mWaitCheckSendDic = null;
		private ConcurrentDictionary<UInt16, CheckPackageInfo> mWaitCheckReceiveDic = null;
		private ClientPeer mUdpPeer = null;

		private UInt16 nCurrentWaitReceiveOrderId;
		private ConcurrentDictionary<UInt16, NetUdpFixedSizePackage> mReceivePackageDic = null;
		private ConcurrentBag<NetCombinePackage> mReceiveGroupList = null;

		public UdpCheckPool (ClientPeer mUdpPeer)
		{
			mCheckPackagePool = new SafeObjectPool<CheckPackageInfo> ();
			mWaitCheckSendDic = new ConcurrentDictionary<ushort, CheckPackageInfo> ();
			mWaitCheckReceiveDic = new ConcurrentDictionary<ushort, CheckPackageInfo> ();

			mReceivePackageDic = new ConcurrentDictionary<ushort, NetUdpFixedSizePackage> ();
			nCurrentWaitReceiveOrderId = 1;
			mReceiveGroupList = new ConcurrentBag<NetCombinePackage> ();

			this.mUdpPeer = mUdpPeer;
			PackageManager.Instance.addNetListenFun (UdpNetCommand.COMMAND_PACKAGECHECK, ReceiveCheckPackage);
		}

		private void ReceiveCheckPackage (ClientPeer peer, NetPackage mPackage)
		{
			PackageCheckResult mPackageCheckResult = Protocol3Utility.getData<PackageCheckResult> (mPackage);
			UInt16 whoId = (UInt16)(mPackageCheckResult.NWhoOrderId >> 16);
			UInt16 nOrderId = (UInt16)(mPackageCheckResult.NWhoOrderId & 0x0000FFFF);

			//DebugSystem.Log ("Server: nWhoId: " + whoId + " | nOrderId: " + nOrderId);

			if (whoId == 2) {
				this.mUdpPeer.SendNetStream (mPackage as NetUdpFixedSizePackage);

				if (mWaitCheckSendDic.ContainsKey (nOrderId)) {
					mUdpPeer.RecycleNetUdpFixedPackage (mWaitCheckSendDic [nOrderId].mPackage);
					CheckPackageInfo mCheckInfo = null;
					if (!mWaitCheckSendDic.TryRemove (nOrderId, out mCheckInfo)) {
						DebugSystem.LogError ("mWaitCheckSendDic Remove 失败");
					}
					mCheckPackagePool.recycle (mCheckInfo);
				} else {
					DebugSystem.LogError ("不存在的发送 OrderId: " + nOrderId);
				}
			} else if (whoId == 1) {
				if (mWaitCheckReceiveDic.ContainsKey (nOrderId)) {
					mUdpPeer.RecycleNetUdpFixedPackage (mWaitCheckReceiveDic [nOrderId].mPackage);
					CheckPackageInfo mCheckInfo = null;
					if (!mWaitCheckReceiveDic.TryRemove (nOrderId, out mCheckInfo)) {
						DebugSystem.LogError ("mWaitCheckReceiveDic Remove 失败");
					}
					mCheckPackagePool.recycle (mCheckInfo);
				} else {
					DebugSystem.LogError ("不存在的接受 OrderId: " + nOrderId);
				}
			}
		}

		public void AddSendCheck (UInt16 nOrderId, NetUdpFixedSizePackage sendBuff)
		{
			if (!ServerConfig.bNeedCheckPackage) {
				return;
			}

			CheckPackageInfo mCheckInfo = mCheckPackagePool.Pop ();
			mCheckInfo.nReSendCount = 0;
			mCheckInfo.mPackage = sendBuff;
			mCheckInfo.mTimer.restart ();
			mWaitCheckSendDic [nOrderId] = mCheckInfo;
		}

		public void AddReceiveCheck (NetUdpFixedSizePackage mReceiveLogicPackage)
		{
			if (!ServerConfig.bNeedCheckPackage) {
				CheckCombinePackage (mReceiveLogicPackage);
				return;
			}

			CheckReceivePackageLoss (mReceiveLogicPackage);

			PackageCheckResult mResult = new PackageCheckResult ();
			mResult.NWhoOrderId = (UInt32)(1 << 16 | mReceiveLogicPackage.nOrderId);
			NetUdpFixedSizePackage mCheckResultPackage = mUdpPeer.GetCheckResultPackage (UdpNetCommand.COMMAND_PACKAGECHECK, mResult);
			mUdpPeer.SendNetStream (mCheckResultPackage);

			CheckPackageInfo mCheckInfo = mCheckPackagePool.Pop ();
			mCheckInfo.nReSendCount = 0;
			mCheckInfo.mPackage = mCheckResultPackage;
			mCheckInfo.mTimer.restart ();
			mWaitCheckReceiveDic [mReceiveLogicPackage.nOrderId] = mCheckInfo;
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

						NetUdpFixedSizePackage mTempPackage = null;
						if (!mReceivePackageDic.TryRemove (nCurrentWaitReceiveOrderId, out mTempPackage)) {
							DebugSystem.LogError ("11111111111111111111111111111");
						}
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

				cc.nCombineGroupId = mPackage.nOrderId;
				cc.nCombinePackageId = mPackage.nPackageId;
				cc.nCombineGroupCount = mPackage.nGroupCount;

				cc.mReceivePackageDic [mPackage.nOrderId] = mPackage;
				cc.mNeedRecyclePackage.Add (mPackage);

				mReceiveGroupList.Add (cc);
			} else {

				bool bInCombineGroup = false;
				NetCombinePackage mRemoveNetCombinePackage = null;

				var iter = mReceiveGroupList.GetEnumerator ();
				while (iter.MoveNext ()) {
					NetCombinePackage currentGroup = iter.Current;
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
						if (!mReceiveGroupList.TryTake (out mRemoveNetCombinePackage)) {
							DebugSystem.LogError ("移除失败");
						}
					}
				} else {
					mUdpPeer.AddLogicHandleQueue (mPackage);
				}
			}
		}

		public void Update (double elapsed)
		{
			var iter1 = mWaitCheckSendDic.GetEnumerator ();
			while (iter1.MoveNext ()) {
				CheckPackageInfo mCheckInfo = iter1.Current.Value;
				if (mCheckInfo.mTimer.elapsed () > nReSendTime) {
					mCheckInfo.nReSendCount++;
					if (mCheckInfo.nReSendCount > nMaxReSendCount) {
						DebugSystem.LogError ("Server 发送超时");
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
						DebugSystem.LogError ("Server 发送超时");
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