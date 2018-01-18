using System.Collections;
using System.Collections.Generic;
using System;
using System.Collections.Concurrent;
using xk_System.Net.UDP.POINTTOPOINT.Protocol;
using xk_System.Debug;
using UdpPointtopointProtocols;
using System.Threading;

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

		private const int nMaxReSendCount = 3;
		private const double nReSendTime = 1000.00;
		private const bool bClient = false;

		private SafeObjectPool<CheckPackageInfo> mCheckPackagePool = null;
		private ConcurrentDictionary<UInt16, CheckPackageInfo> mWaitCheckSendDic = null;
		private ConcurrentDictionary<UInt16, CheckPackageInfo> mWaitCheckReceiveDic = null;
		private ConcurrentQueue<NetUdpFixedSizePackage> mReceiveSurePackageQueue = null;
		private ClientPeer mUdpPeer = null;

		private UInt16 nCurrentWaitReceiveOrderId;
		private UInt16 nCurrentWaitSendOrderId;

		private ConcurrentDictionary<UInt16, NetUdpFixedSizePackage> mReceiveLossPackageDic = null;
		private ConcurrentQueue<NetCombinePackage> mCombinePackageQueue = null;

		private ConcurrentQueue<NetUdpFixedSizePackage> mSendPackageQueue = null;
		private ConcurrentQueue<NetUdpFixedSizePackage> mReceivePackageQueue = null;
		private Thread mThread = null;

		public UdpCheckPool (ClientPeer mUdpPeer)
		{
			mCheckPackagePool = new SafeObjectPool<CheckPackageInfo> ();
			mWaitCheckSendDic = new ConcurrentDictionary<ushort, CheckPackageInfo> ();
			mWaitCheckReceiveDic = new ConcurrentDictionary<ushort, CheckPackageInfo> ();
			mReceiveSurePackageQueue = new ConcurrentQueue<NetUdpFixedSizePackage> ();

			nCurrentWaitReceiveOrderId = ServerConfig.nUdpMinOrderId;
			mCombinePackageQueue = new ConcurrentQueue<NetCombinePackage> ();
			mReceiveLossPackageDic = new ConcurrentDictionary<ushort, NetUdpFixedSizePackage> ();

			nCurrentWaitReceiveOrderId = ServerConfig.nUdpMinOrderId;
			nCurrentWaitSendOrderId = ServerConfig.nUdpMinOrderId;

			this.mUdpPeer = mUdpPeer;

			mSendPackageQueue = new ConcurrentQueue<NetUdpFixedSizePackage> ();
			mReceivePackageQueue = new ConcurrentQueue<NetUdpFixedSizePackage> ();

			mThread = new Thread (CheckPoolThread);
			mThread.Start ();
		}

		private void CheckPoolThread()
		{
			Timer mTimer = new Timer ();
			double elapsed = 0;
			while (true) {
				mTimer.restart ();
				int nPackageCount = 0;
				this.Update (elapsed);
				elapsed = mTimer.elapsed ();

				if (elapsed < 10) {
					Thread.Sleep (10);
				}
			}
		}

		private void Update(double elapsed)
		{
			NetUdpFixedSizePackage mPackage = null;
			while (!mSendPackageQueue.IsEmpty) {
				if (mSendPackageQueue.TryDequeue (out mPackage)) {

				}
			}

			while (!mReceivePackageQueue.IsEmpty) {
				if (mReceivePackageQueue.TryDequeue (out mPackage)) {

				}
			}

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

						DebugSystem.LogError ("Server ReSendPackage: " + iter1.Current.Key);
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

						DebugSystem.LogError ("Server ReSend SureReceive Package: " + iter2.Current.Key);
						this.mUdpPeer.SendNetStream (mCheckInfo.mPackage);
						mCheckInfo.mTimer.restart ();
					}
				}
			}
		}

		private void AddSendPackageOrderId()
		{
			if (nCurrentWaitSendOrderId == ServerConfig.nUdpMaxOrderId) {
				nCurrentWaitSendOrderId = ServerConfig.nUdpMinOrderId;
			} else {
				nCurrentWaitSendOrderId++;
			}
		}

		private void AddReceivePackageOrderId()
		{
			if (nCurrentWaitReceiveOrderId == ServerConfig.nUdpMaxOrderId) {
				nCurrentWaitReceiveOrderId = ServerConfig.nUdpMinOrderId;
			} else {
				nCurrentWaitReceiveOrderId++;
			}
		}

//------------------------------------命令---------------------------------
		public void ReceiveHeartPackage(NetPackage mPackage)
		{


		}

		public void ReceiveCheckPackage (NetPackage mPackage)
		{
			PackageCheckResult mPackageCheckResult = Protocol3Utility.getData<PackageCheckResult> (mPackage);
			UInt16 whoId = (UInt16)(mPackageCheckResult.NWhoOrderId >> 16);
			UInt16 nOrderId = (UInt16)(mPackageCheckResult.NWhoOrderId & 0x0000FFFF);

			//DebugSystem.Log ("ServerCheck: nWhoId: " + whoId + " | nOrderId: " + nOrderId);
			bool bSender = bClient ? whoId == 1 : whoId == 2;
			if (bSender) {
				this.mUdpPeer.SendNetStream (mPackage);

				CheckPackageInfo mRemovePackage = null;
				if (mWaitCheckSendDic.TryRemove (nOrderId, out mRemovePackage)) {
					ObjectPoolManager.Instance.mUdpFixedSizePackagePool.recycle (mRemovePackage.mPackage);
					mRemovePackage.mPackage = null;

					mCheckPackagePool.recycle (mRemovePackage);
				} else {
					DebugSystem.LogError ("Server 已经确认的Send OrderId: " + nOrderId);
				}
			} else {
				CheckPackageInfo mRemovePackage = null;
				if (mWaitCheckReceiveDic.TryRemove (nOrderId, out mRemovePackage)) {
					ObjectPoolManager.Instance.mUdpFixedSizePackagePool.recycle (mRemovePackage.mPackage);
					mRemovePackage.mPackage = null;

					mCheckPackagePool.recycle (mRemovePackage);
				} else {
					DebugSystem.LogError ("Server 已经确认的Receive OrderId: " + nOrderId);
				}
			}
		}

		public void AddSendCheck (NetUdpFixedSizePackage mPackage)
		{
			if (ServerConfig.bNeedCheckPackage) {
				mPackage.nOrderId = nCurrentWaitSendOrderId;

				UInt16 nOrderId = mPackage.nOrderId;
				CheckPackageInfo mCheckInfo = mCheckPackagePool.Pop ();
				mCheckInfo.nReSendCount = 0;
				mCheckInfo.mPackage = mPackage;
				mCheckInfo.mTimer.restart ();

				if (mWaitCheckSendDic.TryAdd (nOrderId, mCheckInfo)) {
					AddSendPackageOrderId ();
				} else {
					throw new Exception ("请增大循环Id 的范围，或者 减慢发包速度");
				}
			}

			mUdpPeer.SendNetStream (mPackage);
		}

		public void AddReceiveCheck (NetUdpFixedSizePackage mPackage)
		{
			if (mPackage.nPackageId > 50) {
				ReceiveLogicPackage (mPackage);
			} else {
				mReceiveSurePackageQueue.Enqueue (mPackage);
			}
		}

		private void ReceiveLogicPackage(NetUdpFixedSizePackage mPackage)
		{
			
#if Test
			DebugSystem.Log ("Server ReceiveInfo: " + mReceiveLogicPackage.nOrderId + " | " + mReceiveLogicPackage.nGroupCount + " | " + mReceiveLogicPackage.Length);
			if (nCurrentWaitReceiveOrderId != mReceiveLogicPackage.nOrderId) {
			DebugSystem.LogError ("丢包： " + nCurrentWaitReceiveOrderId);
			} else {
			AddPackageOrderId ();
			}
#endif

			if (ServerConfig.bNeedCheckPackage) {
				PackageCheckResult mResult = new PackageCheckResult ();
				if (bClient) {
					mResult.NWhoOrderId = (UInt32)(2 << 16 | mPackage.nOrderId);
				} else {
					mResult.NWhoOrderId = (UInt32)(1 << 16 | mPackage.nOrderId);
				}
				NetUdpFixedSizePackage mCheckResultPackage = mUdpPeer.GetUdpSystemPackage (UdpNetCommand.COMMAND_PACKAGECHECK, mResult);

				CheckPackageInfo mCheckInfo = mCheckPackagePool.Pop ();
				mCheckInfo.nReSendCount = 0;
				mCheckInfo.mPackage = mCheckResultPackage;
				mCheckInfo.mTimer.restart ();

				mWaitCheckReceiveDic.TryAdd (mPackage.nOrderId, mCheckInfo);
				mUdpPeer.SendNetStream (mCheckResultPackage);

				CheckReceivePackageLoss (mPackage);
			} else {
				CheckCombinePackage (mPackage);
			}
		}

		private void CheckReceivePackageLoss (NetUdpFixedSizePackage mPackage)
		{
			if (mPackage.nOrderId == nCurrentWaitReceiveOrderId) {
				CheckCombinePackage (mPackage);
				AddReceivePackageOrderId ();

				while (!mReceiveLossPackageDic.IsEmpty) {
					NetUdpFixedSizePackage mTempPackage = null;
					if (mReceiveLossPackageDic.TryRemove (nCurrentWaitReceiveOrderId, out mTempPackage)) {
						CheckCombinePackage (mTempPackage);
						AddReceivePackageOrderId ();
					} else {
						break;
					}
				}
			} else if (mPackage.nOrderId > nCurrentWaitReceiveOrderId) {
				if (mReceiveLossPackageDic.TryAdd (mPackage.nOrderId, mPackage)) {
					DebugSystem.LogError ("mPackage Loss: " + nCurrentWaitReceiveOrderId + " | " + mPackage.nOrderId);
				}

			} else {
				DebugSystem.LogError ("Server 接受 过去的 废物包： " + mPackage.nOrderId);
				ObjectPoolManager.Instance.mUdpFixedSizePackagePool.recycle (mPackage);
			}
		}

		private void CheckCombinePackage (NetUdpFixedSizePackage mPackage)
		{
			if (mPackage.nGroupCount > 1) {
				NetCombinePackage cc = ObjectPoolManager.Instance.mCombinePackagePool.Pop ();

				cc.nCombineGroupId = mPackage.nOrderId;
				cc.nCombinePackageId = mPackage.nPackageId;
				cc.nCombineGroupCount = mPackage.nGroupCount;

				cc.Add (mPackage);

				mCombinePackageQueue.Enqueue (cc);
			} else {
				if (!mCombinePackageQueue.IsEmpty) {
					NetCombinePackage currentGroup = null;
					if (mCombinePackageQueue.TryPeek (out currentGroup)) {
						currentGroup.Add (mPackage);

						if (currentGroup.CheckCombineFinish ()) {
							NetCombinePackage mRemoveNetCombinePackage = null;
							if (mCombinePackageQueue.TryDequeue (out mRemoveNetCombinePackage)) {
								mUdpPeer.AddLogicHandleQueue (mRemoveNetCombinePackage);
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
			mThread.Abort ();
		}
	}
}