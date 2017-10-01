using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;

namespace xk_System.Net.Client
{
	public class NetUdpClient : MonoBehaviour
	{
		// Use this for initialization
		void Start () 
		{
			
		}
	
		// Update is called once per frame
		void Update () 
		{
			
		}
	}

	class UdpClient
	{
		static void Main(string[] args)
		{
			Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);//初始化一个Scoket协议

			IPEndPoint iep = new IPEndPoint(IPAddress.Any, 9095);//初始化一个侦听局域网内部所有IP和指定端口

			EndPoint ep = (EndPoint)iep;

			socket.Bind(iep);//绑定这个实例

			while (true)
			{
				byte[] buffer = new byte[1024];//设置缓冲数据流

				socket.ReceiveFrom(buffer, ref ep);//接收数据,并确把数据设置到缓冲流里面

				//sConsole.WriteLine(Encoding.Unicode.GetString(buffer2).TrimEnd('/u0000') + " " + DateTime.Now.ToString());
			}
		}
	}

}