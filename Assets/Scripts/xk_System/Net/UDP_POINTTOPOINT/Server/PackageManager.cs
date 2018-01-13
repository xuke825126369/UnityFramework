using System.Collections;
using System.Collections.Generic;
using System;
using System.Collections.Concurrent;
using xk_System.Debug;

namespace xk_System.Net.UDP.POINTTOPOINT.Server
{
	public class PackageManager : Singleton<PackageManager>
	{
		private ConcurrentQueue<NetUdpFixedSizePackage> mReceivePackageQueue = null;
		private Dictionary<UInt16, Action<ClientPeer, NetPackage>> mNetEventDic = null;

		public PackageManager()
		{
			mReceivePackageQueue = new ConcurrentQueue<NetUdpFixedSizePackage> ();
			mNetEventDic = new Dictionary<ushort, Action<ClientPeer, NetPackage>> ();
		}

		public void UpdateToClient()
		{
			
		}

		public void Execute(ClientPeer peer,NetPackage mPackage)
		{
			if (mNetEventDic.ContainsKey (mPackage.nPackageId)) {
				mNetEventDic [mPackage.nPackageId] (peer, mPackage);
			} else {
				DebugSystem.LogError ("不存在的包Id: " + mPackage.nPackageId);
			}
		}

		public void addNetListenFun(UInt16 id, Action<ClientPeer, NetPackage> func)
		{
			if (!mNetEventDic.ContainsKey (id)) {
				mNetEventDic [id] = func;
			} else {
				mNetEventDic [id] += func;
			}
		}
	}

}