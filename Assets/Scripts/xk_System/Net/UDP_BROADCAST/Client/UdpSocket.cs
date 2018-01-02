using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using xk_System.Debug;
using System.Text;
using System;
using System.Threading;

namespace xk_System.Net.UDP.BROADCAST.Client
{
	public class SocketUdp_Basic : SocketReceivePeer
	{
		private EndPoint remoteSendBroadCastEndPoint = null;
		private Socket mSendBroadCastSocket = null;

		private string ip = null;
		private UInt16 port = 0;

		public void InitNet (string ip, UInt16 ServerPort)
		{
			this.port = ServerPort;
			this.ip = ip;

			mSendBroadCastSocket = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			IPEndPoint iep = new IPEndPoint (IPAddress.Broadcast, port);
			remoteSendBroadCastEndPoint = (EndPoint)iep;

			mSendBroadCastSocket.EnableBroadcast = true;
		}

		public void SendNetStream (byte[] msg,int offset, int count)
		{
			mSendBroadCastSocket.SendTo (msg, offset, count, SocketFlags.None, remoteSendBroadCastEndPoint);
		}
			
		public void CloseNet ()
		{
			mSendBroadCastSocket.Close ();
		}
	}

}









