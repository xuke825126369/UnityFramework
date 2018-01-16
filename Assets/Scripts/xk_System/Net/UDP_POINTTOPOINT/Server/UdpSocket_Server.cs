using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using xk_System.Debug;
using System.Text;
using System;
using System.Threading;
using System.Collections.Concurrent;

namespace xk_System.Net.UDP.POINTTOPOINT.Server
{
	public class SocketUdp_Server_Basic
	{
		private string ip;
		private UInt16 port;

		private ConcurrentBag<Socket> mSocketPool = null;

		protected NETSTATE m_state;
		protected Queue<peer_event> mPeerEventQueue = new Queue<peer_event> ();

		private bool bClosed = false;
		private int nMaxThreadCount = 16;

		public SocketUdp_Server_Basic()
		{
			mSocketPool = new ConcurrentBag<Socket> ();
		}

		public void InitNet (string ip, UInt16 ServerPort)
		{
			bClosed = false;
			this.port = ServerPort;
			this.ip = ip;
			m_state = NETSTATE.DISCONNECTED;

			for (int i = 0; i < 16; i++) {
				ThreadPool.QueueUserWorkItem (new WaitCallback (WorkItem));
			}
		}

		private void WorkItem(object state)
		{
			Socket mSocket = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			mSocket.ExclusiveAddressUse = false;

			EndPoint bindEndPoint = new IPEndPoint (IPAddress.Parse (ip), port);
			mSocket.Bind (bindEndPoint);

			mSocketPool.Add (mSocket);
			HandData (mSocket);
		}

		private void HandData(Socket mSocket)
		{
			while (!bClosed) {
				int length = 0;
				try {
					EndPoint remoteEndPoint = new IPEndPoint (IPAddress.Any, 0);
					NetUdpFixedSizePackage mPackage = ObjectPoolManager.Instance.mUdpFixedSizePackagePool.Pop ();
					length = mSocket.ReceiveFrom (mPackage.buffer, ref remoteEndPoint);
					mPackage.Length = length;

					if (length > 0) {
						IPEndPoint point = remoteEndPoint as IPEndPoint;
						UInt16 tempPort = (UInt16)point.Port;

						ClientPeer mPeer = null;
						if (!ClientPeerManager.Instance.IsExist (tempPort)) {
							mPeer = ObjectPoolManager.Instance.mClientPeerPool.Pop ();
							mPeer.ConnectClient (mSocket, point);
							ClientPeerManager.Instance.AddClient (mPeer);

							DebugSystem.Log ("增加 客户端信息： " + tempPort);
						}

						mPeer = ClientPeerManager.Instance.FindClient (tempPort);
						mPeer.ReceiveUdpSocketFixedPackage (mPackage);
					} else {
						ObjectPoolManager.Instance.mUdpFixedSizePackagePool.recycle (mPackage);
					}
				} catch (SocketException e) {
					DebugSystem.LogError ("SocketException: " + e.SocketErrorCode);
					break;
				} catch (Exception e) {
					DebugSystem.LogError ("服务器 异常： " + e.Message + " | " + e.StackTrace);

					m_state = NETSTATE.DISCONNECTED;
					peer_event mEvent = new peer_event ();
					mEvent.mNetEventType = NETEVENT.DISCONNECTED;
					mEvent.msg = e.Message;

					break;
				}

			}
		}

		public void CloseNet ()
		{
			bClosed = true;
			while (!mSocketPool.IsEmpty) {
				Socket mSocket = null;
				if (mSocketPool.TryTake (out mSocket)) {
					mSocket.Close ();
				} else {
					break;
				}
			}
		}
	}

	public class SocketUdp_Server_Poll : SocketReceivePeer
	{
		private Socket mSocket = null;
		byte[] mReceiveStream = null;

		public SocketUdp_Server_Poll()
		{
			mReceiveStream = new byte[ServerConfig.nUdpPackageFixedSize];
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

	public class SocketUdp_Server_SocketAsyncEventArgs:SocketReceivePeer
	{
		private SocketAsyncEventArgs ReceiveArgs;
		private Socket mSocket = null;

		public SocketUdp_Server_SocketAsyncEventArgs()
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
			ReceiveArgs.SetBuffer (new byte[ServerConfig.nUdpPackageFixedSize], 0, ServerConfig.nUdpPackageFixedSize);
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









