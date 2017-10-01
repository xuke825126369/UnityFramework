using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.Net;
using xk_System.Debug;
using System.Threading;
using System.Net;
using System;
using System.Net.Sockets;

namespace xk_System.Net.Client.TCP
{
	/// <summary>
	/// 非线程网络实现方法
	/// </summary>
	public class SocketSystem_1: SocketSystem
	{
		public override void init(string ServerAddr, int ServerPort)
		{
			try
			{
				IPEndPoint mIPEndPoint = new IPEndPoint(IPAddress.Parse(ServerAddr), ServerPort);
				mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				mSocket.Connect(mIPEndPoint);
				mSocket.ReceiveTimeout = receiveTimeOut;
				mSocket.SendTimeout = sendTimeOut;
				mSocket.ReceiveBufferSize = receiveInfoPoolCapacity;
				mSocket.SendBufferSize = sendInfoPoolCapacity;
				DebugSystem.Log("Client Net Init Success： IP: " + ServerAddr + " | Port: " + ServerPort);
			}
			catch (SocketException e)
			{
				DebugSystem.LogError(e.SocketErrorCode + " | " + e.Message);
			}
			catch (Exception e)
			{
				DebugSystem.LogError("客户端初始化失败：" + e.Message);
			}
		}

		private ArrayList m_ReadFD = new ArrayList();
		private ArrayList m_WriteFD = new ArrayList();
		private ArrayList m_ExceptFD = new ArrayList();
		private bool Select()
		{
			try
			{
				this.m_ReadFD.Clear();
				this.m_WriteFD.Clear();
				this.m_ExceptFD.Clear();
				this.m_ReadFD.Add(this.mSocket);
				this.m_WriteFD.Add(this.mSocket);
				this.m_ExceptFD.Add(this.mSocket);
				Socket.Select(this.m_ReadFD, this.m_WriteFD, this.m_ExceptFD, 0);

			}catch(SocketException e)
			{
				DebugSystem.LogError(e.SocketErrorCode+" | "+e.Message);
				return false;
			}
			catch (Exception e)
			{
				DebugSystem.LogError(e.Message);
				return false;
			}
			return true;
		}
		private bool ProcessOutput()
		{
			if (this.m_WriteFD.Contains(this.mSocket))
			{
				//  DebugSystem.Log("Send");
				//SendInfo();
			}
			return true;
		}
		private bool ProcessInput()
		{
			if (this.m_ReadFD.Contains(this.mSocket))
			{
				byte[] mbyteStr = new byte[receiveInfoPoolCapacity];
				SocketError error;
				int Length = mSocket.Receive(mbyteStr, 0, mbyteStr.Length, SocketFlags.None, out error);
				while (mSocket.Available > 0)
				{
					for (int i = 0; i < Length; i++)
					{
						mStoreByteList.Add(mbyteStr[i]);
					}
					mbyteStr = new byte[receiveInfoPoolCapacity];
					Length = mSocket.Receive(mbyteStr, 0, mbyteStr.Length, SocketFlags.None, out error);
				}

				byte[] mStr = null;
				if (mStoreByteList.Count > 0)
				{
					for (int i = 0; i < Length; i++)
					{
						mStoreByteList.Add(mbyteStr[i]);
					}
					mStr = mStoreByteList.ToArray();
					mStoreByteList.Clear();
				}
				else
				{
					mStr = new byte[Length];
					Array.Copy(mbyteStr, mStr, Length);
				}
				string Tag="收到消息:" + Length;
				DebugSystem.LogBitStream(Tag,mStr);
				mNetReceiveSystem.Receive(mStr);
			}
			return true;
		}

		private bool ProcessExcept()
		{
			if (this.m_ExceptFD.Contains(this.mSocket))
			{
				this.mSocket.Close();
				DebugSystem.Log("m_Socket.close(), SocketSystem::ProcessExcept");
				return false;
			}
			return true;
		}

		List<byte> mStoreByteList = new List<byte>();

		public void Receive()
		{
			try
			{
				if ((this.mSocket != null) && this.mSocket.Connected)
				{
					if (Select() && ProcessExcept())
					{
						ProcessInput();
					}
				}
			}
			catch (SocketException e)
			{
				DebugSystem.LogError("接受异常： " + e.Message + " | " + e.SocketErrorCode);
				Thread.CurrentThread.Abort();
			}
			catch (Exception e)
			{
				DebugSystem.LogError("接受异常： " + e.Message);
			}
		}

		public override void SendInfo(byte[] msg)
		{
			try
			{
				SocketError merror;
				mSocket.Send(msg, 0, msg.Length, SocketFlags.None, out merror);
				if (merror == SocketError.Success)
				{
					string Tag = "发送消息:" + msg.Length;
					DebugSystem.LogBitStream(Tag, msg);
				}
				else
				{
					DebugSystem.LogError("发送失败: " + merror);
				}
			}
			catch (SocketException e)
			{
				DebugSystem.LogError(e.SocketErrorCode + " | " + e.Message);
			}
			catch (Exception e)
			{
				DebugSystem.LogError(e.Message);
			}
		}

		public override void CloseNet()
		{
			base.CloseNet();
		}

	}

	public class SocketSystem_2 : SocketSystem
	{
		bool OrConnection = false;
		public override void init(string ServerAddr, int ServerPort)
		{
			try
			{
				IPEndPoint mIPEndPoint = new IPEndPoint(IPAddress.Parse(ServerAddr), ServerPort);
				mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				mSocket.Connect(mIPEndPoint);
				mSocket.ReceiveTimeout = receiveTimeOut;
				mSocket.SendTimeout = sendTimeOut;
				mSocket.ReceiveBufferSize = receiveInfoPoolCapacity;
				mSocket.SendBufferSize = sendInfoPoolCapacity;
				mSocket.Blocking = false;
				DebugSystem.Log("Client Net Init Success： IP: " + ServerAddr + " | Port: " + ServerPort);
			}
			catch (SocketException e)
			{
				DebugSystem.LogError("客户端初始化失败000： " + e.SocketErrorCode + " | " + e.Message);
			}
			catch (Exception e)
			{
				DebugSystem.LogError("客户端初始化失败111：" + e.Message);
			}
		}

		List<byte> mStoreByteList = new List<byte>();
		byte[] mbyteStr = new byte[receiveInfoPoolCapacity];

		public int Receive()
		{
			try
			{
				SocketError error;
				int Length = mSocket.Receive(mbyteStr, 0, mbyteStr.Length, SocketFlags.None, out error);
				if (Length == -1)
				{
					DebugSystem.Log(Length);
				}
				else if (Length == 0)
				{
					if (error == SocketError.TimedOut)
					{
						DebugSystem.Log("连接超时");
					}
					else if (error == SocketError.Success)
					{
						DebugSystem.Log("服务器主动断开连接");
						return -1;
					}
				}
				else
				{
					byte[] mStr = new byte[Length];
					Array.Copy(mbyteStr, mStr, Length);

					string Tag="收到消息:" + Length + " | " + mStr.Length + " | " + receiveInfoPoolCapacity;
					DebugSystem.LogBitStream(Tag,mStr);
					mNetReceiveSystem.Receive(mStr);
				}
			}
			catch (SocketException e)
			{
				DebugSystem.LogError("接受异常0000： " + e.Message + " | " + e.SocketErrorCode);
				return -1;
			}
			catch (Exception e)
			{
				DebugSystem.LogError("接受异常11111： " + e.Message + " | " + e.StackTrace);
				return -1;
			}
			return 0;

		}

		public override void SendInfo(byte[] msg)
		{
			try
			{
				SocketError merror;
				mSocket.Send(msg, 0, msg.Length, SocketFlags.None, out merror);
				if (merror == SocketError.Success)
				{
					string Tag="发送成功:" + msg.Length;
					DebugSystem.LogBitStream(Tag,msg);
				}
				else
				{
					DebugSystem.LogError("发送失败: " + merror);
				}

			}
			catch (SocketException e)
			{
				DebugSystem.LogError(e.SocketErrorCode + " | " + e.Message);
			}
			catch (Exception e)
			{
				DebugSystem.LogError(e.Message);
			}
		}

		public override void CloseNet()
		{
			base.CloseNet();
		}

	}

	public class SocketSystem_Thread : SocketSystem
	{
		private List<Thread> ThreadPool;
		public override void init(string ServerAddr, int ServerPort)
		{
			try
			{
				IPEndPoint mIPEndPoint = new IPEndPoint(IPAddress.Parse(ServerAddr), ServerPort);
				mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				mSocket.Connect(mIPEndPoint);
				mSocket.ReceiveTimeout = receiveTimeOut;
				mSocket.SendTimeout = sendTimeOut;
				mSocket.ReceiveBufferSize = receiveInfoPoolCapacity;
				mSocket.SendBufferSize = sendInfoPoolCapacity;
				mSocket.Blocking = true;

				NewStartThread_Receive();
				DebugSystem.Log("Client Net Init Success： IP: " + ServerAddr + " | Port: " + ServerPort);
			}
			catch(SocketException e)
			{
				DebugSystem.LogError(e.SocketErrorCode+" | " + e.Message);
			}
			catch (Exception e)
			{
				DebugSystem.LogError("客户端初始化失败："+e.Message);
			}
		}

		private void NewStartThread_Receive()
		{
			// Thread mThread = new Thread(ReceiveInfo);
			Thread mThread = new Thread(Receive);
			mThread.IsBackground = false;
			mThread.Start();
			if (ThreadPool == null)
			{
				ThreadPool = new List<Thread>();
			}
			ThreadPool.Add(mThread);
		}

		List<byte> mStoreByteList = new List<byte>();
		byte[] mbyteStr = new byte[receiveInfoPoolCapacity];

		/// <summary>
		/// 用Socket.Avaiable，可以用来防止接受的流是个残废的流（不完整的流）比如(发了一条数据，不用Avaiable，则有可能得到的是一个多流加一个半流)
		/// </summary>
		private void ReceiveInfo()
		{
			while (mSocket!=null)
			{
				try
				{                                     
					SocketError error;                  
					int Length = mSocket.Receive(mbyteStr, 0, mbyteStr.Length, SocketFlags.None, out error);                   
					// DebugSystem.LogBitStream(mbyteStr);
					// DebugSystem.Log("Error: "+error);
					// DebugSystem.Log("Available: " + mSocket.Available + " | " + Length);
					if (Length == -1)
					{
						DebugSystem.LogError("接受长度："+Length);
						CloseNet();
						break;
					}
					else if (Length == 0)
					{
						if (error == SocketError.TimedOut)
						{
							//DebugSystem.LogError("连接超时");
						}else if(error==SocketError.Success)
						{
							DebugSystem.LogError("服务器主动断开连接");
							CloseNet();
							break;
						}
					}
					else if (mSocket.Available > 0)
					{
						for (int i = 0; i < Length; i++)
						{
							mStoreByteList.Add(mbyteStr[i]);
						}
					}
					else
					{
						byte[] mStr = null;
						if (mStoreByteList.Count > 0)
						{
							for (int i = 0; i < Length; i++)
							{
								mStoreByteList.Add(mbyteStr[i]);
							}
							mStr = mStoreByteList.ToArray();
							mStoreByteList.Clear();
						}
						else
						{
							mStr = new byte[Length];
							Array.Copy(mbyteStr, mStr, Length);
						}
						string Tag = "收到消息: " + Length + " | " + mStr.Length + " | " + receiveInfoPoolCapacity;
						DebugSystem.LogBitStream(Tag, mStr);
						mNetReceiveSystem.Receive(mStr);
					}
				}
				catch (SocketException e)
				{
					DebugSystem.LogError("接受异常0000： "+e.Message +" | "+e.SocketErrorCode);
					break;
				}catch(Exception e)
				{
					DebugSystem.LogError("接受异常11111： "+e.Message+" | "+e.StackTrace);
					break;
				}
			}
			DebugSystem.LogError("网络线程结束");

		}

		private void Receive()
		{
			while (mSocket!=null)
			{
				try
				{
					SocketError error;
					int Length = mSocket.Receive(mbyteStr, 0, mbyteStr.Length, SocketFlags.None, out error);
					if (Length == -1)
					{
						DebugSystem.LogError("接受长度："+Length);
						CloseNet();
						break;
					}
					else if (Length == 0)
					{
						if (error == SocketError.TimedOut)
						{
							//DebugSystem.Log("连接超时");
						}
						else if (error == SocketError.Success)
						{
							DebugSystem.LogError("服务器主动断开连接");
							CloseNet();
							break;
						}
					}
					else
					{
						byte[] mStr = new byte[Length];
						Array.Copy(mbyteStr, mStr, Length);

						string Tag="收到消息: " + Length+" | "+mStr.Length + " | " + receiveInfoPoolCapacity;
						DebugSystem.LogBitStream(Tag,mStr);
						mNetReceiveSystem.Receive(mStr);
					}
				}
				catch (SocketException e)
				{
					DebugSystem.LogError("接受异常0000： " + e.Message + " | " + e.SocketErrorCode);
					break;
				}
				catch (Exception e)
				{
					DebugSystem.LogError("接受异常11111： " + e.Message + " | " + e.StackTrace);
					break;
				}
			}

			DebugSystem.LogError("网络线程结束");

		}

		public override void SendInfo(byte[] msg)
		{
			try
			{
				SocketError merror;
				mSocket.Send(msg, 0, msg.Length, SocketFlags.None, out merror);
				string Tag = "";
				if (merror == SocketError.Success)
				{
					Tag="发送成功:" + msg.Length;
				} else
				{
					Tag="发送失败: " +merror;
				}
				DebugSystem.LogBitStream(Tag, msg);
			}
			catch(SocketException e)
			{
				DebugSystem.LogError(e.SocketErrorCode+" | "+e.Message);
			}catch(Exception e)
			{
				DebugSystem.LogError(e.Message);
			}
		}

		public override void CloseNet()
		{
			if (ThreadPool != null)
			{
				foreach (Thread t in ThreadPool)
				{
					t.Abort();
				}
				ThreadPool.Clear();
			}
			base.CloseNet();     
		}

	}

	public class SocketSystem_Async:SocketSystem
	{
		// 发送和接收的超时时间
		public int _sendTimeout = 3;
		public int _revTimeout = 3;

		public SocketSystem_Async()
		{

		}

		public override void init(string ServerAddr, int ServerPort)
		{       
			if (mSocket != null && mSocket.Connected)
			{
				return;
			}
			try
			{
				mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				mSocket.SendTimeout = _sendTimeout;
				mSocket.ReceiveTimeout = _revTimeout;

				IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(ServerAddr), ServerPort);
				mSocket.BeginConnect(ipe, new System.AsyncCallback(ConnectionCallback), null);
			}
			catch (System.Exception e)
			{
				DebugSystem.LogError(e.Message);

			}
		}

		public override void SendInfo(byte[] msg)
		{
			Send(msg);
		}


		// 异步连接回调
		void ConnectionCallback(System.IAsyncResult ar)
		{
			mSocket.EndConnect(ar);
			try
			{
				byte[] stream = new byte[4096];
				mSocket.BeginReceive(stream, 0, stream.Length, SocketFlags.None, new System.AsyncCallback(ReceiveInfo), stream);
			}
			catch (System.Exception e)
			{
				if (e.GetType() == typeof(SocketException))
				{
					if (((SocketException)e).SocketErrorCode == SocketError.ConnectionRefused)
					{

					}
					else
					{

					}
				}
				Disconnect(0);
			}
		}

		// 接收消息体
		void ReceiveInfo(System.IAsyncResult ar)
		{
			byte[] stream = (byte[])ar.AsyncState;
			try
			{
				int read = mSocket.EndReceive(ar);

				// 用户已下线
				if (read < 1)
				{
					Disconnect(0);
					return;
				}
				NetReceiveSystem.getSingle().Receive(stream);
				mSocket.BeginReceive(stream, 0, stream.Length, SocketFlags.None, new System.AsyncCallback(ReceiveInfo), stream);

			}
			catch (System.Exception e)
			{
				Disconnect(0);
			}
		}

		// 发送消息
		public void Send(byte[] bts)
		{
			if (!mSocket.Connected)
				return;

			NetworkStream ns;
			lock (mSocket)
			{
				ns = new NetworkStream(mSocket);
			}

			if (ns.CanWrite)
			{
				try
				{
					ns.BeginWrite(bts, 0, bts.Length, new System.AsyncCallback(SendCallback), ns);
				}
				catch (System.Exception)
				{
					Disconnect(0);
				}
			}
		}

		//发送回调
		private void SendCallback(System.IAsyncResult ar)
		{
			NetworkStream ns = (NetworkStream)ar.AsyncState;
			try
			{
				ns.EndWrite(ar);
				ns.Flush();
				ns.Close();
			}
			catch (System.Exception)
			{
				Disconnect(0);
			}

		}

		// 关闭连接
		public void Disconnect(int timeout)
		{
			if (mSocket.Connected)
			{
				mSocket.Shutdown(SocketShutdown.Receive);
				mSocket.Close(timeout);
			}
			else
			{
				mSocket.Close();
			}

		}
	}

	public class SocketSevice:SocketSystem
	{
		SocketAsyncEventArgs ReceiveArgs;

		public override void init(string ServerAddr, int ServerPort)
		{
			try
			{
				mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				IPAddress mIPAddress = IPAddress.Parse(ServerAddr);
				IPEndPoint mIPEndPoint = new IPEndPoint(mIPAddress, ServerPort);
				mSocket.Connect(mIPEndPoint);
				ConnectServer();
				DebugSystem.Log("Client Net Init Success： IP: " + ServerAddr + " | Port: " + ServerPort);
			}
			catch (SocketException e)
			{
				DebugSystem.LogError("客户端初始化失败：" + e.Message +" | "+e.SocketErrorCode);
			}
		}

		private void ConnectServer()
		{
			ReceiveArgs = new SocketAsyncEventArgs();
			ReceiveArgs.Completed += Receive_Fun;
			ReceiveArgs.SetBuffer(new byte[receiveInfoPoolCapacity], 0, receiveInfoPoolCapacity);
			mSocket.ReceiveAsync(ReceiveArgs);
		}

		public override void SendInfo(byte[] msg)
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

		private List<byte> mStorebyteList = new List<byte>();

		private void Receive_Fun(object sender, SocketAsyncEventArgs e)
		{
			if (e.SocketError == SocketError.Success)
			{
				if (e.BytesTransferred > 0)
				{
					// DebugSystem.Log("接收数据个数： " + e.BytesTransferred);
					// DebugSystem.Log("接收数据个数： " + e.Buffer.Length);
					// DebugSystem.Log("接收数据个数： " + mSocket.Available);
					if (mSocket.Available > 0)
					{
						foreach (byte b in e.Buffer)
						{
							mStorebyteList.Add(b);
						}
						// DebugSystem.LogError("传输字节数超出缓冲数组: "+mSocket.Available);
					}
					else
					{
						byte[] mbyteArray = null;
						if (mStorebyteList.Count > 0)
						{
							for (int i = 0; i < e.BytesTransferred; i++)
							{
								mStorebyteList.Add(e.Buffer[i]);
							}
							mbyteArray = mStorebyteList.ToArray();
							mStorebyteList.Clear();
						}
						else
						{
							mbyteArray = new byte[e.BytesTransferred];
							Array.Copy(e.Buffer,mbyteArray,mbyteArray.Length);                         
						}
						mNetReceiveSystem.Receive(mbyteArray);
					}             
				}
			}else
			{
				DebugSystem.Log("接收数据失败： " + e.SocketError.ToString());
			}
			mSocket.ReceiveAsync(e);
		}


		public override void CloseNet()
		{
			base.CloseNet();
		}


	}
}
