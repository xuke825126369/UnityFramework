using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using xk_System.Debug;
using System.Text;
using System;
using System.Threading;


namespace xk_System.Net.Client.Udp
{
	class SocketSystem_Udp:SocketSystem
	{
		private EndPoint ep;
		public override void init (string ServerAddr, int ServerPort)
		{
			mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);//初始化一个Scoket协议

			IPEndPoint iep = new IPEndPoint(IPAddress.Parse(ServerAddr), ServerPort);//初始化一个侦听局域网内部所有IP和指定端口
			ep = (EndPoint)iep;

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
					if (length>0)
					{
						mNetReceiveSystem.ReceiveSocketStream(data);
					}
				}catch(Exception e)
				{
					DebugSystem.LogError ("UDP 客户端 接受 异常： " + length);
					break;
				}
			}
		}

		public override void Update ()
		{
			
		}

		public override void SendNetStream (byte[] msg)
		{
			mSocket.SendTo (msg, ep);
		}
	}

}