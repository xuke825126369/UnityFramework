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
	public class SocketUdp_Basic:SocketReceivePeer
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

		private void HandData()
		{
			while (true) {
				if (m_state == NETSTATE.CONNECTED) {
					int length = 0;
					try {
						length = mSocket.ReceiveFrom (mReceiveStream.Buffer, 0, mReceiveStream.Buffer.Length, SocketFlags.None, ref remoteEndPoint);
						if (length > 0) {
							HandleReceivePackage ();
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

	public class SocketUdp_Poll : SocketReceivePeer
	{
		private Socket mSocket = null;
		byte[] mReceiveStream = null;

		public SocketUdp_Poll()
		{
			mReceiveStream = new byte[ClientConfig.nMaxBufferSize];
		}

		public void InitNet (string ServerAddr, int ServerPort)
		{
			try {
				IPEndPoint mIPEndPoint = new IPEndPoint (IPAddress.Parse (ServerAddr), ServerPort);
				mSocket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				mSocket.Connect (mIPEndPoint);
				mSocket.Blocking = false;
				DebugSystem.Log ("Client Net InitNet Success： IP: " + ServerAddr + " | Port: " + ServerPort);
			} catch (SocketException e) {
				DebugSystem.LogError ("客户端初始化失败000： " + e.SocketErrorCode + " | " + e.Message);
			} catch (Exception e) {
				DebugSystem.LogError ("客户端初始化失败111：" + e.Message);
			}
		}

		private void Poll()
		{
			if (mSocket.Poll (0, SelectMode.SelectRead)) {
				ProcessInput ();
			}

			if (mSocket.Poll (0, SelectMode.SelectError)) {
				ProcessExcept ();
			}
		}

		private void ProcessInput()
		{
			SocketError error;

			int Length = mSocket.Receive (mReceiveStream, 0, mReceiveStream.Length, SocketFlags.None, out error);
		}

		private void ProcessExcept ()
		{
			//DebugSystem.LogError ("Client SocketExcept");
			this.mSocket.Close ();
			this.mSocket = null;
		}

		public override void Update (double elapsed)
		{
			Poll ();
			base.Update (elapsed);
		}

		public void CloseNet ()
		{
			if (mSocket != null) {
				mSocket.Close ();
				mSocket = null;
			}
		}
	}

	public class SocketUdp_SocketAsyncEventArgs:SocketReceivePeer
	{
		private SocketAsyncEventArgs ReceiveArgs;
		private Socket mSocket = null;

		public SocketUdp_SocketAsyncEventArgs()
		{

		}

		public void InitNet (string ServerAddr, int ServerPort)
		{
			try {
				mSocket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				IPAddress mIPAddress = IPAddress.Parse (ServerAddr);
				IPEndPoint mIPEndPoint = new IPEndPoint (mIPAddress, ServerPort);
				mSocket.Connect (mIPEndPoint);
				ConnectServer ();
				DebugSystem.Log ("Client Net InitNet Success： IP: " + ServerAddr + " | Port: " + ServerPort);
			} catch (SocketException e) {
				DebugSystem.LogError ("客户端初始化失败：" + e.Message + " | " + e.SocketErrorCode);
			}
		}

		private void ConnectServer ()
		{
			ReceiveArgs = new SocketAsyncEventArgs ();
			ReceiveArgs.Completed += Receive_Fun;
			ReceiveArgs.SetBuffer (new byte[ClientConfig.nMaxBufferSize], 0, ClientConfig.nMaxBufferSize);
			mSocket.ReceiveAsync (ReceiveArgs);
		}

		public void SendNetStream (byte[] msg,int offset,int Length)
		{
			SocketError mError = SocketError.SocketError;
			try {
				mSocket.Send (msg, offset, Length, SocketFlags.None, out mError);
			} catch (Exception e) {
				DebugSystem.LogError ("发送字节失败： " + e.Message + " | " + mError.ToString ());
			}
		}

		private void Receive_Fun (object sender, SocketAsyncEventArgs e)
		{
			if (e.SocketError == SocketError.Success && e.BytesTransferred > 0) {
				Array.Copy (e.Buffer, 0, mReceiveStream.Buffer, 0, e.BytesTransferred);
				mSocket.ReceiveAsync (e);
			} else {
				DebugSystem.Log ("接收数据失败： " + e.SocketError.ToString ());
				CloseNet ();
			}
		}

		public void CloseNet ()
		{
			if (mSocket != null) {
				mSocket.Close ();
				mSocket = null;
			}
		}			
	}
}









