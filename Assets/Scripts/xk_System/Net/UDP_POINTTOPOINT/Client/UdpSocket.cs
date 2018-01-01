using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using xk_System.Debug;
using System.Text;
using System;
using System.Threading;

namespace xk_System.Net.UDP.POINTTOPOINT.Client
{
	public abstract class SocketUdp_Basic
	{
		private EndPoint remoteEndPoint = null;
		private Socket mSocket = null;
		private Thread mThread = null;

		private string ip;
		private UInt16 port;

		protected UDPTYPE mUdpType;
		protected NETSTATE m_state;
		protected Queue<peer_event> mPeerEventQueue = new Queue<peer_event> ();

		public void InitNet (string ip, UInt16 ServerPort)
		{
			this.port = ServerPort;
			this.ip = ip;
			m_state = NETSTATE.DISCONNECTED;
			mUdpType = UDPTYPE.POINTTOPOINT;

			mSocket = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			IPEndPoint iep = new IPEndPoint (IPAddress.Parse (ip), port);
			remoteEndPoint = (EndPoint)iep;

			mThread = new Thread (HandData);
			mThread.Start ();
		}

		public void InitNet(IPAddress address, UInt16 port)
		{
			this.port = port;
			this.ip = ip;

			mSocket = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			IPEndPoint iep = new IPEndPoint (address, port);
			remoteEndPoint = (EndPoint)iep;

			mThread = new Thread (HandData);
			mThread.Start ();
		}

		byte[] data = new byte[ClientConfig.nMaxBufferSize];

		private void HandData()
		{
			while (true) {
				if (m_state == NETSTATE.CONNECTED) {
					int length = 0;
					try {
						length = mSocket.ReceiveFrom (data, ref remoteEndPoint);
						if (length > 0) {
							ReceiveSocketStream (data, 0, length);
						}
					} catch (Exception e) {
						this.CloseNet ();

						m_state = NETSTATE.DISCONNECTED;
						peer_event mEvent = new peer_event ();
						mEvent.mNetEventType = NETEVENT.DISCONNECTED;
						mEvent.msg = e.Message;
						break;
					}
				} else {
					Thread.Sleep (100);
				}
			}
		}

		public abstract void ReceiveSocketStream (byte[] stream, int offset, int length);

		public void SendNetStream (byte[] msg,int offset,int Length)
		{
			if (m_state == NETSTATE.CONNECTED) {
				mSocket.SendTo (msg, offset, Length, SocketFlags.None, remoteEndPoint);
			}
		}

		protected virtual void reConnectServer ()
		{

		}

		public void CloseNet ()
		{
			if (mSocket != null) {
				mSocket.Close ();
				mSocket = null;
			}
			mThread.Abort ();
		}
	}
}









