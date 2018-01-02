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
	public class SocketSystem_UdpServer :SocketReceivePeer
	{
		EndPoint ep = null;
		private Socket mSocket = null;
		Thread mThread  = null;
		private int nServerPort = 0;

		public void InitNet (UInt16 ServerPort)
		{
			nServerPort = ServerPort;
			mSocket = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);//初始化一个Scoket实习,采用UDP传输

			IPEndPoint iep = new IPEndPoint (IPAddress.Any, ServerPort);//初始化一个发送广播和指定端口的网络端口实例
			ep = (EndPoint)iep;

			mSocket.Bind (ep);
			mThread = new Thread (new ThreadStart (HandData));
			mThread.Start ();
		}

		byte[] data = new byte[ServerConfig.nMaxBufferSize];
		void HandData()
		{
			while (true) {
				int length = 0;
				try {
					
					IPEndPoint iep = new IPEndPoint (IPAddress.Any, nServerPort);//初始化一个发送广播和指定端口的网络端口实例
					var client = (EndPoint)iep;
					length = mSocket.ReceiveFrom (data, 0, data.Length, SocketFlags.None, ref client);
					DebugSystem.Log ("length:" + length);
					IPEndPoint remotePoint = client as IPEndPoint;
					string remoteIpstr = remotePoint.ToString ();
					
					DebugSystem.Log ("远程IP: " + remoteIpstr + " | " + remotePoint.Port);
					if (length > 0) {
						ReceiveSocketStream (data, 0, length);
					}
				} catch (Exception e) {
					DebugSystem.LogError ("UDP 服务器 接受 异常： " + length + " | " + e.Message);
					break;
				}
			}
		}

		public  void CloseNet ()
		{
			mSocket.Close ();
			mThread.Abort ();
		}

	}
}