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
	//Poll
	public class SocketSystem_Poll : SocketSystem
	{
		private Socket mSocket = null;
		byte[] mReceiveStream = null;

		public SocketSystem_Poll()
		{
			mNetSendSystem = new NetSendSystem (this);
			mNetReceiveSystem = new NetNoLockReceiveSystem (this);
			mReceiveStream = new byte[ClientConfig.receiveBufferSize];
		}

		public override void InitNet (string ServerAddr, int ServerPort)
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
			if (mSocket.Poll (0, SelectMode.SelectWrite)) {
				//DebugSystem.Log ("This Socket is writable.");
			} 

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
			mNetReceiveSystem.ReceiveSocketStream (mReceiveStream, 0, Length);
		}

		private void ProcessExcept ()
		{
			//DebugSystem.LogError ("Client SocketExcept");
			this.mSocket.Close ();
			this.mSocket = null;
		}

		public override void HandleNetPackage ()
		{
			Poll ();
			base.HandleNetPackage ();
		}

		public override void SendNetStream (byte[] msg,int offset,int Length)
		{
			try {
				SocketError merror;
				mSocket.Send (msg, offset, Length, SocketFlags.None, out merror);
				if (merror != SocketError.Success) {
					if (mSocket.Blocking == false && merror == SocketError.WouldBlock) {
						SendNetStream (msg, offset, Length);
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

	public class SocketSystem_Basic : SocketSystem
	{
		Thread mThread = null;
		protected Socket mSocket = null;
		private byte[] mReceiveStream = null;

		public SocketSystem_Basic()
		{
			mNetSendSystem = new NetSendSystem (this);
			mNetReceiveSystem = new NetLockReceiveSystem (this);
			mReceiveStream = new byte[ClientConfig.receiveBufferSize];
		}

		public override void InitNet (string ServerAddr, int ServerPort)
		{
			try {
				IPEndPoint mIPEndPoint = new IPEndPoint (IPAddress.Parse (ServerAddr), ServerPort);
				mSocket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				mSocket.Connect (mIPEndPoint);
				mSocket.Blocking = false;

				NewStartThread_Receive ();
				DebugSystem.Log ("Client Net InitNet Success： IP: " + ServerAddr + " | Port: " + ServerPort);
			} catch (SocketException e) {
				DebugSystem.LogError (e.SocketErrorCode + " | " + e.Message);
			} catch (Exception e) {
				DebugSystem.LogError ("客户端初始化失败：" + e.Message);
			}
		}

		private void NewStartThread_Receive ()
		{
			mThread = new Thread (Receive);
			mThread.IsBackground = false;
			mThread.Start ();
		}

		private void Receive ()
		{
			while (mSocket != null) {
				Thread.Sleep (3);
				try {
					SocketError error;
					int Length = 0;
					if (mSocket == null)
					{
						break;
					}
					lock(mSocket)
					{
						Length = mSocket.Receive (mReceiveStream, 0, mReceiveStream.Length, SocketFlags.None, out error);
					}

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
						mNetReceiveSystem.ReceiveSocketStream (mReceiveStream, 0, Length);
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
				if (merror != SocketError.Success) {
					if (mSocket.Blocking == false && merror == SocketError.WouldBlock) {
						SendNetStream (msg, offset, Length);
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
			mThread.Abort ();
			if (mSocket != null) {
				mSocket.Close ();
				mSocket = null;
			}   
		}
	}

	public class SocketSystem_SocketAsyncEventArgs:SocketSystem
	{
		SocketAsyncEventArgs ReceiveArgs;
		private Socket mSocket = null;
		public SocketSystem_SocketAsyncEventArgs()
		{
			mNetSendSystem = new NetSendSystem (this);
			mNetReceiveSystem = new NetLockReceiveSystem (this);
		}

		public override void InitNet (string ServerAddr, int ServerPort)
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
			ReceiveArgs.SetBuffer (new byte[ClientConfig.receiveBufferSize], 0, ClientConfig.receiveBufferSize);
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

		private void Receive_Fun (object sender, SocketAsyncEventArgs e)
		{
			if (e.SocketError == SocketError.Success) {
				if (e.BytesTransferred > 0) {
					mNetReceiveSystem.ReceiveSocketStream (e.Buffer, 0, e.BytesTransferred);
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
