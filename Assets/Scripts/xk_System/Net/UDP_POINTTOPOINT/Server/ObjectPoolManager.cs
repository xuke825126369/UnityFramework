using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace xk_System.Net.UDP.POINTTOPOINT.Server
{
	public class ObjectPoolManager:Singleton<ObjectPoolManager>
	{
		public SafeObjectPool<NetUdpFixedSizePackage> mUdpFixedSizePackagePool;
		public SafeObjectPool<NetCombinePackage> mCombinePackagePool;
		public SafeObjectPool<ClientPeer> mClientPeerPool;

		public ObjectPoolManager()
		{
			mUdpFixedSizePackagePool = new SafeObjectPool<NetUdpFixedSizePackage> ();
			mCombinePackagePool = new SafeObjectPool<NetCombinePackage> ();
			mClientPeerPool = new SafeObjectPool<ClientPeer> ();
		}

	}
}