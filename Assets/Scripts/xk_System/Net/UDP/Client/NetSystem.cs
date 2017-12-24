using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.Debug;
using System.Net.Sockets;
using System;
using xk_System.DataStructure;
using xk_System.Event;
using xk_System.Net.UDP.Client.Event;

namespace xk_System.Net.UDP.Client
{
	public class NetSystem:SocketSystem_Udp, NetEventInterface
	{
		public void initNet (string ServerAddr, int ServerPort)
		{
			base.InitNet (ServerAddr, ServerPort);
		}

		public void sendNetData (int command, byte[] buffer)
		{
			base.mNetSendSystem.SendNetData (command, buffer);  
		}

		public void handleNetData ()
		{
			base.HandleNetPackage ();
		}

		public void addNetListenFun (Action<NetPackage> fun)
		{
			base.mNetReceiveSystem.addListenFun (fun);
		}

		public void removeNetListenFun (Action<NetPackage> fun)
		{
			base.mNetReceiveSystem.removeListenFun (fun);
		}

		public void closeNet ()
		{
			base.CloseNet ();
		}
	}

	public class SocketConfig
	{
		protected const int receiveInfoPoolCapacity = 10;
		protected const int sendInfoPoolCapacity = 10;
		protected const int receiveTimeOut = 5000;
		protected const int sendTimeOut = 5000;

		public virtual void ConfigureSocket (Socket mSocket)
		{
			mSocket.ExclusiveAddressUse = true;
			mSocket.LingerState = new LingerOption (true, 10);
			mSocket.NoDelay = false;

			mSocket.ReceiveBufferSize = 8192;
			mSocket.ReceiveTimeout = 1000;
			mSocket.SendBufferSize = 8192;
			mSocket.SendTimeout = 1000;

			mSocket.Blocking = false;
			mSocket.Ttl = 42;
		}

		public void PrintSocketConfigInfo (Socket mSocket)
		{
			DebugSystem.Log ("------------------- Socket Config ------------------------ ");
			DebugSystem.Log ("ExclusiveAddressUse :" + mSocket.ExclusiveAddressUse);
			DebugSystem.Log ("LingerState: " + mSocket.LingerState.Enabled + " | " + mSocket.LingerState.LingerTime);
			DebugSystem.Log ("Ttl: " + mSocket.Ttl);
			DebugSystem.Log ("NoDelay: " + mSocket.NoDelay);

			DebugSystem.Log ("Block: " + mSocket.Blocking);
			DebugSystem.Log ("ReceiveTimeout: " + mSocket.ReceiveTimeout);
			DebugSystem.Log ("SendTimeout: " + mSocket.SendTimeout);

			DebugSystem.Log ("ReceiveBufferSize: " + mSocket.ReceiveBufferSize);
			DebugSystem.Log ("SendBufferSize: " + mSocket.SendBufferSize);
			DebugSystem.Log ("---------------- Finish -------------------");
		}

		public void PrintSocketState(Socket mSocket)
		{
			DebugSystem.Log ("------------------- Socket State ------------------------ ");
			DebugSystem.Log ("IsBound: " + mSocket.IsBound);
			DebugSystem.Log ("Connected: " + mSocket.Connected);
			DebugSystem.Log ("---------------- Finish -------------------");
		}

	}

	public class SocketSystem :SocketConfig
	{
		public NetSendSystemInterface mNetSendSystem;
		public NetReceiveSystemInterface mNetReceiveSystem;

		public virtual void InitNet (string ServerAddr, int ServerPort)
		{

		}

		public virtual void SendNetStream (byte[] msg,int offset,int Length)
		{

		}

		public virtual void HandleNetPackage()
		{
			mNetSendSystem.HandleNetPackage ();
			mNetReceiveSystem.HandleNetPackage ();
		}

		public virtual void CloseNet()
		{
			mNetSendSystem.release ();
			mNetReceiveSystem.release ();
		}
	}

	//begin~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~网络发送系统~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	public interface  NetSendSystemInterface
	{
		void SendNetData (int id, byte[] buffer);
		void HandleNetPackage ();
		void release ();
	}

	public interface NetReceiveSystemInterface
	{
		void addListenFun (Action<NetPackage> fun);
		void removeListenFun (Action<NetPackage> fun);

		bool isCanReceiveFromSocketStream ();
		void ReceiveSocketStream (byte[] data, int index, int Length);
		void HandleNetPackage ();
		void release ();
	}
}


