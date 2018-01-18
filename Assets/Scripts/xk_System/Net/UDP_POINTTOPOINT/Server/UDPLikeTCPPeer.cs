using System.Collections;
using System.Collections.Generic;
using xk_System.Net.UDP.POINTTOPOINT.Protocol;
using UdpPointtopointProtocols;

namespace xk_System.Net.UDP.POINTTOPOINT.Server
{
	public class UDPLikeTCPPeer : SocketSendPeer
	{
		private double fHeartBeatTime = 0.0;

		public UDPLikeTCPPeer()
		{
			
		}

		public override void Update (double elapsed)
		{
			base.Update (elapsed);
			fHeartBeatTime += elapsed;
			if (fHeartBeatTime >= 3) {
				SendHeartBeat ();
				fHeartBeatTime = 0.0;
			}
		}

		public void SendHeartBeat()
		{
			HeartBeat sendMsg = new HeartBeat ();
			NetUdpFixedSizePackage mPackage = GetUdpSystemPackage (UdpNetCommand.COMMAND_HEARTBEAT, sendMsg);
			SendNetStream (mPackage);
			ObjectPoolManager.Instance.mUdpFixedSizePackagePool.recycle (mPackage);
		}

		public void ReceiveUdpClientHeart(NetPackage mPackage)
		{
			HeartBeat msg = Protocol3Utility.getData<HeartBeat> (mPackage);
		}

		public void ReceiveUdpCheckPackage(NetPackage mPackage)
		{
			mUdpCheckPool.ReceiveCheckPackage (mPackage);
		}
	}
}