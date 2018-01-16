using System.Collections;
using System.Collections.Generic;
using System;
using System.Collections.Concurrent;
using xk_System.Debug;

namespace xk_System.Net.UDP.POINTTOPOINT.Server
{
	public class ClientPeerManager:Singleton<ClientPeerManager>
	{
		private ConcurrentDictionary<UInt16, ClientPeer> mClientDic = null;

		public  ClientPeerManager()
		{
			mClientDic = new ConcurrentDictionary<ushort, ClientPeer> ();
		}

		public void Update(double elapsed)
		{
			if (elapsed > 0.1) {
				DebugSystem.LogError ("帧 时间 太长: " + elapsed);
			}

			lock (mClientDic) {
				if (!mClientDic.IsEmpty) {
					var iter = mClientDic.GetEnumerator ();
					while (iter.MoveNext ()) {
						iter.Current.Value.Update (elapsed);
					}
				}
			}
		}

		public bool IsExist(UInt16 port)
		{
			return mClientDic.ContainsKey (port);
		}

		public void AddClient(ClientPeer peer)
		{
			if (!mClientDic.TryAdd (peer.getPort (), peer)) {
				DebugSystem.LogError ("AddClient Error");
			}
		}

		public ClientPeer FindClient(UInt16 port)
		{
			ClientPeer peer = null;
			if (IsExist (port)) {
				peer = mClientDic [port];
				return peer;
			}

			DebugSystem.LogError ("ClientPeer 不存在");
			return null;
		}

		public bool RemoveClient(UInt16 port)
		{
			ClientPeer peer;
			return mClientDic.TryRemove (port, out peer);
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