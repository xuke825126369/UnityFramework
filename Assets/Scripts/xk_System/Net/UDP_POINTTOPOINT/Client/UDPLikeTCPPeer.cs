using System.Collections;
using System.Collections.Generic;
using xk_System.Net.UDP.POINTTOPOINT.Protocol;
using UdpPointtopointProtocols;

namespace xk_System.Net.UDP.POINTTOPOINT.Client
{
	public class UDPLikeTCPPeer : SocketSendPeer
	{
		private double fHeartBeatTime = 0.0;

		public UDPLikeTCPPeer()
		{
			addNetListenFun (UdpNetCommand.COMMAND_HEARTBEAT, ReceiveServerHeartBeat);
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

		private void SendHeartBeat()
		{
			HeartBeat sendMsg = new HeartBeat ();
			SendNetData (UdpNetCommand.COMMAND_HEARTBEAT, sendMsg);
		}

		private void ReceiveServerHeartBeat(NetPackage mPackage)
		{
			HeartBeat msg = Protocol3Utility.getData<HeartBeat> (mPackage);
		}
	}
}