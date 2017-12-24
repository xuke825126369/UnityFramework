using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System;
using xk_System.Debug;

namespace xk_System.Net.TCP.Server
{
	public class IdManager:Singleton<IdManager>
	{
		Queue<int> mIdQueue = new Queue<int> ();
		private int maxId = 0;

		public int allot()
		{
			int tempId = -1;
			if (mIdQueue.Count == 0) {
				tempId = maxId++;
			} else {
				tempId = mIdQueue.Dequeue ();
			}
			return tempId;
		}

		public void recycle(int id)
		{
			mIdQueue.Enqueue (id);
		}
	}

	public class ClientFactory:Singleton<ClientFactory>
	{
		public Dictionary<int,Client> mClientPool = new Dictionary<int, Client> ();

		public void AddClient(Client client)
		{
			mClientPool[client.getId()] = client;
		}
	
		public void DeleteClient(int clientId)
		{
			mClientPool.Remove (clientId);
		}

		public Client GetClient(int clientId)
		{
			return mClientPool [clientId];
		}
	}

	public class SocketAsyncEventArgs_Token
	{
		private int Id;
		private Socket clientSocekt;
		private SocketAsyncEventArgs mSend_SocketAsyncEventArgs = null;

		public void init(Socket clientSocekt, SocketAsyncEventArgs send)
		{
			this.Id = IdManager.Instance.allot ();
			this.clientSocekt = clientSocekt;
			mSend_SocketAsyncEventArgs = send;
		}

		public Socket getSocket()
		{
			return clientSocekt;
		}

		public int getId()
		{
			return Id;
		}

		public void recycle()
		{
			IdManager.Instance.recycle (this.Id);
			clientSocekt.Close ();
			clientSocekt = null;
		}

		public void SendNetStream(byte[] msg,int offset,int Length)
		{
			if (Length > ServerConfig.nMaxBufferSize) {
				DebugSystem.LogError ("发送Buffer 超过最大尺寸");
				return;
			}
			Array.Copy (msg, offset, mSend_SocketAsyncEventArgs.Buffer, mSend_SocketAsyncEventArgs.Offset, Length);
			mSend_SocketAsyncEventArgs.SetBuffer (mSend_SocketAsyncEventArgs.Offset, Length);
			clientSocekt.SendAsync (mSend_SocketAsyncEventArgs);
		}
	}

	public class ClientFactory_Select:Singleton<ClientFactory_Select>
	{
		public Dictionary<int,Client_Select> mClientPool = new Dictionary<int, Client_Select> ();

		public void AddClient(Client_Select client)
		{
			mClientPool[client.getId()] = client;
		}

		public void DeleteClient(int clientId)
		{
			mClientPool.Remove (clientId);
		}

		public Client_Select GetClient(int clientId)
		{
			return mClientPool [clientId];
		}
	}

	public class Select_Token
	{
		private int Id;
		private Socket clientSocekt;
		public ArraySegment<byte> mReceiveStream;
		public ArraySegment<byte> mSendStream;

		public void init(Socket clientSocekt,ArraySegment<byte> mSendBuffer,ArraySegment<byte> mReceiveBuffer)
		{
			this.Id = IdManager.Instance.allot ();
			this.clientSocekt = clientSocekt;
			clientSocekt.Blocking = false;
			mReceiveStream = mReceiveBuffer;
			mSendStream = mSendBuffer;
		}

		public Socket getSocket()
		{
			return clientSocekt;
		}

		public int getId()
		{
			return Id;
		}

		public void recycle()
		{
			IdManager.Instance.recycle (this.Id);
			clientSocekt.Close ();
			clientSocekt = null;
		}

		public void SendNetStream (byte[] msg, int index, int Length)
		{
			try {
				SocketError merror;
				int sendLength = clientSocekt.Send (msg, index, Length, SocketFlags.None, out merror);
				if (sendLength != Length) {
					DebugSystem.LogError ("Client:SendLength:  " + sendLength + " | " + Length);
				}
				if (merror != SocketError.Success) {
					if (clientSocekt.Blocking == false && merror == SocketError.WouldBlock) {
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

		public void ProcessInput ()
		{
			SocketError error;
			int Length = clientSocekt.Receive (mReceiveStream.Array, mReceiveStream.Offset, mReceiveStream.Count, SocketFlags.None, out error);
			if (error == SocketError.Success) {
				((SocketPeer_Select)this).ReceiveSocketStream (mReceiveStream.Array, mReceiveStream.Offset, Length);
			} else {
				DebugSystem.LogError (error.ToString ());
			}
		}
		
	}


	public class Client : SocketPeer_SocketAsyncEventArgs
	{
		public Client()
		{
			
		}

		public void Update()
		{
			HandleNetPackage ();
		}

		public void closeNet ()
		{
			
		}
	}

	public class Client_Select : SocketPeer_Select
	{
		public Client_Select()
		{

		}

		public void Update()
		{
			
		}

		public void closeNet ()
		{

		}
	}
}

