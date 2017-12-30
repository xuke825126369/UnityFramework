using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace xk_System.Net.UDP.Client
{
	public enum NetState
	{
		none = 0,

		connecting = 1,
		connected = 2,

		disconnecting = 4,
		disconnected = 5,
	}

	public enum NetSendState
	{
		sending = 7,
		sended_success = 8,
		sended_failed = 9
	}

	public enum NetReceiveState
	{
		receiveing = 10,
		received_success = 11,
		received_failed = 12,
	}

	public class packageEvent
	{
		int packid;
	}
}