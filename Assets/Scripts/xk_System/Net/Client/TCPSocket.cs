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
	//Select
	public class SocketSystem_Select: SocketSystem
	{
		private Socket mSocket;

		private ArrayList m_ReadFD = new ArrayList ();
		private ArrayList m_WriteFD = new ArrayList ();
		private ArrayList m_ExceptFD = new ArrayList ();

		byte[] mReceiveStream = new byte[ClientConfig.nMaxPackageSize * ClientConfig.nPerFrameHandlePackageCount];
		public override void init (string ServerAddr, int ServerPort)
		{
			try {
				IPEndPoint mIPEndPoint = new IPEndPoint (IPAddress.Parse (ServerAddr), ServerPort);
				mSocket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				mSocket.Connect (mIPEndPoint);
				ConfigureSocket (mSocket);
				DebugSystem.Log ("Client Net Init Success： IP: " + ServerAddr + " | Port: " + ServerPort);
			} catch (SocketException e) {
				DebugSystem.LogError (e.SocketErrorCode + " | " + e.Message);
			} catch (Exception e) {
				DebugSystem.LogError ("客户端初始化失败：" + e.Message);
			}
		}

		public override void ConfigureSocket (Socket mSocket)
		{
			mSocket.ReceiveBufferSize = 100;
			mSocket.ReceiveTimeout = 0;
			mSocket.SendTimeout = 0;
			mSocket.Blocking = false;
			PrintSocketConfigInfo (mSocket);
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
				} else {
					DebugSystem.LogError ("WriteFD 不可Send");
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
				mNetReceiveSystem.ReceiveSocketStream (mReceiveStream, 0, Length);
				if (mSocket.Available > 0) {
					DebugSystem.LogError ("Available > 0： " + Length + " | " + mReceiveStream.Length);
					ProcessInput ();

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


		public override void Update ()
		{
			if (this.CheckSocketState ()) {
				this.Select ();
			}
		}

		public override void SendNetStream (byte[] msg, int index, int Length)
		{
			try {
				SocketError merror;
				int sendLength = mSocket.Send (msg, index, Length, SocketFlags.None, out merror);
				if (sendLength != Length) {
					DebugSystem.LogError ("Client:SendLength:  " + sendLength + " | " + Length);
				}
				if (merror != SocketError.Success) {
					if (mSocket.Blocking == false && merror == SocketError.WouldBlock) {
						SendNetStream (msg, index, Length);
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

		public override void CloseNet ()
		{
			if (mSocket != null) {
				mSocket.Close ();
				mSocket = null;
			}
		}
	}

	//Pool
	public class SocketSystem_Poll : SocketSystem
	{
		private Socket mSocket = null;
		byte[] mReceiveStream = new byte[receiveInfoPoolCapacity];
		public override void init (string ServerAddr, int ServerPort)
		{
			try {
				IPEndPoint mIPEndPoint = new IPEndPoint (IPAddress.Parse (ServerAddr), ServerPort);
				mSocket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				mSocket.Connect (mIPEndPoint);
				mSocket.ReceiveTimeout = receiveTimeOut;
				mSocket.SendTimeout = sendTimeOut;
				mSocket.ReceiveBufferSize = receiveInfoPoolCapacity;
				mSocket.SendBufferSize = sendInfoPoolCapacity;
				mSocket.Blocking = false;
				DebugSystem.Log ("Client Net Init Success： IP: " + ServerAddr + " | Port: " + ServerPort);
			} catch (SocketException e) {
				DebugSystem.LogError ("客户端初始化失败000： " + e.SocketErrorCode + " | " + e.Message);
			} catch (Exception e) {
				DebugSystem.LogError ("客户端初始化失败111：" + e.Message);
			}
		}

		private void Poll()
		{
			if (mSocket.Poll (0, SelectMode.SelectWrite)) {
				DebugSystem.Log ("This Socket is writable.");
			} 

			if (mSocket.Poll (0, SelectMode.SelectRead)) {
				DebugSystem.Log ("This Socket is readable.");
			}

			if (mSocket.Poll (0, SelectMode.SelectError)) {
				DebugSystem.Log ("This Socket has an error.");
			}
		}

		private void ProcessInput()
		{
			SocketError error;
			int Length = mSocket.Receive (mReceiveStream, 0, mReceiveStream.Length, SocketFlags.None, out error);
			mNetReceiveSystem.ReceiveSocketStream (mReceiveStream, 0, Length);
		}

		public override void Update ()
		{
			Poll ();
		}

		public override void SendNetStream (byte[] msg,int offset,int Length)
		{
			try {
				SocketError merror;
				mSocket.Send (msg, offset, Length, SocketFlags.None, out merror);
				if (merror == SocketError.Success) {
					string Tag = "发送成功:" + msg.Length;
					DebugSystem.LogBitStream (Tag, msg);
				} else {
					DebugSystem.LogError ("发送失败: " + merror);
				}

			} catch (SocketException e) {
				DebugSystem.LogError (e.SocketErrorCode + " | " + e.Message);
			} catch (Exception e) {
				DebugSystem.LogError (e.Message);
			}
		}

		public override void CloseNet ()
		{
			if (mSocket != null) {
				mSocket.Close ();
				mSocket = null;
			}
		}
	}

	public class SocketSystem_Thread : SocketSystem
	{
		Thread mThread = null;

		protected Socket mSocket = null;
		public override void init (string ServerAddr, int ServerPort)
		{
			try {
				IPEndPoint mIPEndPoint = new IPEndPoint (IPAddress.Parse (ServerAddr), ServerPort);
				mSocket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				mSocket.Connect (mIPEndPoint);
				mSocket.ReceiveTimeout = receiveTimeOut;
				mSocket.SendTimeout = sendTimeOut;
				mSocket.ReceiveBufferSize = receiveInfoPoolCapacity;
				mSocket.SendBufferSize = sendInfoPoolCapacity;
				mSocket.Blocking = false;

				NewStartThread_Receive ();
				DebugSystem.Log ("Client Net Init Success： IP: " + ServerAddr + " | Port: " + ServerPort);
			} catch (SocketException e) {
				DebugSystem.LogError (e.SocketErrorCode + " | " + e.Message);
			} catch (Exception e) {
				DebugSystem.LogError ("客户端初始化失败：" + e.Message);
			}
		}

		public override void Update ()
		{
			
		}

		private void NewStartThread_Receive ()
		{
			mThread = new Thread (Receive);
			mThread.IsBackground = false;
			mThread.Start ();
		}

		List<byte> mStoreByteList = new List<byte> ();
		byte[] mbyteStr = new byte[receiveInfoPoolCapacity];

		/// <summary>
		/// 用Socket.Avaiable，可以用来防止接受的流是个残废的流（不完整的流）比如(发了一条数据，不用Avaiable，则有可能得到的是一个多流加一个半流)
		/// </summary>
		private void ReceiveInfo ()
		{
			while (mSocket != null) {
				Thread.Sleep (1);
				try {                                     
					SocketError error;                  
					int Length = mSocket.Receive (mbyteStr, 0, mbyteStr.Length, SocketFlags.None, out error);                   
					// DebugSystem.LogBitStream(mbyteStr);
					// DebugSystem.Log("Error: "+error);
					// DebugSystem.Log("Available: " + mSocket.Available + " | " + Length);
					if (Length == -1) {
						DebugSystem.LogError ("接受长度：" + Length);
						CloseNet ();
						break;
					} else if (Length == 0) {
						if (error == SocketError.TimedOut) {
							//DebugSystem.LogError("连接超时");
						} else if (error == SocketError.Success) {
							DebugSystem.LogError ("服务器主动断开连接");
							CloseNet ();
							break;
						}
					} else if (mSocket.Available > 0) {
						for (int i = 0; i < Length; i++) {
							mStoreByteList.Add (mbyteStr [i]);
						}
					} else {
						byte[] mStr = null;
						if (mStoreByteList.Count > 0) {
							for (int i = 0; i < Length; i++) {
								mStoreByteList.Add (mbyteStr [i]);
							}
							mStr = mStoreByteList.ToArray ();
							mStoreByteList.Clear ();
						} else {
							mStr = new byte[Length];
							Array.Copy (mbyteStr, mStr, Length);
						}
						string Tag = "收到消息: " + Length + " | " + mStr.Length + " | " + receiveInfoPoolCapacity;
						DebugSystem.LogBitStream (Tag, mStr);
						mNetReceiveSystem.ReceiveSocketStream (mStr,0,mStr.Length);
					}
				} catch (SocketException e) {
					DebugSystem.LogError ("接受异常0000： " + e.Message + " | " + e.SocketErrorCode);
					break;
				} catch (Exception e) {
					DebugSystem.LogError ("接受异常11111： " + e.Message + " | " + e.StackTrace);
					break;
				}
			}
			DebugSystem.LogError ("网络线程结束");

		}

		private void Receive ()
		{
			while (mSocket != null) {
				try {
					SocketError error;
					int Length = mSocket.Receive (mbyteStr, 0, mbyteStr.Length, SocketFlags.None, out error);
					if (Length == -1) {
						DebugSystem.LogError ("接受长度：" + Length);
						CloseNet ();
						break;
					} else if (Length == 0) {
						if (error == SocketError.TimedOut) {
							DebugSystem.Log ("连接超时");
						} else if (error == SocketError.Success) {
							DebugSystem.LogError ("服务器主动断开连接");
							CloseNet ();
							break;
						}
					} else {
						byte[] mStr = new byte[Length];
						Array.Copy (mbyteStr, mStr, Length);
						mNetReceiveSystem.ReceiveSocketStream (mStr,0,mStr.Length);

						string Tag = "收到消息: " + Length + " | " + mStr.Length + " | " + receiveInfoPoolCapacity;
						DebugSystem.LogBitStream (Tag, mStr);
					}
				} catch (SocketException e) {
					DebugSystem.LogError ("Socket 错误： " + e.Message + " | " + e.SocketErrorCode);
					break;
				} catch (Exception e) {
					DebugSystem.LogError ("接受异常： " + e.Message + " | " + e.StackTrace);
					break;
				}
			}

			DebugSystem.LogError ("网络线程结束");
		}

		public override void SendNetStream (byte[] msg,int offset,int Length)
		{
			try {
				SocketError merror;
				mSocket.Send (msg, offset, Length, SocketFlags.None, out merror);
				string Tag = "";
				if (merror == SocketError.Success) {
					Tag = "发送成功:" + msg.Length;
				} else {
					Tag = "发送失败: " + merror;
				}
				DebugSystem.LogBitStream (Tag, msg);
			} catch (SocketException e) {
				DebugSystem.LogError (e.SocketErrorCode + " | " + e.Message);
			} catch (Exception e) {
				DebugSystem.LogError (e.Message);
			}
		}

		public override void CloseNet ()
		{
			mThread = null;
			if (mSocket != null) {
				mSocket.Close ();
				mSocket = null;
			}   
		}
	}

	public class SocketSystem_Async:SocketSystem
	{
		public int _sendTimeout = 3;
		public int _revTimeout = 3;

		private Socket mSocket = null;
		public SocketSystem_Async ()
		{

		}

		public override void Update ()
		{
			
		}

		public override void init (string ServerAddr, int ServerPort)
		{       
			if (mSocket != null && mSocket.Connected) {
				return;
			}
			try {
				mSocket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				mSocket.SendTimeout = _sendTimeout;
				mSocket.ReceiveTimeout = _revTimeout;

				IPEndPoint ipe = new IPEndPoint (IPAddress.Parse (ServerAddr), ServerPort);
				mSocket.BeginConnect (ipe, new System.AsyncCallback (ConnectionCallback), null);
			} catch (System.Exception e) {
				DebugSystem.LogError (e.Message);

			}
		}

		public override void SendNetStream (byte[] msg,int offset,int Length)
		{
			Send (msg);
		}
			
		void ConnectionCallback (System.IAsyncResult ar)
		{
			mSocket.EndConnect (ar);
			try {
				byte[] stream = new byte[4096];
				mSocket.BeginReceive (stream, 0, stream.Length, SocketFlags.None, new System.AsyncCallback (ReceiveInfo), stream);
			} catch (System.Exception e) {
				if (e.GetType () == typeof(SocketException)) {
					if (((SocketException)e).SocketErrorCode == SocketError.ConnectionRefused) {

					} else {

					}
				}
				Disconnect (0);
			}
		}

		// 接收消息体
		void ReceiveInfo (System.IAsyncResult ar)
		{
			byte[] stream = (byte[])ar.AsyncState;
			try {
				int read = mSocket.EndReceive (ar);

				// 用户已下线
				if (read < 1) {
					Disconnect (0);
					return;
				}

				mNetReceiveSystem.ReceiveSocketStream (stream,0,stream.Length);
				mSocket.BeginReceive (stream, 0, stream.Length, SocketFlags.None, new System.AsyncCallback (ReceiveInfo), stream);

			} catch (System.Exception e) {
				Disconnect (0);
			}
		}

		// 发送消息
		public void Send (byte[] bts)
		{
			if (!mSocket.Connected)
				return;

			NetworkStream ns;
			lock (mSocket) {
				ns = new NetworkStream (mSocket);
			}

			if (ns.CanWrite) {
				try {
					ns.BeginWrite (bts, 0, bts.Length, new System.AsyncCallback (SendCallback), ns);
				} catch (System.Exception) {
					Disconnect (0);
				}
			}
		}

		//发送回调
		private void SendCallback (System.IAsyncResult ar)
		{
			NetworkStream ns = (NetworkStream)ar.AsyncState;
			try {
				ns.EndWrite (ar);
				ns.Flush ();
				ns.Close ();
			} catch (System.Exception) {
				Disconnect (0);
			}

		}

		// 关闭连接
		public void Disconnect (int timeout)
		{
			if (mSocket.Connected) {
				mSocket.Shutdown (SocketShutdown.Receive);
				mSocket.Close (timeout);
			} else {
				mSocket.Close ();
			}

		}

		public override void CloseNet()
		{
			Disconnect (0);
		}
	}

	public class SocketSystem_SocketAsyncEventArgs:SocketSystem
	{
		SocketAsyncEventArgs ReceiveArgs;

		private Socket mSocket = null;
		public override void init (string ServerAddr, int ServerPort)
		{
			try {
				mSocket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				IPAddress mIPAddress = IPAddress.Parse (ServerAddr);
				IPEndPoint mIPEndPoint = new IPEndPoint (mIPAddress, ServerPort);
				mSocket.Connect (mIPEndPoint);
				ConnectServer ();
				DebugSystem.Log ("Client Net Init Success： IP: " + ServerAddr + " | Port: " + ServerPort);
			} catch (SocketException e) {
				DebugSystem.LogError ("客户端初始化失败：" + e.Message + " | " + e.SocketErrorCode);
			}
		}

		public override void Update ()
		{
			
		}

		private void ConnectServer ()
		{
			ReceiveArgs = new SocketAsyncEventArgs ();
			ReceiveArgs.Completed += Receive_Fun;
			ReceiveArgs.SetBuffer (new byte[receiveInfoPoolCapacity], 0, receiveInfoPoolCapacity);
			mSocket.ReceiveAsync (ReceiveArgs);
		}

		public override void SendNetStream (byte[] msg,int offset,int Length)
		{
			SocketError mError = SocketError.SocketError;
			try {
				mSocket.Send (msg, offset, Length, SocketFlags.None, out mError);
			} catch (Exception e) {
				DebugSystem.LogError ("发送字节失败： " + e.Message + " | " + mError.ToString ());
			}
		}

		private List<byte> mStorebyteList = new List<byte> ();

		private void Receive_Fun (object sender, SocketAsyncEventArgs e)
		{
			if (e.SocketError == SocketError.Success) {
				if (e.BytesTransferred > 0) {
					// DebugSystem.Log("接收数据个数： " + e.BytesTransferred);
					// DebugSystem.Log("接收数据个数： " + e.Buffer.Length);
					// DebugSystem.Log("接收数据个数： " + mSocket.Available);
					if (mSocket.Available > 0) {
						foreach (byte b in e.Buffer) {
							mStorebyteList.Add (b);
						}
						// DebugSystem.LogError("传输字节数超出缓冲数组: "+mSocket.Available);
					} else {
						byte[] mbyteArray = null;
						if (mStorebyteList.Count > 0) {
							for (int i = 0; i < e.BytesTransferred; i++) {
								mStorebyteList.Add (e.Buffer [i]);
							}
							mbyteArray = mStorebyteList.ToArray ();
							mStorebyteList.Clear ();
						} else {
							mbyteArray = new byte[e.BytesTransferred];
							Array.Copy (e.Buffer, mbyteArray, mbyteArray.Length);                         
						}
						mNetReceiveSystem.ReceiveSocketStream (mbyteArray,0,mbyteArray.Length);
					}

					mSocket.ReceiveAsync (e);
				}
			} else {
				DebugSystem.Log ("接收数据失败： " + e.SocketError.ToString ());
				CloseNet ();
			}
		}

		public override void CloseNet ()
		{
			if (mSocket != null) {
				mSocket.Close ();
				mSocket = null;
			}
		}
			
	}
}
