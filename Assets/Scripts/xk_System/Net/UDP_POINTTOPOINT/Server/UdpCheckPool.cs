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
		private const float nReSendTime = 1000.0f;
		private const bool bClient = false;

		private SafeObjectPool<CheckPackageInfo> mCheckPackagePool = null;
		private ConcurrentDictionary<UInt16, CheckPackageInfo> mWaitCheckSendDic = null;
		private ConcurrentDictionary<UInt16, CheckPackageInfo> mWaitCheckReceiveDic = null;
		private ClientPeer mUdpPeer = null;

		private UInt16 nCurrentWaitReceiveOrderId;
		private ConcurrentDictionary<UInt16, NetUdpFixedSizePackage> mReceiveLossPackageDic = null;

		private ConcurrentQueue<NetCombinePackage> mCombinePackageQueue = null;

		public UdpCheckPool (ClientPeer mUdpPeer)
		{
			mCheckPackagePool = new SafeObjectPool<CheckPackageInfo> ();
			mWaitCheckSendDic = new ConcurrentDictionary<ushort, CheckPackageInfo> ();
			mWaitCheckReceiveDic = new ConcurrentDictionary<ushort, CheckPackageInfo> ();

			nCurrentWaitReceiveOrderId = ServerConfig.nUdpMinOrderId;
			mCombinePackageQueue = new ConcurrentQueue<NetCombinePackage> ();
			mReceiveLossPackageDic = new ConcurrentDictionary<ushort, NetUdpFixedSizePackage> ();

			this.mUdpPeer = mUdpPeer;
		}

		private void AddPackageOrderId()
		{
			if (nCurrentWaitReceiveOrderId == ServerConfig.nUdpMaxOrderId) {
				nCurrentWaitReceiveOrderId = ServerConfig.nUdpMinOrderId;
			} else {
				nCurrentWaitReceiveOrderId++;
			}
		}

		public void ReceiveCheckPackage (NetPackage mPackage)
		{
			PackageCheckResult mPackageCheckResult = Protocol3Utility.getData<PackageCheckResult> (mPackage);
			UInt16 whoId = (UInt16)(mPackageCheckResult.NWhoOrderId >> 16);
			UInt16 nOrderId = (UInt16)(mPackageCheckResult.NWhoOrderId & 0x0000FFFF);

			//DebugSystem.Log ("Server: nWhoId: " + whoId + " | nOrderId: " + nOrderId);
			bool bSender = bClient ? whoId == 1 : whoId == 2;
			if (bSender) {
				this.mUdpPeer.SendNetStream (mPackage);

				CheckPackageInfo mRemovePackage = null;
				if (mWaitCheckSendDic.TryRemove (nOrderId, out mRemovePackage)) {
					mUdpPeer.RecycleNetUdpFixedPackage (mRemovePackage.mPackage);
					mRemovePackage.mPackage = null;

					mCheckPackagePool.recycle (mRemovePackage);
				} else {
					DebugSystem.LogError ("已经确认的Send OrderId: " + nOrderId);
				}
			} else {
				CheckPackageInfo mRemovePackage = null;
				if (mWaitCheckReceiveDic.TryRemove (nOrderId, out mRemovePackage)) {
					mUdpPeer.RecycleNetUdpFixedPackage (mRemovePackage.mPackage);
					mRemovePackage.mPackage = null;

					mCheckPackagePool.recycle (mRemovePackage);
				} else {
					DebugSystem.LogError ("已经确认的Receive OrderId: " + nOrderId);
				}
			}
		}

		public void Update (double elapsed)
		{
			lock (mWaitCheckSendDic) {
				var iter1 = mWaitCheckSendDic.GetEnumerator ();
				while (iter1.MoveNext ()) {
					CheckPackageInfo mCheckInfo = iter1.Current.Value;
					if (mCheckInfo.mTimer.elapsed () > nReSendTime) {
						mCheckInfo.nReSendCount++;
						if (mCheckInfo.nReSendCount > nMaxReSendCount) {
							DebugSystem.LogError ("Server 发送超时");
							break;
						}

						this.mUdpPeer.SendNetStream (mCheckInfo.mPackage);
						mCheckInfo.mTimer.restart ();
					}
				}
			}

			lock (mWaitCheckReceiveDic) {
				var iter2 = mWaitCheckReceiveDic.GetEnumerator ();
				while (iter2.MoveNext ()) {
					CheckPackageInfo mCheckInfo = iter2.Current.Value;
					if (mCheckInfo.mTimer.elapsed () > nReSendTime) {
						mCheckInfo.nReSendCount++;
						if (mCheckInfo.nReSendCount > nMaxReSendCount) {
							DebugSystem.LogError ("Server 发送超时");
							break;
						}

						this.mUdpPeer.SendNetStream (mCheckInfo.mPackage);
						mCheckInfo.mTimer.restart ();
					}
				}
			}
		}

		public void AddSendCheck (NetUdpFixedSizePackage mPackage)
		{
			if (ServerConfig.bNeedCheckPackage) {
				UInt16 nOrderId = mPackage.nOrderId;
			
				CheckPackageInfo mCheckInfo = mCheckPackagePool.Pop ();
				mCheckInfo.nReSendCount = 0;
				mCheckInfo.mPackage = mPackage;
				mCheckInfo.mTimer.restart ();

				if (!mWaitCheckSendDic.TryAdd (nOrderId, mCheckInfo)) {
					DebugSystem.LogError ("Error ");
				}
			}

			mUdpPeer.SendNetStream (mPackage);
		}

		public void AddReceiveCheck (NetUdpFixedSizePackage mReceiveLogicPackage)
		{
			if (ServerConfig.bNeedCheckPackage) {
				PackageCheckResult mResult = new PackageCheckResult ();
				if (bClient) {
					mResult.NWhoOrderId = (UInt32)(2 << 16 | mReceiveLogicPackage.nOrderId);
				} else {
					mResult.NWhoOrderId = (UInt32)(1 << 16 | mReceiveLogicPackage.nOrderId);
				}
				NetUdpFixedSizePackage mCheckResultPackage = mUdpPeer.GetUdpSystemPackage (UdpNetCommand.COMMAND_PACKAGECHECK, mResult);

				CheckPackageInfo mCheckInfo = mCheckPackagePool.Pop ();
				mCheckInfo.nReSendCount = 0;
				mCheckInfo.mPackage = mCheckResultPackage;
				mCheckInfo.mTimer.restart ();

				if (!mWaitCheckReceiveDic.TryAdd (mReceiveLogicPackage.nOrderId, mCheckInfo)) {
					DebugSystem.LogError ("Server Add SendCheck Error ");
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

				while (!mReceiveLossPackageDic.IsEmpty) {
					NetUdpFixedSizePackage mTempPackage = null;
					if (mReceiveLossPackageDic.TryRemove (nCurrentWaitReceiveOrderId, out mTempPackage)) {
						CheckCombinePackage (mTempPackage);
						AddPackageOrderId ();
					} else {
						break;
					}
				}
			} else if (mPackage.nOrderId > nCurrentWaitReceiveOrderId) {
				if (!mReceiveLossPackageDic.TryAdd (mPackage.nOrderId, mPackage)) {
					DebugSystem.LogError ("Server Add Loss Package Error");
				}
			} else {
				DebugSystem.Log ("Server 接受 过去的 废物包： " + mPackage.nPackageId);
				mUdpPeer.RecycleNetUdpFixedPackage (mPackage);
			}
		}

		private void CheckCombinePackage (NetUdpFixedSizePackage mPackage)
		{
			if (mPackage.nGroupCount > 1) {
				NetCombinePackage cc = mUdpPeer.GetNetCombinePackage ();

				cc.nCombineGroupId = mPackage.nOrderId;
				cc.nCombinePackageId = mPackage.nPackageId;
				cc.nCombineGroupCount = mPackage.nGroupCount;

				cc.mCombinePackageQueue.Enqueue (mPackage);

				mCombinePackageQueue.Enqueue (cc);
			} else {
				if (!mCombinePackageQueue.IsEmpty) {
					NetCombinePackage currentGroup = null;
					if (mCombinePackageQueue.TryPeek (out currentGroup)) {
						currentGroup.mCombinePackageQueue.Enqueue (mPackage);

						if (currentGroup.CheckCombineFinish ()) {
							NetCombinePackage mRemoveNetCombinePackage = null;
							if (mCombinePackageQueue.TryDequeue (out mRemoveNetCombinePackage)) {
								mUdpPeer.AddLogicHandleQueue (mRemoveNetCombinePackage);
							} else {
								DebugSystem.LogError ("移除失败");
							}
						}
					
					}
				} else {
					mUdpPeer.AddLogicHandleQueue (mPackage);
				}
			}
		}

		public void release ()
		{

		}
	}
}