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
	public class NetTCPServer : MonoBehaviour
	{
		
	}

	/// <summary>
	/// 基于SocketAsyncEventArgs 实现 IOCP 服务器
	/// </summary>
	internal sealed class SocketSystem_TCPServer:SocketSystem
	{
		private static Mutex mutex = new Mutex();
		private Int32 numConnectedSockets;
		private Int32 numConnections;
		private Int32 bufferSize;
		private static SystemObjectPool<SocketAsyncEventArgs> ioContextPool;

		internal SocketSystem_TCPServer(Int32 numConnections = 10000 , Int32 bufferSize = 8192)
		{
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
			IPAddress[] addressList = Dns.GetHostEntry(Environment.MachineName).AddressList;
			IPEndPoint localEndPoint = new IPEndPoint(addressList[addressList.Length - 1], ServerPort);

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

		public override void SendNetStream(byte[] msg)
		{
			SocketError mError=SocketError.SocketError;
			try
			{
				mSocket.Send(msg,0,msg.Length,SocketFlags.None,out mError);
			}catch(Exception e)
			{
				DebugSystem.LogError("发送字节失败： "+e.Message+" | "+mError.ToString());
			}
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
				this.ProcessReceive(e);
				break;
			case SocketAsyncOperation.Send:
				this.ProcessSend(e);
				break;
			default:
				throw new ArgumentException("The last operation completed on the socket was not a receive or send");
			}
		}

		private void ProcessReceive(SocketAsyncEventArgs e)
		{
			if (e.BytesTransferred > 0)
			{
				if (e.SocketError == SocketError.Success)
				{
					Socket s = (Socket)e.UserToken;
					if (s.Available == 0)
					{
						mNetReceiveSystem.ReceiveSocketStream(e.Buffer);
					}
					else if (!s.ReceiveAsync(e))
					{
						this.ProcessReceive(e);
					}
				}
				else
				{
					this.ProcessError(e);
				}
			}
			else
			{
				this.CloseClientSocket(e);
			}
		}

		/// <summary>
		/// 发送完成时处理函数
		/// </summary>
		/// <param name="e">与发送完成操作相关联的SocketAsyncEventArg对象</param>
		private void ProcessSend(SocketAsyncEventArgs e)
		{
			if (e.SocketError == SocketError.Success)
			{
				Socket s = (Socket)e.UserToken;

				//接收时根据接收的字节数收缩了缓冲区的大小，因此投递接收请求时，恢复缓冲区大小
				e.SetBuffer(0, bufferSize);
				if (!s.ReceiveAsync(e))     //投递接收请求
				{
					// 同步接收时处理接收完成事件
					this.ProcessReceive(e);
				}
			}
			else
			{
				this.ProcessError(e);
			}
		}

		/// <summary>
		/// 处理socket错误
		/// </summary>
		/// <param name="e"></param>
		private void ProcessError(SocketAsyncEventArgs e)
		{
			Socket s = e.UserToken as Socket;
			IPEndPoint localEp = s.LocalEndPoint as IPEndPoint;

			this.CloseClientSocket(s, e);

			string outStr = String.Format("套接字错误 {0}, IP {1}, 操作 {2}。", (Int32)e.SocketError, localEp, e.LastOperation);
			DebugSystem.LogError (outStr);
			//Console.WriteLine("Socket error {0} on endpoint {1} during {2}.", (Int32)e.SocketError, localEp, e.LastOperation);
		}

		/// <summary>
		/// 关闭socket连接
		/// </summary>
		/// <param name="e">SocketAsyncEventArg associated with the completed send/receive operation.</param>
		private void CloseClientSocket(SocketAsyncEventArgs e)
		{
			Socket s = e.UserToken as Socket;
			this.CloseClientSocket(s, e);
		}

		private void CloseClientSocket(Socket s, SocketAsyncEventArgs e)
		{
			Interlocked.Decrement(ref this.numConnectedSockets);

			// SocketAsyncEventArg 对象被释放，压入可重用队列。
			ioContextPool.Push(e);            
			string outStr = String.Format("客户 {0} 断开, 共有 {1} 个连接。", s.RemoteEndPoint.ToString(), this.numConnectedSockets);
			DebugSystem.Log (outStr);        
			//Console.WriteLine("A client has been disconnected from the server. There are {0} clients connected to the server", this.numConnectedSockets);
			try
			{
				s.Shutdown(SocketShutdown.Send);
			}
			catch (Exception)
			{
				// Throw if client has closed, so it is not necessary to catch.
			}
			finally
			{
				s.Close();
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
						ioContext.UserToken = s;

						Interlocked.Increment(ref this.numConnectedSockets);
						string outStr = String.Format("客户 {0} 连入, 共有 {1} 个连接。",  s.RemoteEndPoint.ToString(),this.numConnectedSockets);
						DebugSystem.Log(outStr);

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
			
		public override void CloseNet ()
		{
			this.mSocket.Close();
			mutex.ReleaseMutex();
		}

	}
}