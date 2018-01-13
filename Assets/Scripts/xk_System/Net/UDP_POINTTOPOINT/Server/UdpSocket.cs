using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using xk_System.Debug;
using System.Text;
using System;
using System.Threading;

namespace xk_System.Net.UDP.POINTTOPOINT.Server
{
	public class SocketUdp_Basic:SocketReceivePeer
	{
		private EndPoint remoteEndPoint = null;
		private Socket mSocket = null;
		private Thread mThread = null;

		protected NETSTATE m_state;
		protected Queue<peer_event> mPeerEventQueue = new Queue<peer_event> ();

		public void ConnectClient (Socket mSocket, EndPoint remoteEndPoint)
		{
			this.m_state = NETSTATE.DISCONNECTED;
			this.mSocket = mSocket;
			this.remoteEndPoint = remoteEndPoint;
		}

		public UInt16 getPort()
		{
			IPEndPoint point = remoteEndPoint as IPEndPoint;
			if (point == null) {
				DebugSystem.LogError ("IPEndPoint is Null");
			}
			return (UInt16)(point.Port);
		}

		public void SendNetStream (byte[] msg,int offset,int Length)
		{
			mSocket.SendTo (msg, offset, Length, SocketFlags.None, remoteEndPoint);
		}

	}
}









