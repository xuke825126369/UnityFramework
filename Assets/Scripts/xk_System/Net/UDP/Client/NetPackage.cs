using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Google.Protobuf;

namespace xk_System.Net.UDP.Client
{
	public class NetPackage:ObjectPoolInterface
	{
		public ulong orderId;
		public int command;
		public byte[] buffer = null;
		public virtual void reset()
		{
			command = -1;
			buffer = null;
		}
	}

	public class NetProtobufPackage : NetPackage
	{
		public IMessage msg;

		public override void reset()
		{
			msg = null;
		}
	}
}