using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;

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

	public class Client
	{
		private int Id;
		private Socket clientSocekt;

		public Client(Socket clientSocekt)
		{
			this.Id = IdManager.Instance.allot ();
			this.clientSocekt = clientSocekt;
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
	}
}

