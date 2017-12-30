using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using xk_System.Debug;
using System.Text;
using System;
using System.Threading;

namespace xk_System.Net.UDP.Client
{
	public abstract class SocketUdp_Basic
	{
		protected NetState m_state;
		protected EndPoint ep;
		protected Socket mSocket = null;
		private Thread mThread = null;

		protected string ip;
		private UInt16 port;
		protected bool bHaveServerIp =false;

		public void InitNet (UInt16 ServerPort)
		{
			port = ServerPort;
			bHaveServerIp = false;
			connectServer ();

			mThread = new Thread (HandData);
			mThread.Start ();
		}

		byte[] data = new byte[ClientConfig.nMaxBufferSize];

		private void HandData()
		{
			while (true) {
				int length = 0;
				try {
					length = mSocket.ReceiveFrom (data, ref ep);
					if (length > 0) {
						ReceiveSocketStream (data, 0, data.Length);
					}
				} catch (Exception e) {
					DebugSystem.Log (e.Message);
					this.CloseNet ();
					break;
				}
			}
		}

		public abstract void ReceiveSocketStream (byte[] stream, int offset, int length);

		public void SendNetStream (byte[] msg,int offset,int Length)
		{
			mSocket.SendTo (msg, offset, Length, SocketFlags.None, ep);
		}

		protected void connectServer()
		{
			if (m_state == NetState.connected) {
				return;
			}

			mSocket = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			IPEndPoint iep = new IPEndPoint (IPAddress.Parse (ip), port);
			ep = (EndPoint)iep;

			DebugSystem.Log ("当前连接的服务器地址： " + ep.ToString ());
			SendNetStream ();
		}

		protected void reConnectServer()
		{
			if (m_state == NetState.connected_success) {
				return;
			}

			this.CloseNet ();
			connectServer ();
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