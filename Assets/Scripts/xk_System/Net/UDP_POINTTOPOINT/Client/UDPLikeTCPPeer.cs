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
			addNetListenFun (UdpNetCommand.COMMAND_SCBROADCASTIP, ReceiveServerBroadcastIP);
		}

		public override void Update (double elapsed)
		{
			fHeartBeatTime += elapsed;
			if (fHeartBeatTime >= 3) {
				SendHeartBeat ();
				fHeartBeatTime = 0.0;
			}
		}

		private void ReceiveServerBroadcastIP (NetReceivePackage mPackage)
		{
			//if (!bHaveServerIp) {
			//	UdpProtocols.scBroadcastIP msg = Protocol3Utility.getData<UdpProtocols.scBroadcastIP> (mPackage.buffer.Array, mPackage.buffer.Offset, mPackage.buffer.Count);
			//	ip = msg.Ip;
			//	InitNetServerIP ();
			//
			//	SendHeartBeat ();
			//}
		}

		private void SendHeartBeat()
		{
			HeartBeat sendMsg = new HeartBeat ();
			SendNetData (UdpNetCommand.COMMAND_HEARTBEAT, sendMsg);
		}

		private void ReceiveServerHeartBeat(NetReceivePackage mPackage)
		{
			HeartBeat msg = Protocol3Utility.getData<HeartBeat> (mPackage.buffer.Array, mPackage.buffer.Offset, mPackage.buffer.Count);

		}
	}
}