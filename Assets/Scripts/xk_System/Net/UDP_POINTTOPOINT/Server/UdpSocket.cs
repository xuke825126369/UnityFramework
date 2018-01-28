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
		protected NETSTATE m_state;
		protected Queue<peer_event> mPeerEventQueue = new Queue<peer_event> ();

		public void AcceptClient (Socket mSocket, EndPoint remoteEndPoint)
		{
			this.m_state = NETSTATE.DISCONNECTED;
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

		public void SendNetPackage(NetPackage mNetPackage)
		{
			NetEndPointPackage mPackage = ObjectPoolManager.Instance.mNetEndPointPackagePool.Pop ();
			mPackage.mRemoteEndPoint = remoteEndPoint;
			mPackage.mPackage = mNetPackage;

			SocketUdp_Server_Basic.Instance.SendNetPackage (mPackage);
		}

	}

}









