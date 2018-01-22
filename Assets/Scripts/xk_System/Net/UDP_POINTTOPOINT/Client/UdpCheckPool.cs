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
			public Timer mTimer;
			public int nReSendCount;

			public CheckPackageInfo ()
			{
				mTimer = new Timer ();
			}
		}

		private const int nMaxReSendCount = 3;
		private const double nReSendTime = 1000.00;
		private const bool bClient = true;

		private ObjectPool<CheckPackageInfo> mCheckPackagePool = null;

		private Dictionary<UInt16, CheckPackageInfo> mWaitCheckSendDic = null;
		private Dictionary<UInt16, CheckPackageInfo> mWaitCheckReceiveDic = null;
		private ClientPeer mUdpPeer = null;

		private UInt16 nCurrentWaitReceiveOrderId;
		private Dictionary<UInt16, NetUdpFixedSizePackage> mReceiveLossPackageDic = null;
		private Queue<NetCombinePackage> mReceiveGroupList = null;

		public UdpCheckPool (ClientPeer mUdpPeer)
		{
			mCheckPackagePool = new ObjectPool<CheckPackageInfo> (8);
			mWaitCheckSendDic = new Dictionary<ushort, CheckPackageInfo> (8);
			mWaitCheckReceiveDic = new Dictionary<ushort, CheckPackageInfo> (8);

			mReceiveLossPackageDic = new Dictionary<ushort, NetUdpFixedSizePackage> ();
			nCurrentWaitReceiveOrderId = ClientConfig.nUdpMinOrderId;
			mReceiveGroupList = new Queue<NetCombinePackage> ();

			this.mUdpPeer = mUdpPeer;
			mUdpPeer.addNetListenFun (UdpNetCommand.COMMAND_PACKAGECHECK, ReceiveCheckPackage);
		}

		private void AddPackageOrderId()
		{
			if (nCurrentWaitReceiveOrderId == ClientConfig.nUdpMaxOrderId) {
				nCurrentWaitReceiveOrderId = ClientConfig.nUdpMinOrderId;
			} else {
				nCurrentWaitReceiveOrderId++;
			}
		}

		private void ReceiveCheckPackage (NetPackage mPackage)
		{
			PackageCheckResult mPackageCheckResult = Protocol3Utility.getData<PackageCheckResult> (mPackage);
			UInt16 whoId = (UInt16)(mPackageCheckResult.NWhoOrderId >> 16);
			UInt16 nOrderId = (UInt16)(mPackageCheckResult.NWhoOrderId & 0x0000FFFF);

			//DebugSystem.Log ("Client Check: nWhoId: " + whoId + " | nOrderId: " + nOrderId);

			bool bSender = bClient ? whoId == 1 : whoId == 2;
			if (bSender) {
				this.mUdpPeer.SendNetStream (mPackage);

				CheckPackageInfo mRemovePackage = null;
				if (mWaitCheckSendDic.TryGetValue (nOrderId, out mRemovePackage)) {
					mUdpPeer.SafeRecycleReceivePackage (mRemovePackage.mPackage);
					mRemovePackage.mPackage = null;

					mWaitCheckSendDic.Remove (nOrderId);
					mCheckPackagePool.recycle (mRemovePackage);
				} else {
					DebugSystem.LogError ("Client 我已经收到 Send确认包了: " + nOrderId);
				}
			} else {
				CheckPackageInfo mRemovePackage = null;
				if (mWaitCheckReceiveDic.TryGetValue (nOrderId, out mRemovePackage)) {
					mUdpPeer.SafeRecycleReceivePackage (mRemovePackage.mPackage);
					mRemovePackage.mPackage = null;

					mWaitCheckReceiveDic.Remove (nOrderId);
					mCheckPackagePool.recycle (mRemovePackage);
				} else {
					DebugSystem.LogError ("Client 我已经收到Receive确认包了: " + nOrderId);
				}
			}
		}

		public void AddSendCheck (NetUdpFixedSizePackage mPackage)
		{
			if (ClientConfig.bNeedCheckPackage) {
				UInt16 nOrderId = mPackage.nOrderId;

				CheckPackageInfo mCheckInfo = mCheckPackagePool.Pop ();
				mCheckInfo.nReSendCount = 0;
				mCheckInfo.mPackage = mPackage;
				mCheckInfo.mTimer.restart ();

				if (!mWaitCheckSendDic.ContainsKey (nOrderId)) {
					mWaitCheckSendDic.Add (nOrderId, mCheckInfo);
				} else {
					DebugSystem.LogError ("Client Add SendCheck Repeated !!!" + nOrderId);
				}
			}

			mUdpPeer.SendNetStream (mPackage);
		}

		public void AddReceiveCheck (NetUdpFixedSizePackage mReceiveLogicPackage)
		{
			if (ClientConfig.bNeedCheckPackage) {
				UInt16 nOrderId = mReceiveLogicPackage.nOrderId;
				PackageCheckResult mResult = new PackageCheckResult ();
				if (bClient) {
					mResult.NWhoOrderId = (UInt32)(2 << 16 | nOrderId);
				} else {
					mResult.NWhoOrderId = (UInt32)(1 << 16 | nOrderId);
				}

				NetUdpFixedSizePackage mCheckResultPackage = mUdpPeer.GetUdpSystemPackage (UdpNetCommand.COMMAND_PACKAGECHECK, mResult);

				CheckPackageInfo mCheckInfo = mCheckPackagePool.Pop ();
				mCheckInfo.nReSendCount = 0;
				mCheckInfo.mPackage = mCheckResultPackage;
				mCheckInfo.mTimer.restart ();

				if (!mWaitCheckReceiveDic.ContainsKey (nOrderId)) {
					mWaitCheckReceiveDic.Add (nOrderId, mCheckInfo);
				}

				mUdpPeer.SendNetStream (mCheckResultPackage);

				CheckReceivePackageLoss (mReceiveLogicPackage);
			} else {
				CheckCombinePackage (mReceiveLogicPackage);
			}
		}

		private void CheckReceivePackageLoss (NetUdpFixedSizePackage mPackage)
		{
			if (mPackage.nOrderId == nCurrentWaitReceiveOrderId) {
				CheckCombinePackage (mPackage);
				AddPackageOrderId ();
				while (mReceiveLossPackageDic.Count > 0) {
					if (mReceiveLossPackageDic.ContainsKey (nCurrentWaitReceiveOrderId)) {
						mPackage = mReceiveLossPackageDic [nCurrentWaitReceiveOrderId];
						mReceiveLossPackageDic.Remove (nCurrentWaitReceiveOrderId);

						CheckCombinePackage (mPackage);
						AddPackageOrderId ();
					} else {
						break;
					}
				}
			} else if (mPackage.nOrderId > nCurrentWaitReceiveOrderId) {
				mReceiveLossPackageDic [mPackage.nOrderId] = mPackage;
				DebugSystem.Log ("Client loss Pcakge: " + nCurrentWaitReceiveOrderId + " | " + mPackage.nOrderId);
			} else {
				DebugSystem.LogError ("Client 接受 过去的 废物包： " + mPackage.nOrderId);
			}
		}

		private void CheckCombinePackage (NetUdpFixedSizePackage mPackage)
		{
			if (mPackage.nGroupCount > 1) {
				NetCombinePackage cc = mUdpPeer.SafeGetNetCombinePackage ();

				cc.nCombineGroupId = mPackage.nOrderId;
				cc.nCombinePackageId = mPackage.nPackageId;
				cc.nCombineGroupCount = mPackage.nGroupCount;

				cc.Add (mPackage);

				mReceiveGroupList.Enqueue (cc);
			} else {
				if (mReceiveGroupList.Count > 0) {
					var currentGroup = mReceiveGroupList.Peek ();
					currentGroup.Add (mPackage);

					if (currentGroup.CheckCombineFinish ()) {
						mUdpPeer.AddLogicHandleQueue (mReceiveGroupList.Dequeue ());
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
						DebugSystem.LogError ("Client 发送超时");
						break;
					}

					DebugSystem.LogError ("Client ReSend Package: " + iter1.Current.Key);
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
						break;
					}

					DebugSystem.LogError ("Client ReSend SureReceive Package: " + iter2.Current.Key);
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