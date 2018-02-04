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
	public class SocketUdp_Server_Basic:Singleton<SocketUdp_Server_Basic>
	{
		private string ip;
		private UInt16 port;

		protected NETSTATE m_state;
		protected ConcurrentQueue<peer_event> mPeerEventQueue = new ConcurrentQueue<peer_event> ();
		private Socket mSocket = null;

		private Thread mSendThread = null;
		private Thread mReceiveThread = null;
		private Thread mHandleReceiveDataThread = null;

		private bool bClosed = false;

		private ConcurrentQueue<NetEndPointPackage> mSendPackageQueue = null;
		private ConcurrentQueue<NetEndPointPackage> mReceivePackageQueue = null;

		public void InitNet (string ip, UInt16 ServerPort)
		{
			bClosed = false;
			this.port = ServerPort;
			this.ip = ip;
			m_state = NETSTATE.DISCONNECTED;

			mSocket = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			mSocket.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			EndPoint bindEndPoint = new IPEndPoint (IPAddress.Parse (ip), port);
			mSocket.Bind (bindEndPoint);

			//mEndPointPool = new SafeObjectPool<IPEndPoint> (100);
			mSendPackageQueue = new ConcurrentQueue<NetEndPointPackage> ();
			mReceivePackageQueue = new ConcurrentQueue<NetEndPointPackage> ();

			mHandleReceiveDataThread = new Thread (HandleReceiveData);
			mHandleReceiveDataThread.IsBackground = false;
			mHandleReceiveDataThread.Start ();

			mReceiveThread = new Thread (ReceiveThreadUpdate);
			mReceiveThread.IsBackground = false;
			mReceiveThread.Start ();

			mSendThread = new Thread (SendThreadUpdate);
			mSendThread.IsBackground = false;
			mSendThread.Start ();

			mSocket.ReceiveBufferSize = 1024 * 1024 * 2;
			DebugSystem.Log ("Server ReceiveBufferSize: " + mSocket.ReceiveBufferSize);
			DebugSystem.Log ("Server SendBufferSize: " + mSocket.SendBufferSize);
		}

		private void ReceiveThreadUpdate()
		{
			while (true) {
				int length = 0;
				try {
					EndPoint tempEndPoint = new IPEndPoint (IPAddress.Broadcast, 0);
					NetUdpFixedSizePackage mPackage = ObjectPoolManager.Instance.mUdpFixedSizePackagePool.Pop ();
					length = mSocket.ReceiveFrom (mPackage.buffer, ref tempEndPoint);
					mPackage.Length = length;

					if (length > 0) {
						NetEndPointPackage mEndPointPackage = ObjectPoolManager.Instance.mNetEndPointPackagePool.Pop ();
						mEndPointPackage.mPackage = mPackage;
						mEndPointPackage.mRemoteEndPoint = tempEndPoint;
						mReceivePackageQueue.Enqueue (mEndPointPackage);
					} else {
						ObjectPoolManager.Instance.mUdpFixedSizePackagePool.recycle (mPackage);

						DebugSystem.LogError ("接受长度： " + length);
						break;
					}
				} catch (SocketException e) {
					if (e.SocketErrorCode == SocketError.Interrupted) {
						DebugSystem.LogWarning ("阻塞Socket 调用已被取消");
					} else {
						DebugSystem.LogWarning ("SocketException: " + e.SocketErrorCode);
					}
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

			DebugSystem.LogWarning ("Server ReceiveThread Safe Quit !");
		}

		private void HandleReceiveData()
		{
			while (true) {
				int nHandlePackageCount = 0;
				while (!mReceivePackageQueue.IsEmpty) {
					NetEndPointPackage mEndPointPackage = null;
					if (!mReceivePackageQueue.TryDequeue (out mEndPointPackage)) {
						break;
					}

					IPEndPoint point = mEndPointPackage.mRemoteEndPoint as IPEndPoint;
					UInt16 tempPort = (UInt16)point.Port;

					ClientPeer mPeer = null;
					if (!ClientPeerManager.Instance.IsExist (tempPort)) {
						mPeer = ObjectPoolManager.Instance.mClientPeerPool.Pop ();
						mPeer.AcceptClient (mSocket, point);
						ClientPeerManager.Instance.AddClient (mPeer);

						DebugSystem.Log ("增加 客户端信息： " + tempPort);
					}

					mPeer = ClientPeerManager.Instance.FindClient (tempPort);
					mPeer.ReceiveUdpSocketFixedPackage (mEndPointPackage.mPackage as NetUdpFixedSizePackage);

					nHandlePackageCount++;
					if (nHandlePackageCount > 50) {
						nHandlePackageCount = 0;
						Thread.Sleep (1);
					}
				}

				if (bClosed) {
					break;
				}

				Thread.Sleep (50);
			}

			DebugSystem.LogWarning ("Server HandleReceiveLogicThread Safe Quit !");
		}

		private void SendThreadUpdate()
		{
			while (!bClosed) {
				int nPackageCount = 0;
				while (!mSendPackageQueue.IsEmpty) {
					NetEndPointPackage mNetPackage = null;
					if (!mSendPackageQueue.TryDequeue (out mNetPackage)) {
						break;
					}

					this.SendNetStream (mNetPackage);

					nPackageCount++;
					if (nPackageCount > 50) {
						nPackageCount = 0;
						Thread.Sleep (1);
					}
				}

				Thread.Sleep (50);
			}

			DebugSystem.LogWarning ("Server SendThread Safe Quit !");
		}

		public void SendNetPackage(NetEndPointPackage mNetPackage)
		{
			mSendPackageQueue.Enqueue (mNetPackage);
		}

		private void SendNetStream (NetEndPointPackage mEndPointPacakge)
		{
			EndPoint remoteEndPoint = mEndPointPacakge.mRemoteEndPoint;
			NetPackage mPackage = mEndPointPacakge.mPackage;

			DebugSystem.Assert (mPackage.Length >= ServerConfig.nUdpPackageFixedHeadSize, "发送长度要大于等于 包头： " + mPackage.Length);
			int nSendLength = mSocket.SendTo (mPackage.buffer, 0, mPackage.Length, SocketFlags.None, remoteEndPoint);
			DebugSystem.Assert (nSendLength > 0, "Server 发送失败： " + nSendLength);

			ObjectPoolManager.Instance.mNetEndPointPackagePool.recycle (mEndPointPacakge);
		}

		public void CloseNet ()
		{
			bClosed = true;
			mSocket.Shutdown (SocketShutdown.Send);
			mSocket.Close ();
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









