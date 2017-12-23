using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using xk_System.Debug;
using System.Collections;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace xk_System.Net.Server
{
	public class SocketSystem_SocketAsyncEventArgs
	{
		int m_numConnectedSockets = 0;
		int m_totalBytesRead = 0;

		private ObjectPool<SocketAsyncEventArgs> ioContextPool;
		private List<SocketAsyncEventArgs> mUsedContextPool;
		BufferManager mBufferManager;

		private Socket mListenSocket = null;
		internal SocketSystem_SocketAsyncEventArgs()
		{
			mUsedContextPool = new List<SocketAsyncEventArgs> ();
			ioContextPool = new ObjectPool<SocketAsyncEventArgs> ();

			mBufferManager = new BufferManager (2 * ServerConfig.nMaxBufferSize * ServerConfig.numConnections, ServerConfig.nMaxBufferSize);
			
			for (int i = 0; i < ServerConfig.numConnections; i++) {
				SocketAsyncEventArgs ioContext = new SocketAsyncEventArgs ();
				ioContext.Completed += new EventHandler<SocketAsyncEventArgs> (OnIOCompleted);
				mBufferManager.SetBuffer (ioContext);
				ioContextPool.recycle (ioContext);
			}
		}

		public void InitNet (string ServerAddr, int ServerPort)
		{
			IPAddress serverAddr = IPAddress.Parse (ServerAddr);
			IPEndPoint localEndPoint = new IPEndPoint (serverAddr, ServerPort);

			this.mListenSocket = new Socket (localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

			this.mListenSocket.Bind (localEndPoint);
			this.mListenSocket.Listen (0);

			this.StartAccept (null);
		}

		private void OnIOCompleted(object sender, SocketAsyncEventArgs e)
		{
			switch (e.LastOperation) {
			case SocketAsyncOperation.Receive:
				this.ProcessReceive (e);
				break;
			case SocketAsyncOperation.Send:
				this.ProcessSend (e);
				break;
			default:
				throw new ArgumentException ("The last operation completed on the socket was not a receive or send");
			}
		}
			
		private void StartAccept(SocketAsyncEventArgs acceptEventArg)
		{
			if (acceptEventArg == null) {
				acceptEventArg = new SocketAsyncEventArgs ();
				acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs> (OnAcceptCompleted);
			} else {
				acceptEventArg.AcceptSocket = null;
			}
				
			if (!this.mListenSocket.AcceptAsync (acceptEventArg)) {
				this.ProcessAccept (acceptEventArg);
			}
		}

		private void OnAcceptCompleted(object sender,SocketAsyncEventArgs args)
		{
			this.ProcessAccept (args);
		}

		private void ProcessAccept(SocketAsyncEventArgs e)
		{
			Socket s = e.AcceptSocket;
			SocketAsyncEventArgs ioContext = ioContextPool.Pop ();
			mUsedContextPool.Add (ioContext);
			if (ioContext != null) {
				SocketAsyncEventArgs_Token mClient = new Client ();
				mClient.init (s, ioContextPool.Pop());
				ioContext.UserToken = mClient;

				m_numConnectedSockets++;
				string outStr = String.Format ("客户 {0} 连入, 共有 {1} 个连接。", s.RemoteEndPoint.ToString (), this.m_numConnectedSockets);
				DebugSystem.Log (outStr);

				ClientFactory.Instance.AddClient ((Client)mClient);

				if (!s.ReceiveAsync (ioContext)) {
					this.ProcessReceive (ioContext);
				}
			} else {
				s.Send (Encoding.Default.GetBytes ("连接已经达到最大数!"));
				string outStr = String.Format ("连接已满，拒绝 {0} 的连接。", s.RemoteEndPoint);
				DebugSystem.Log (outStr);
				s.Close ();
			}

			this.StartAccept (e);
		}

		private void ProcessReceive(SocketAsyncEventArgs e)
		{
			if (e.SocketError == SocketError.Success && e.BytesTransferred > 0) {
				var client = e.UserToken as Client;
				client.ReceiveSocketStream (e.Buffer, 0, e.BytesTransferred);

				Socket socket = client.getSocket ();
				if (!socket.ReceiveAsync (e)) {
					this.ProcessReceive (e);
				}
			} else {
				this.CloseClientSocket (e);
			}
		}

		private void ProcessSend(SocketAsyncEventArgs e)
		{
			if (e.SocketError == SocketError.Success && e.BytesTransferred > 0) {
				
			} else {
				this.CloseClientSocket (e);
			}
		}

		private void CloseClientSocket(SocketAsyncEventArgs e)
		{
			Interlocked.Decrement (ref this.m_numConnectedSockets);
			Client client = e.UserToken as Client;
			Socket s = client.getSocket ();

			string outStr = String.Format ("客户 {0} 断开, 共有 {1} 个连接。", s.RemoteEndPoint.ToString (), this.m_numConnectedSockets);
			DebugSystem.Log (outStr);        
			IPEndPoint localEp = s.LocalEndPoint as IPEndPoint;
			if (e.SocketError != SocketError.Success) {
				outStr = String.Format ("套接字错误 {0}, IP {1}, 操作 {2}。", e.SocketError, localEp, e.LastOperation);
				DebugSystem.LogError (outStr);
			}

			lock (mUsedContextPool) {
				mUsedContextPool.Remove (e);
			}

			lock (ioContextPool) {
				ioContextPool.recycle (e);
			}

			try {
				s.Shutdown (SocketShutdown.Send);
			} catch (SocketException  e1) {
				s.Close ();
				DebugSystem.LogError (e1.Message);
			} catch (Exception e2) {
				s.Close ();
				DebugSystem.LogError (e2.Message);
			}

			s = null;
		}

		public void CloseNet ()
		{
			while (mUsedContextPool.Count > 0) {
				var v = mUsedContextPool [0];
				CloseClientSocket (v);
			}
			ioContextPool.release ();
			mListenSocket.Close ();
			mListenSocket = null;
		}
	}

	//Select
	public class SocketSystem_Select
	{
		private Socket mSocket;
		private ArrayList m_ReadFD = null;
		private ArrayList m_WriteFD = null;
		private ArrayList m_ExceptFD = null;
		byte[] mReceiveStream = null;

		public SocketSystem_Select ()
		{
			mReceiveStream = new byte[ClientConfig.nMaxBufferSize];
			m_ReadFD = new ArrayList ();
			m_WriteFD = new ArrayList ();
			m_ExceptFD = new ArrayList ();
		}

		public void InitNet (string ServerAddr, int ServerPort)
		{
			try {
				IPEndPoint mIPEndPoint = new IPEndPoint (IPAddress.Parse (ServerAddr), ServerPort);
				mSocket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				mSocket.Connect (mIPEndPoint);
				ConfigureSocket (mSocket);
				DebugSystem.Log ("Client Net InitNet Success： IP: " + ServerAddr + " | Port: " + ServerPort);
			} catch (SocketException e) {
				DebugSystem.LogError (e.SocketErrorCode + " | " + e.Message);
			} catch (Exception e) {
				DebugSystem.LogError ("客户端初始化失败：" + e.Message);
			}
		}

		public void ConfigureSocket (Socket mSocket)
		{
			mSocket.Blocking = false;
		}

		private bool CheckSocketState()
		{
			if (mSocket == null) {
				return false;
			}

			//PrintSocketState (mSocket);
			return true;
		}

		private void Select ()
		{
			try {
				this.m_ReadFD.Clear ();
				this.m_WriteFD.Clear ();
				this.m_ExceptFD.Clear ();

				this.m_ReadFD.Add (this.mSocket);
				this.m_WriteFD.Add (this.mSocket);
				this.m_ExceptFD.Add (this.mSocket);
				Socket.Select (this.m_ReadFD, this.m_WriteFD, this.m_ExceptFD, 0);

				if (this.m_ExceptFD.Contains (this.mSocket)) {
					ProcessExcept ();
				}

				if (this.m_WriteFD.Contains (this.mSocket)) {
					ProcessOutput ();
				}

				if (this.m_ReadFD.Contains (this.mSocket)) {
					ProcessInput ();
				}
			} catch (SocketException e) {
				DebugSystem.LogError (e.SocketErrorCode + " | " + e.Message);
			} catch (Exception e) {
				DebugSystem.LogError (e.Message);
			}
		}

		private void ProcessOutput ()
		{
			//DebugSystem.Log ("Client Can Write ...");
		}

		private void ProcessInput ()
		{
			//DebugSystem.Log ("Client Can Read ...");
			SocketError error;
			int Length = mSocket.Receive (mReceiveStream, 0, mReceiveStream.Length, SocketFlags.None, out error);
			if (error == SocketError.Success) {
				//mNetReceiveSystem.ReceiveSocketStream (mReceiveStream, 0, Length);
				if (mSocket.Available > 0) {
					DebugSystem.LogError ("Available > 0： " + mSocket.Available +" | "+ Length + " | " + mReceiveStream.Length);
					//ProcessInput ();
				}
			} else {
				DebugSystem.LogError (error.ToString ());
			}
		}

		private void ProcessExcept ()
		{
			//DebugSystem.LogError ("Client SocketExcept");
			this.mSocket.Close ();
			this.mSocket = null;
		}


		public void HandleNetPackage ()
		{
			if (this.CheckSocketState ()) {
				this.Select ();
			}
		}

		public void SendNetStream (int clientId,byte[] msg, int index, int Length)
		{
			try {
				SocketError merror;
				int sendLength = mSocket.Send (msg, index, Length, SocketFlags.None, out merror);
				if (sendLength != Length) {
					DebugSystem.LogError ("Client:SendLength:  " + sendLength + " | " + Length);
				}
				if (merror != SocketError.Success) {
					if (mSocket.Blocking == false && merror == SocketError.WouldBlock) {
						SendNetStream (clientId, msg, index, Length);
					} else {
						DebugSystem.LogError ("发送失败: " + merror);
					}
				}
			} catch (SocketException e) {
				DebugSystem.LogError (e.SocketErrorCode + " | " + e.Message);
			} catch (Exception e) {
				DebugSystem.LogError (e.Message);
			}
		}

		public void CloseNet ()
		{
			if (mSocket != null) {
				mSocket.Close ();
				mSocket = null;
			}

			DebugSystem.Log ("关闭 Socket TCP 客户端");
		}
	}
}