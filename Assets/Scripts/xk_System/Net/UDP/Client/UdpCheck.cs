using System.Collections;
using System.Collections.Generic;
using System;
using xk_System.Net.UDP.Protocol;

namespace xk_System.Net.UDP.Client
{
	public class UdpCheck
	{
		private Dictionary<UInt32, ArraySegment<byte>> mWaitCheckQueue = null;
		private Dictionary<UInt32, UdpProtocols.PackageCheckResult> mCheckResultDic = new Dictionary<uint, UdpProtocols.PackageCheckResult>();
		private SocketPeer mUdpPeer;

		public UdpCheck(SocketPeer mUdpPeer)
		{
			mWaitCheckQueue = new Dictionary<uint, ArraySegment<byte>> ();
			this.mUdpPeer = mUdpPeer;
			mUdpPeer.addNetListenFun (UdpNetCommand.COMMAND_PACKAGECHECK, ReceiveCheckPackage);
		}

		public void AddSendCheck(UInt32 nOrderId, byte[] sendBuff)
		{
			mWaitCheckQueue [nOrderId] = sendBuff;

			System.Timers.Timer tm = new System.Timers.Timer ();
			tm.Interval = 3.0;
			tm.AutoReset = false;
			tm.Elapsed += (object sender, System.Timers.ElapsedEventArgs args) => {
				if (mWaitCheckQueue.ContainsKey (nOrderId)) {
					byte[] buff = mWaitCheckQueue [nOrderId];
					this.mUdpPeer.SendNetStream (buff, 0, buff.Length);
				}
			};
		}

		private void ReceiveCheckPackage(NetReceivePackage mPackage)
		{
			UdpProtocols.PackageCheckResult mPackageCheckResult = Protocol3Utility.getData<UdpProtocols.PackageCheckResult> (mPackage.buffer, 0, mPackage.buffer.Length);
			this.mUdpPeer.SendNetData (mPackage.nUniqueId, mPackageCheckResult);
			if (mWaitCheckQueue.ContainsKey (mPackageCheckResult.NOrderId)) {
				mWaitCheckQueue.Remove (mPackageCheckResult.NOrderId);
			}
		}


		private List<CombinePackage> mReceiveGroupList = new List<CombinePackage> ();

		public class CombinePackage
		{
			public int groupId;
			public int nGroupCount;

			public int nPackageId;
			public Dictionary<int, NetReceivePackage> mReceivePackageDic;

			public bool CheckCombinFinish()
			{
				return mReceivePackageDic.Count == nGroupCount;
			}

			public NetReceivePackage GetCombinePackage()
			{
				byte[] buffer = new byte[nGroupCount * ClientConfig.nMaxBufferSize];
				int Length = 0;
				for (int i = groupId; i < groupId + nGroupCount; i++) {
					ArraySegment<byte> tempBuf = mReceivePackageDic [i].buffer;
					Array.Copy (tempBuf, 0, buffer, i * ClientConfig.nMaxBufferSize, ClientConfig.nMaxBufferSize);
					Length += tempBuf.Count;
				}

				NetReceivePackage mNetReceivePackage = new NetReceivePackage ();
				mNetReceivePackage.buffer = new ArraySegment<byte> (buffer, 0, Length);
				mNetReceivePackage.nOrderId = groupId;
				mNetReceivePackage.nPackageId = nPackageId;

				return mNetReceivePackage;
			}
		}

		public bool AddReceiveCheck(out NetReceivePackage mPackage)
		{
			if (mPackage.nGroupCount > 1) {
				int groupId = mPackage.nOrderId;

				CombinePackage cc = new CombinePackage ();
				cc.groupId = groupId;
				cc.nGroupCount = mPackage.nGroupCount;
				cc.mReceivePackageDic = new Dictionary<int, NetReceivePackage> ();
				cc.mReceivePackageDic [mPackage.nOrderId] = mPackage;
				mReceiveGroupList.Add (cc);
				return false;
			}

			bool orInGroup = false;
			for (int i = 0; i < mReceiveGroupList; i++) 
			{
				var currentGroup = mReceiveGroupList [i];
				if (currentGroup.groupId + currentGroup.nGroupCount > mPackage.nOrderId &&
				    currentGroup.groupId < mPackage.nOrderId) {

					currentGroup.mReceivePackageDic [mPackage.nOrderId] = mPackage;
				
					if (currentGroup.CheckCombinFinish ()) {
						mPackage = currentGroup.GetCombinePackage ();
						return true;
					} else {
						return false;
					}
				}
			}
		}

		public void Update()
		{
			
		}
	}
}