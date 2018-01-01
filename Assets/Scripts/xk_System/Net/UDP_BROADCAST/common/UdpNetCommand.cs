using System.Collections;
using System.Collections.Generic;
using System;

namespace xk_System.Net.UDP.BROADCAST.Protocol
{
	public static class UdpNetCommand
	{
		public const UInt16 COMMAND_UDPLikeTCP = 1;
		public const UInt16 COMMAND_PACKAGECHECK = 2;
		public const UInt16 COMMAND_SCBROADCASTIP = 3;
		public const UInt16 COMMAND_HEARTBEAT = 4;

		public const UInt16 COMMAND_TESTCHAT = 5;
	}
}
