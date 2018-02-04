using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using xk_System.Debug;
using System.Text;
using System;
using System.Threading;
using System.Collections.Concurrent;

namespace xk_System.Net.UDP.POINTTOPOINT.Client
{
	public class SocketUdp_Basic : SocketReceivePeer
	{
		private EndPoint remoteEndPoint = null;

		private Socket mSocket = null;
		private Thread mReceiveThread = null;
		private Thread mSendThread = null;

		private string ip;
		private UInt16 port;

		protected UDPTYPE mUdpType;
		protected NETSTATE m_state;
		protected Queue<peer_event> mPeerEventQueue = new Queue<peer_event> ();

		private ConcurrentQueue<NetPackage> mSendPackageQueue = null;
		private bool bClosed = false;
		public void InitNet (string ip, UInt16 ServerPort)
		{
			bClosed = false;
			this.port = ServerPort;
			this.ip = ip;
			m_state = NETSTATE.DISCONNECTED;
			mUdpType = UDPTYPE.POINTTOPOINT;

			mSocket = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			remoteEndPoint = new IPEndPoint (IPAddress.Parse (ip), port);

			mSocket.Connect (remoteEndPoint);

			mReceiveThread = new Thread (ReceiveThreadUpdate);
			mReceiveThread.IsBackground = true;
			mReceiveThread.Start ();

			mSendPackageQueue = new ConcurrentQueue<NetPackage> ();
			mSendThread = new Thread (SendThreadUpdate);
			mSendThread.IsBackground = true;
			mSendThread.Start ();
		}
		
		private void ReceiveThreadUpdate()
		{
			while (!bClosed) {
				int length = 0;
				try {
					NetUdpFixedSizePackage mReceiveStream = ObjectPoolManager.Instance.mUdpFixedSizePackagePool.Pop ();
					length = mSocket.ReceiveFrom (mReceiveStream.buffer, 0, mReceiveStream.buffer.Length, SocketFlags.None, ref remoteEndPoint);
					if (length > 0) {
						//DebugSystem.Log("Client ReceiveLength: " + length);
						mReceiveStream.Length = length;
						ReceiveNetPackage (mReceiveStream);
					} else {
						ObjectPoolManager.Instance.mUdpFixedSizePackagePool.recycle (mReceiveStream);
					}
				} catch (SocketException e) {
					if (e.SocketErrorCode == SocketError.Shutdown) {
						DebugSystem.LogWarning ("Socket 接受操作被禁止");
					} else {
						DebugSystem.LogWarning ("ScoketException: " + e.SocketErrorCode);
					}
					break;
				} catch (Exception e) {
					DebugSystem.LogError (e.Message);

					m_state = NETSTATE.DISCONNECTED;
					peer_event mEvent = new peer_event ();
					mEvent.mNetEventType = NETEVENT.DISCONNECTED;
					mEvent.msg = e.Message;
					break;
				}
			}

			DebugSystem.LogWarning ("Client ReceiveThread Safe Quit !");
		}

		private void SendThreadUpdate()
		{
			while (true) {
				while (!mSendPackageQueue.IsEmpty) {
					NetPackage mNetPackage = null;
					if (!mSendPackageQueue.TryDequeue (out mNetPackage)) {
						break;
					}

					SendNetStream (mNetPackage.buffer, 0, mNetPackage.Length);
					Thread.Sleep (1);
				}

				if (bClosed) {
					break;
				}

				Thread.Sleep (50);
			}

			DebugSystem.LogWarning ("Client SendThread Safe Quit !");
		}

		public void SendNetPackage(NetPackage mNetPackage)
		{
			mSendPackageQueue.Enqueue (mNetPackage);
		}

		private void SendNetStream (byte[] msg, int offset, int Length)
		{
			DebugSystem.Assert (Length >= ClientConfig.nUdpPackageFixedHeadSize, "发送长度要大于等于 包头： " + Length);
			int nSendLength = mSocket.SendTo (msg, offset, Length, SocketFlags.None, remoteEndPoint);
			DebugSystem.Assert (nSendLength > 0, "Client 发送失败： " + nSendLength);
		}

		protected virtual void reConnectServer ()
		{

		}

		public void CloseNet ()
		{
			bClosed = true;
			mSocket.Shutdown (SocketShutdown.Receive);
			mSocket.Close ();
		}

	}

	public class SocketUdp_Poll : SocketReceivePeer
	{
		private Socket mSocket = null;
		byte[] mReceiveStream = null;

		public SocketUdp_Poll()
		{
			mReceiveStream = new byte[ClientConfig.nUdpPackageFixedSize];
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
			ReceiveArgs.SetBuffer (new byte[ClientConfig.nUdpPackageFixedSize], 0, ClientConfig.nUdpPackageFixedSize);
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
				//Array.Copy (e.Buffer, 0, mReceiveStream.buffer, 0, e.BytesTransferred);
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









