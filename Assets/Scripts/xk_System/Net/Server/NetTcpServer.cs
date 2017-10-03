using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using xk_System.Debug;
using UnityEngine;
using xk_System.Crypto;
using System.Collections;
using Google.Protobuf;
using game.protobuf.data;

namespace xk_System.Net.Server
{
	/// <summary>
	/// 基于SocketAsyncEventArgs 实现 IOCP 服务器
	/// </summary>
	public class SocketSystem_TCPServer:SocketSystem
	{
		private static Mutex mutex = new Mutex();
		private Int32 numConnectedSockets;
		int m_totalBytesRead; 
		private Int32 numConnections;
		private Int32 bufferSize;
		private static SystemObjectPool<SocketAsyncEventArgs> ioContextPool;

		internal SocketSystem_TCPServer(Int32 numConnections = 100 , Int32 bufferSize = 8192)
		{
			this.m_totalBytesRead = 0;
			this.numConnectedSockets = 0;
			this.numConnections = numConnections;
			this.bufferSize = bufferSize;

			ioContextPool = SystemObjectPool<SocketAsyncEventArgs>.Instance;

			for (Int32 i = 0; i < this.numConnections; i++)
			{
				SocketAsyncEventArgs ioContext = new SocketAsyncEventArgs();
				ioContext.Completed += new EventHandler<SocketAsyncEventArgs>(OnIOCompleted);
				ioContext.SetBuffer(new Byte[this.bufferSize], 0, this.bufferSize);
				ioContextPool.Push(ioContext);
			}
		}

		public override void init (string ServerAddr, int ServerPort)
		{
			IPAddress serverAddr = IPAddress.Parse (ServerAddr);
			IPEndPoint localEndPoint = new IPEndPoint(serverAddr, ServerPort);

			this.mSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			this.mSocket.ReceiveBufferSize = bufferSize;
			this.mSocket.SendBufferSize = bufferSize;

			if (localEndPoint.AddressFamily == AddressFamily.InterNetworkV6)
			{
				this.mSocket.SetSocketOption(SocketOptionLevel.IPv6, (SocketOptionName)27, false);
				this.mSocket.Bind(new IPEndPoint(IPAddress.IPv6Any, localEndPoint.Port));
			}
			else
			{
				this.mSocket.Bind(localEndPoint);
			}
				
			this.mSocket.Listen(numConnections);
			this.StartAccept(null);

			mutex.WaitOne();
		}
			
		private void StartAccept(SocketAsyncEventArgs acceptEventArg)
		{
			if (acceptEventArg == null)
			{
				acceptEventArg = new SocketAsyncEventArgs();
				acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
			}
			else
			{
				// 重用前进行对象清理
				acceptEventArg.AcceptSocket = null;
			}

			if (!this.mSocket.AcceptAsync(acceptEventArg))
			{
				this.ProcessAccept(acceptEventArg);
			}
		}
			
		private void OnIOCompleted(object sender, SocketAsyncEventArgs e)
		{
			// Determine which type of operation just completed and call the associated handler.
			switch (e.LastOperation)
			{
			case SocketAsyncOperation.Receive:
				this.ProcessReceive (e);
				DebugSystem.Log ("接受数据");
				break;
			case SocketAsyncOperation.Send:
				this.ProcessSend(e);
				DebugSystem.Log ("读取数据");
				break;
			default:
				throw new ArgumentException("The last operation completed on the socket was not a receive or send");
			}
		}

		private void OnAcceptCompleted(object sender, SocketAsyncEventArgs e)
		{
			this.ProcessAccept(e);
		}

		private void ProcessAccept(SocketAsyncEventArgs e)
		{
			Socket s = e.AcceptSocket;
			if (s.Connected)
			{
				try
				{
					SocketAsyncEventArgs ioContext = ioContextPool.Pop();
					if (ioContext != null)
					{
						Client mClient = new Client(s);
						ioContext.UserToken = mClient;

						Interlocked.Increment(ref this.numConnectedSockets);
						string outStr = String.Format("客户 {0} 连入, 共有 {1} 个连接。",  s.RemoteEndPoint.ToString(),this.numConnectedSockets);
						DebugSystem.Log(outStr);

						ClientFactory.Instance.AddClient(mClient);

						if (!s.ReceiveAsync(ioContext))
						{
							this.ProcessReceive(ioContext);
						}
					}
					else
					{
						s.Send(Encoding.Default.GetBytes("连接已经达到最大数!"));
						string outStr = String.Format("连接已满，拒绝 {0} 的连接。", s.RemoteEndPoint);
						DebugSystem.Log(outStr);
						s.Close();
					}
				}
				catch (SocketException ex)
				{
					Socket token = e.UserToken as Socket;
					string outStr = String.Format("接收客户 {0} 数据出错, 异常信息： {1} 。", token.RemoteEndPoint, ex.ToString());
					DebugSystem.LogError (outStr);
				}
				catch (Exception ex)
				{
					DebugSystem.LogError ("异常：" + ex.ToString ());
				}
				// 投递下一个接受请求
				this.StartAccept(e);
			}
		}

		private void ProcessReceive(SocketAsyncEventArgs e)
		{
			if (e.SocketError == SocketError.Success) {
				if (e.BytesTransferred > 0) {
					Interlocked.Add (ref m_totalBytesRead, e.BytesTransferred);
					DebugSystem.Log ("The server has read a total bytes： " + m_totalBytesRead + " | " + e.BytesTransferred);

					byte[] mReceive = new byte[e.BytesTransferred];
					Array.Copy (e.Buffer, 0, mReceive, 0, mReceive.Length);

					var client = e.UserToken as Client;
					mNetReceiveSystem.ReceiveSocketStream (client.getId(),mReceive);

					Socket socket = client.getSocket();
					if (!socket.ReceiveAsync (e)) {
						this.ProcessReceive (e);
					}
				} else {
					this.CloseClientSocket (e);
				}
			} else {
				this.CloseClientSocket (e);
			}
		}


		public override void SendNetStream(int clientId,byte[] msg)
		{
			Client mClient = ClientFactory.Instance.GetClient (clientId);
			SocketError mError=SocketError.SocketError;
			try
			{
				var senddata = ioContextPool.Pop();
				senddata.SetBuffer(msg,0,msg.Length);
				mClient.getSocket().SendAsync(senddata);
			}catch(Exception e)
			{
				DebugSystem.LogError("发送字节失败： "+e.Message+" | "+mError.ToString());
			}
		}
			
		private void ProcessSend(SocketAsyncEventArgs e)
		{
			if (e.SocketError == SocketError.Success)
			{
				DebugSystem.Log ("Server 发送成功");
			}
			else
			{
				this.CloseClientSocket(e);
			}
		}
			
		//这里是关闭客户端链接
		private void CloseClientSocket(SocketAsyncEventArgs e)
		{
			Socket s = e.UserToken as Socket;
			Interlocked.Decrement(ref this.numConnectedSockets);           
			string outStr = String.Format("客户 {0} 断开, 共有 {1} 个连接。", s.RemoteEndPoint.ToString(), this.numConnectedSockets);
			DebugSystem.Log (outStr);        
			IPEndPoint localEp = s.LocalEndPoint as IPEndPoint;
			outStr = String.Format("套接字错误 {0}, IP {1}, 操作 {2}。", (Int32)e.SocketError, localEp, e.LastOperation);
			DebugSystem.LogError (outStr);

			ioContextPool.Push(e);
			s.Close();
			s = null;
		}
			
		//关闭服务器
		public override void CloseNet ()
		{
			this.mSocket.Close();
			mutex.ReleaseMutex();
		}
	}

	class BufferManager
	{
		public BufferManager(int count,int receiveBufferSize)
		{


		}



	}
}