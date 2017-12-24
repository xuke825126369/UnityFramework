using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace xk_System.Net.TCP.Server
{
	public static class NetPackageIdentifer
	{
		public static int getId(int clientId, int command)
		{
			return command * 100000 + clientId;
		}

		public static int getCommand(int Id)
		{
			return Id / 100000;
		}

		public static int getClientId(int Id)
		{
			return Id % 1000000;
		}
	}
}
