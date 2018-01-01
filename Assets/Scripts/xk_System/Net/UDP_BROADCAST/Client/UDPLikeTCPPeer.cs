using System.Collections;
using System.Collections.Generic;
using xk_System.Net.UDP.BROADCAST.Protocol;

namespace xk_System.Net.UDP.BROADCAST.Client
{
	public class UDPLikeTCPPeer : SocketPeer
	{
		public UDPLikeTCPPeer()
		{
			addNetListenFun (UdpNetCommand.COMMAND_SCBROADCASTIP, ReceiveServerBroadcastIP);
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
	}
}