using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace xk_System.Net.Server
{
	public class NetObjectPool:Singleton<NetObjectPool>
	{
		public ArrayGCPool<byte> mStreamPool = new ArrayGCPool<byte>();
		public ObjectPool<NetPackage> mNetPackagePool = new ObjectPool<NetPackage> ();
	}

	public class NetPackage:ObjectPoolInterface
	{
		public int socketId;
		public int command;
		public byte[] buffer;

		public void reset()
		{
			socketId = -1;
			command = -1;
			buffer = null;
		}
	}
}