using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using xk_System.Debug;
using System.Collections;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace xk_System.Net.TCP.Server
{
	public class SocketSystem_SocketAsyncEventArgs
	{
		int m_numConnectedSockets = 0;
		int m_totalBytesRead = 0;

		private ObjectPool<SocketAsyncEventArgs> ioContextPool;
		private List<SocketAsyncEventArgs> mUsedContextPool;
		private BufferManager mBufferManager;
		private Socket mListenSocket = null;

		internal SocketSystem_SocketAsyncEventArgs ()
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

		private void OnIOCompleted (object sender, SocketAsyncEventArgs e)
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

		private void StartAccept (SocketAsyncEventArgs acceptEventArg)
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

		private void OnAcceptCompleted (object sender, SocketAsyncEventArgs args)
		{
			this.ProcessAccept (args);
		}

		private void ProcessAccept (SocketAsyncEventArgs e)
		{
			Socket s = e.AcceptSocket;
			SocketAsyncEventArgs ioReadContext = ioContextPool.Pop ();
			SocketAsyncEventArgs ioWriteContext = ioContextPool.Pop ();

			mUsedContextPool.Add (ioReadContext);
			if (ioReadContext != null) {
				SocketAsyncEventArgs_Token mClient = new Client ();
				mClient.init (s, ioWriteContext);
				ioReadContext.UserToken = mClient;

				m_numConnectedSockets++;
				string outStr = String.Format ("客户 {0} 连入, 共有 {1} 个连接。", s.RemoteEndPoint.ToString (), this.m_numConnectedSockets);
				DebugSystem.Log (outStr);

				ClientFactory.Instance.AddClient ((Client)mClient);

				if (!s.ReceiveAsync (ioReadContext)) {
					this.ProcessReceive (ioReadContext);
				}
			} else {
				s.Send (Encoding.Default.GetBytes ("连接已经达到最大数!"));
				string outStr = String.Format ("连接已满，拒绝 {0} 的连接。", s.RemoteEndPoint);
				DebugSystem.Log (outStr);
				s.Close ();
			}

			this.StartAccept (e);
		}

		private void ProcessReceive (SocketAsyncEventArgs e)
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

		private void ProcessSend (SocketAsyncEventArgs e)
		{
			if (e.SocketError == SocketError.Success) {
				//DebugSystem.Log ("发送成功");
			} else {
				this.CloseClientSocket (e);
			}
		}

		private void CloseClientSocket (SocketAsyncEventArgs e)
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
		private Socket mListenSocket;
		private List<Socket> m_ReadFD = null;
		private List<Socket> m_WriteFD = null;
		private List<Socket> m_ExceptFD = null;
		private List<Socket> mClientSocketsPool = null;
		private BufferManager mBufferManager = null;
		int m_numConnectedSockets = 0;
		private Dictionary<Socket, Client_Select> mDicToken = null; 
		public SocketSystem_Select ()
		{
			m_ReadFD = new List<Socket> (ServerConfig.numConnections);
			m_WriteFD = new List<Socket> (ServerConfig.numConnections);
			m_ExceptFD = new List<Socket> (ServerConfig.numConnections);
			mClientSocketsPool = new List<Socket> (ServerConfig.numConnections);
			mDicToken = new Dictionary<Socket, Client_Select> (ServerConfig.numConnections);

			mBufferManager = new BufferManager (ServerConfig.nMaxBufferSize * ServerConfig.numConnections, ServerConfig.nMaxBufferSize);
		}

		public void InitNet (string ServerAddr, int ServerPort)
		{
			try {
				IPEndPoint mIPEndPoint = new IPEndPoint (IPAddress.Parse (ServerAddr), ServerPort);
				mListenSocket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				mListenSocket.Bind (mIPEndPoint);
				mListenSocket.Listen (0);

				StartAccept (null);
				DebugSystem.Log ("Client Net InitNet Success： IP: " + ServerAddr + " | Port: " + ServerPort);
			} catch (SocketException e) {
				DebugSystem.LogError (e.SocketErrorCode + " | " + e.Message);
			} catch (Exception e) {
				DebugSystem.LogError ("客户端初始化失败：" + e.Message);
			}
		}

		private void StartAccept (SocketAsyncEventArgs acceptEventArg)
		{
			if (acceptEventArg == null) {
				acceptEventArg = new SocketAsyncEventArgs ();
				acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs> (ProcessAccept);
			} else {
				acceptEventArg.AcceptSocket = null;
			}

			if (!this.mListenSocket.AcceptAsync (acceptEventArg)) {
				this.ProcessAccept (null, acceptEventArg);
			}
		}

		private void ProcessAccept (object sender, SocketAsyncEventArgs e)
		{
			Socket s = e.AcceptSocket;
			mClientSocketsPool.Add (s);

			Select_Token mClient = new Client_Select ();
			ArraySegment<byte> mSendBuffer = new ArraySegment<byte> ();
			ArraySegment<byte> mReceieBuffer = new ArraySegment<byte> ();
			mBufferManager.SetBuffer (ref mSendBuffer);
			mBufferManager.SetBuffer (ref mReceieBuffer);
			mClient.init (s, mSendBuffer, mReceieBuffer);
			mDicToken [s] = (Client_Select)mClient;

			string outStr = String.Format ("客户 {0} 连入, 共有 {1} 个连接。", s.RemoteEndPoint.ToString (), this.m_numConnectedSockets);
			DebugSystem.Log (outStr);

			ClientFactory_Select.Instance.AddClient ((Client_Select)mClient);

			this.StartAccept (e);
		}

		private bool CheckSocketState ()
		{
			if (mListenSocket == null) {
				return false;
			}

			//PrintSocketState (mSocket);
			return true;
		}

		private void Select ()
		{
			if (mClientSocketsPool.Count == 0) {
				return;
			}

			try {
				this.m_ReadFD.Clear ();
				this.m_WriteFD.Clear ();
				this.m_ExceptFD.Clear ();

				this.m_ReadFD.AddRange (this.mClientSocketsPool);
				this.m_WriteFD.AddRange (this.mClientSocketsPool);
				this.m_ExceptFD.AddRange (this.mClientSocketsPool);
				Socket.Select (this.m_ReadFD, this.m_WriteFD, this.m_ExceptFD, 0);

				for (int i = 0; i < this.mClientSocketsPool.Count; i++) {
					Socket mSocket = this.mClientSocketsPool [i];
					if (this.m_ExceptFD.Contains (mSocket)) {
						ProcessExcept (mSocket);
					}

					if (this.m_ReadFD.Contains (mSocket)) {
						ProcessInput (mSocket);
					}
				}
			} catch (SocketException e) {
				DebugSystem.LogError (e.SocketErrorCode + " | " + e.Message);
			} catch (Exception e) {
				DebugSystem.LogError (e.Message);
			}
		}

		private void ProcessInput (Socket mSocket)
		{
			mDicToken [mSocket].ProcessInput ();
		}

		private void ProcessExcept (Socket mSocket)
		{
			mSocket.Close ();
			mSocket = null;

			DebugSystem.LogError ("Socket 异常关闭");
		}

		public void HandleNetPackage ()
		{
			if (this.CheckSocketState ()) {
				this.Select ();
			}
		}

		public void CloseNet ()
		{
			if (mListenSocket != null) {
				mListenSocket.Close ();
				mListenSocket = null;
			}

			DebugSystem.Log ("关闭 Socket TCP 服务器");
		}
	}
}