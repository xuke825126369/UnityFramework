using System.Collections;
using System.Collections.Generic;
using xk_System.Net.UDP.POINTTOPOINT.Protocol;
using UdpPointtopointProtocols;

namespace xk_System.Net.UDP.POINTTOPOINT.Server
{

	public class UDPLikeTCPPeer : SocketSendPeer
	{
		private void SendHeartBeat()
		{
			HeartBeat sendMsg = new HeartBeat ();
			NetUdpFixedSizePackage mPackage = GetUdpSystemPackage (UdpNetCommand.COMMAND_HEARTBEAT, sendMsg);
			SendNetPackage (mPackage);
			//ObjectPoolManager.Instance.mUdpFixedSizePackagePool.recycle (mPackage);
		}

		public void ReceiveUdpClientHeart(NetPackage mPackage)
		{
			HeartBeat msg = Protocol3Utility.getData<HeartBeat> (mPackage);
			SendHeartBeat ();
		}

		public void ReceiveUdpCheckPackage(NetPackage mPackage)
		{
			mUdpCheckPool.ReceiveCheckPackage (mPackage);
		}

	}

}