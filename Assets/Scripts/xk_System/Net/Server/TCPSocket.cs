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
	class BufferManager
	{
    	int m_numBytes;                 // the total number of bytes controlled by the buffer pool
    	byte[] m_buffer;                // the underlying byte array maintained by the Buffer Manager
    	Stack<int> m_freeIndexPool;     // 
    	int m_currentIndex;
    	int m_bufferSize;

    	public BufferManager(int totalBytes, int bufferSize)
		{
			m_numBytes = totalBytes;
			m_currentIndex = 0;
			m_bufferSize = bufferSize;
			m_freeIndexPool = new Stack<int> ();
		}

    	// Allocates buffer space used by the buffer pool
    	public void InitBuffer()
    	{
        	// create one big large buffer and divide that 
        	// out to each SocketAsyncEventArg object
        	m_buffer = new byte[m_numBytes];
    	}

    	// Assigns a buffer from the buffer pool to the 
    	// specified SocketAsyncEventArgs object
    	//
    	// <returns>true if the buffer was successfully set, else false</returns>
    	public bool SetBuffer(SocketAsyncEventArgs args)
		{
			if (m_freeIndexPool.Count > 0) {
				args.SetBuffer (m_buffer, m_freeIndexPool.Pop (), m_bufferSize);
			} else {
				if ((m_numBytes - m_bufferSize) < m_currentIndex) {
					return false;
				}
				args.SetBuffer (m_buffer, m_currentIndex, m_bufferSize);
				m_currentIndex += m_bufferSize;
			}
			return true;
		}

    	// Removes the buffer from a SocketAsyncEventArg object.  
    	// This frees the buffer back to the buffer pool
    	public void FreeBuffer(SocketAsyncEventArgs args)
    	{
        	m_freeIndexPool.Push(args.Offset);
        	args.SetBuffer(null, 0, 0);
    	}
	}

	public class SocketSystem_TCPServer:SocketSystem
	{
		Semaphore m_maxNumberAcceptedClients;
		int m_numConnectedSockets = 0;
		int m_totalBytesRead = 0;

		private ObjectPool<SocketAsyncEventArgs> ioContextPool;
		private List<SocketAsyncEventArgs> mUsedContextPool;
		BufferManager mBufferManager;

		private Socket mmListenSocket = null;
		internal SocketSystem_TCPServer()
		{
			mNetSendSystem = new NetSendSystem (this);
			mNetReceiveSystem = new NetReceiveSystem (this);

			m_maxNumberAcceptedClients = new Semaphore (ServerConfig.numConnections, ServerConfig.numConnections);
			mUsedContextPool = new List<SocketAsyncEventArgs> ();
			ioContextPool = new ObjectPool<SocketAsyncEventArgs> ();

			mBufferManager = new BufferManager (2 * ServerConfig.receiveBufferSize * ServerConfig.numConnections, ServerConfig.receiveBufferSize);
			mBufferManager.InitBuffer ();

			for (Int32 i = 0; i < ServerConfig.numConnections; i++) {
				SocketAsyncEventArgs ioContext = new SocketAsyncEventArgs ();
				ioContext.Completed += new EventHandler<SocketAsyncEventArgs> (OnIOCompleted);
				mBufferManager.SetBuffer (ioContext);
				ioContextPool.recycle (ioContext);
			}
		}

		public override void InitNet (string ServerAddr, int ServerPort)
		{
			IPAddress serverAddr = IPAddress.Parse (ServerAddr);
			IPEndPoint localEndPoint = new IPEndPoint (serverAddr, ServerPort);

			this.mmListenSocket = new Socket (localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

			this.mmListenSocket.Bind (localEndPoint);
			this.mmListenSocket.Listen (0);

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

			m_maxNumberAcceptedClients.WaitOne ();
			if (!this.mmListenSocket.AcceptAsync (acceptEventArg)) {
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
				Client mClient = new Client (s);
				ioContext.UserToken = mClient;

				Interlocked.Increment (ref m_numConnectedSockets);
				string outStr = String.Format ("客户 {0} 连入, 共有 {1} 个连接。", s.RemoteEndPoint.ToString (), this.m_numConnectedSockets);
				DebugSystem.Log (outStr);

				ClientFactory.Instance.AddClient (mClient);

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
				Interlocked.Add (ref m_totalBytesRead, e.BytesTransferred);
				//DebugSystem.Log ("The server has read a total bytes： " + m_totalBytesRead + " | " + e.BytesTransferred);

				var client = e.UserToken as Client;
				mNetReceiveSystem.ReceiveSocketStream (client.getId (), e.Buffer, 0, e.BytesTransferred);

				Socket socket = client.getSocket ();
				if (!socket.ReceiveAsync (e)) {
					this.ProcessReceive (e);
				}
			} else {
				this.CloseClientSocket (e);
			}
		}

		public override void SendNetStream(int socketId, ArraySegment<byte> msg)
		{
			Client mClient = ClientFactory.Instance.GetClient (socketId);
			SocketAsyncEventArgs senddata = null;
			lock(ioContextPool)
			{
				senddata = ioContextPool.Pop ();
			}
			DebugSystem.Assert (senddata != null, "Array is Null");
			Array.Copy (msg.Array, msg.Offset, senddata.Buffer, senddata.Offset, msg.Count);
			senddata.SetBuffer (senddata.Offset, msg.Count);
			mClient.getSocket ().SendAsync (senddata);
		}

		private void ProcessSend(SocketAsyncEventArgs e)
		{
			if (e.SocketError == SocketError.Success) {
				lock (ioContextPool) {
					ioContextPool.recycle (e);
				}
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

			mUsedContextPool.Remove (e);
			ioContextPool.recycle (e);

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
			
		public override void CloseNet ()
		{
			while (mUsedContextPool.Count > 0) {
				var v = mUsedContextPool [0];
				CloseClientSocket (v);
			}
			ioContextPool.release ();

			m_maxNumberAcceptedClients.Release ();
			base.CloseNet ();
		}
	}
}