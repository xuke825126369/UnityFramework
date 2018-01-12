using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace xk_System.Net.UDP.POINTTOPOINT.Server
{
	public class ObjectPoolManager:Singleton<ObjectPoolManager>
	{
		public ObjectPool<NetUdpFixedSizePackage> mUdpFixedSizePackagePool;
		public ObjectPool<NetCombinePackage> mCombinePackagePool;

		public ObjectPoolManager()
		{
			mUdpFixedSizePackagePool = new ObjectPool<NetUdpFixedSizePackage> ();
			mCombinePackagePool = new ObjectPool<NetCombinePackage> ();
		}
	}
}