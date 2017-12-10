using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using xk_System.Debug;
using UnityEngine;
using xk_System.Crypto;
using System.Collections;
using Google.Protobuf;
using game.protobuf.data;

namespace xk_System.Net.Client
{
	public class NetPackage:ObjectPoolInterface
	{
		public int command;
		public byte[] buffer;

		public void reset()
		{
			command = -1;
			buffer = null;
		}
	}
}
