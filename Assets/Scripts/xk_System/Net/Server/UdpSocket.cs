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
	public class SocketSystem_UdpServer 
	{
		EndPoint ep = null;
		private Socket mSocket = null;
		public void InitNet (string ServerAddr, int ServerPort)
		{
			mSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);//初始化一个Scoket实习,采用UDP传输

			IPEndPoint iep = new IPEndPoint(IPAddress.Any, ServerPort);//初始化一个发送广播和指定端口的网络端口实例
			ep = (EndPoint)iep;

			//mSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);//设置该scoket实例的发送形式

			mSocket.Bind (iep);
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
					DebugSystem.Log("length:"+length);
					IPEndPoint remotePoint = ep as IPEndPoint;
					string remoteIpstr = remotePoint.Address.ToString();
					DebugSystem.Log("远程IP: "+remoteIpstr);
					if (length > 0)
					{
						//mNetReceiveSystem.ReceiveSocketStream(data,0,data.Length);
					}
				}catch(Exception e)
				{
					DebugSystem.LogError ("UDP 服务器 接受 异常： " + length);
					break;
				}
			}
		}

		public void SendNetStream (int clientId,byte[] msg,int offset, int count)
		{
			//mSocket.SendTo (msg, ep);
		}

		public  void CloseNet ()
		{
			
		}
	}
}