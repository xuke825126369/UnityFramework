using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace xk_System.Net.UDP.POINTTOPOINT.Client
{
	public class ObjectPoolManager : Singleton<ObjectPoolManager>
	{
		public SafeObjectPool<NetUdpFixedSizePackage> mUdpFixedSizePackagePool;
		public SafeObjectPool<NetCombinePackage> mCombinePackagePool;

		public ObjectPoolManager()
		{
			mUdpFixedSizePackagePool = new SafeObjectPool<NetUdpFixedSizePackage> ();
			mCombinePackagePool = new SafeObjectPool<NetCombinePackage> ();
		}
	}
}