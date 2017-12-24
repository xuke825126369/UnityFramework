using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;
using xk_System.Debug;
using System.Text;
using System;
using System.Threading;

namespace xk_System.Net.UDP.Client
{
	public class SocketSystem_Udp
	{
		private EndPoint ep;
		private Socket mSocket = null;
		Thread mThread = null;

		public SocketPeer mSocketPeer;

		public SocketSystem_Udp()
		{
			mSocketPeer = new SocketPeer (this);
		}

		public void InitNet (int ServerPort)
		{
			mSocket = new Socket (AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);//初始化一个Scoket协议

			IPEndPoint iep = new IPEndPoint (IPAddress.Parse ("192.168.122.24"), ServerPort);//初始化一个侦听局域网内部所有IP和指定端口
			ep = (EndPoint)iep;

			mThread = new Thread (HandData);
			mThread.Start ();
		}

		byte[] data = new byte[ClientConfig.nMaxBufferSize];
		void HandData()
		{
			while (true) {
				int length = 0;
				try {
					length = mSocket.ReceiveFrom (data, ref ep);
					if (length > 0) {
						mSocketPeer.ReceiveSocketStream (data, 0, data.Length);
					}
				} catch (Exception e) {
					DebugSystem.LogError ("UDP 客户端 接受 异常： " + length);
					break;
				}

				Thread.Sleep (10);
			}
		}

		public void SendNetStream (byte[] msg,int offset,int Length)
		{
			mSocket.SendTo (msg, offset, Length, SocketFlags.None, ep);
		}

		public void CloseNet ()
		{
			if (mSocket != null) {
				mSocket.Close ();
				mSocket = null;
			}

			mThread.Abort ();
		}
	}
}