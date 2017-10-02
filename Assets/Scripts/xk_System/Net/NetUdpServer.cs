using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System;
using xk_System.Debug;
using System.Text;

namespace xk_System.Net.Server
{
	public class NetUdpServer : MonoBehaviour 
	{
		private void Start()
		{

		}

		private void Update()
		{

		}
	}
		
	public class UdpServer:SocketSystem
	{
		EndPoint ep = null;

		public override void init (string ServerAddr, int ServerPort)
		{
			mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);//初始化一个Scoket实习,采用UDP传输

			IPEndPoint iep = new IPEndPoint(IPAddress.Broadcast, ServerPort);//初始化一个发送广播和指定端口的网络端口实例
			ep = (EndPoint)iep;

			mSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);//设置该scoket实例的发送形式

			string request = "你好,TEST SEND!";//初始化需要发送而的发送数据

			byte[] buffer = Encoding.Unicode.GetBytes(request);

			mSocket.SendTo(buffer, iep);

			mSocket.Close();

			Thread mThread = new Thread (new ThreadStart(HandData));
			mThread.Start ();
		}

		void HandData()
		{
			while (true) 
			{
				byte[] data = new byte[1024];
				int length = 0;
				try
				{
					length = mSocket.ReceiveFrom (data, ref ep);
				}catch(Exception e)
				{
					DebugSystem.LogError ("UDP 接受 异常： " + length);
					break;
				}
			}
		}

		public override void SendNetStream (byte[] msg)
		{
			
		}

		public override void CloseNet ()
		{
			base.CloseNet ();
		}
	}
}