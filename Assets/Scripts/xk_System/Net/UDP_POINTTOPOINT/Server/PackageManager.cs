using System.Collections;
using System.Collections.Generic;
using System;
using System.Collections.Concurrent;
using xk_System.Debug;
using UdpPointtopointProtocols;
using xk_System.Net.UDP.POINTTOPOINT.Protocol;

namespace xk_System.Net.UDP.POINTTOPOINT.Server
{
	public class PackageManager : Singleton<PackageManager>
	{
		private Dictionary<UInt16, Action<ClientPeer, NetPackage>> mNetEventDic = null;
		public PackageManager()
		{
			mNetEventDic = new Dictionary<ushort, Action<ClientPeer, NetPackage>> ();

			this.InitUdpSystemCommand ();
		}

		private void InitUdpSystemCommand()
		{
			addNetListenFun (UdpNetCommand.COMMAND_PACKAGECHECK, ReceiveUdpCheckPackage);
			addNetListenFun (UdpNetCommand.COMMAND_HEARTBEAT, ReceiveServerHeartBeat);
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

		private void ReceiveServerHeartBeat(ClientPeer peer,NetPackage mPackage)
		{
			peer.ReceiveUdpClientHeart (mPackage);
		}

		private void ReceiveUdpCheckPackage(ClientPeer peer,NetPackage mPackage)
		{
			peer.ReceiveUdpCheckPackage (mPackage);
		}
	}

}