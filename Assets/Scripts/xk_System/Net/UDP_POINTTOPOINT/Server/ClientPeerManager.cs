using System.Collections;
using System.Collections.Generic;
using System;

namespace xk_System.Net.UDP.POINTTOPOINT.Server
{
	public class ClientPeerManager:Singleton<ClientPeerManager>
	{
		Dictionary<UInt16, ClientPeer> mClientDic = null;

		public  ClientPeerManager()
		{
			mClientDic = new Dictionary<ushort, ClientPeer> ();
		}

		public void Update(double elapsed)
		{


		}

		public void AddClient(UInt16 port, ClientPeer peer)
		{
			mClientDic [port] = peer;
		}

		public void RemoveClient(UInt16 port)
		{
			mClientDic.Remove (port);
		}

		public void Broadcast(UInt16 id, object data)
		{
			var iter = mClientDic.GetEnumerator ();
			while (iter.MoveNext ()) {
				iter.Current.Value.SendNetData (id, data);
			}
		}

		public void Broadcast(List<UInt16> clientIds, UInt16 id, object data)
		{
			var iter = clientIds.GetEnumerator ();
			while (iter.MoveNext ()) {
				mClientDic [iter.Current].SendNetData (id, data);
			}
		}
		
	}
}