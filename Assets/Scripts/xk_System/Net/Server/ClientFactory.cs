using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System;
using xk_System.Debug;

namespace xk_System.Net.Server
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

	public class Client : NetSystem_SocketAsyncEventArgs
	{
		public Client()
		{

		}

	}
}

