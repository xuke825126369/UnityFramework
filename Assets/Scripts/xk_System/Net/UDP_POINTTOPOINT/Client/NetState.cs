using System.Collections;
using System.Collections.Generic;

namespace xk_System.Net.UDP.POINTTOPOINT.Client
{
	public enum UDPTYPE
	{
		POINTTOPOINT = 1,
		BROADCAST = 2,
	}

	public enum NETSTATE
	{
		CONNECTING = 1,
		CONNECTED = 2,

		DISCONNECTING = 3,
		DISCONNECTED = 4,

		EXCEPTION = 5,
	}

	public enum NETEVENT
	{
		CONNECTING = 1,
		CONNECTED = 2,

		DISCONNECTING = 3,
		DISCONNECTED = 4,

		EXCEPTION = 5,
	}

	public struct peer_event
	{
		public NETEVENT mNetEventType;
		public string msg;
	}




}