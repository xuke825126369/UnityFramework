using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System;
using xk_System.Debug;
using xk_System.Net.UDP.Server.Event;

namespace xk_System.Net.UDP.Server
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

