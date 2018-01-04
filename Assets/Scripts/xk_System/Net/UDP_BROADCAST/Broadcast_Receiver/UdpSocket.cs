using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System;
using xk_System.Debug;
using System.Text;

namespace xk_System.Net.UDP.BROADCAST.Server
{
	public class UdpSockek_Basic :SocketReceivePeer
	{
		EndPoint bindEndPoint = null;
		EndPoint remoteEndPoint = null;
		private Socket mSocket = null;
		Thread mThread  = null;
		private int nServerPort = 0;

		public void InitNet (UInt16 ServerPort)
		{
			nServerPort = ServerPort;
			mSocket = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);//初始化一个Scoket实习,采用UDP传输

			IPEndPoint iep = new IPEndPoint (IPAddress.Any, ServerPort);//初始化一个发送广播和指定端口的网络端口实例
			bindEndPoint = (EndPoint)iep;
			mSocket.Bind (bindEndPoint);

			iep = new IPEndPoint (IPAddress.Broadcast, nServerPort);//初始化一个发送广播和指定端口的网络端口实例
			remoteEndPoint = (EndPoint)iep;

			mThread = new Thread (new ThreadStart (HandData));
			mThread.Start ();
		}

		byte[] data = new byte[ServerConfig.nMaxBufferSize];
		void HandData()
		{
			while (true) {
				int length = 0;
				try {
					
					length = mSocket.ReceiveFrom (data, 0, data.Length, SocketFlags.None, ref remoteEndPoint);

					if (length > 0) {
						ReceiveSocketStream (data, 0, length);
					}
				} catch (SocketException e) {
					DebugSystem.LogError ("UDP 广播接收器 SocketError： " + e.SocketErrorCode.ToString ());
					break;
				}
			}
		}

		public  void CloseNet ()
		{
			mThread.Abort ();
			mSocket.Close ();
		}

	}
}