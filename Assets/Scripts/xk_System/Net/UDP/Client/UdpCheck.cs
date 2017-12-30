using System.Collections;
using System.Collections.Generic;
using System;
using xk_System.Net.UDP.Protocol;

namespace xk_System.Net.UDP.Client
{
	public class UdpCheck
	{
		private Dictionary<UInt32, byte[]> mWaitCheckQueue = null;
		private Dictionary<UInt32, UdpProtocols.PackageCheckResult> mCheckResultDic = new Dictionary<uint, UdpProtocols.PackageCheckResult>();
		private SocketPeer mUdpPeer;

		public UdpCheck(SocketPeer mUdpPeer)
		{
			mWaitCheckQueue = new Dictionary<uint, NetPackage> ();
			this.mUdpPeer = mUdpPeer;
			mUdpPeer.addNetListenFun (UdpNetCommand.COMMAND_PACKAGECHECK, ReceiveCheckPackage);
		}

		public void AddCheck(UInt32 nOrderId, byte[] sendBuff)
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

		private void ReceiveCheckPackage(NetPackage mPackage)
		{
			UdpProtocols.PackageCheckResult mPackageCheckResult = Protocol3Utility.getData<UdpProtocols.PackageCheckResult> (mPackage.buffer, 0, mPackage.buffer.Length);
			this.mUdpPeer.SendNetData (mPackage.command, mPackageCheckResult);
			if (mWaitCheckQueue.ContainsKey (mPackageCheckResult.NOrderId)) {
				mWaitCheckQueue.Remove (mPackageCheckResult.NOrderId);
			}
		}

		public void Update()
		{
			
		}
	}
}