using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace xk_System.Net.UDP.Client
{
	public class NetPackage:ObjectPoolInterface
	{
		public int command;
		public byte[] buffer = null;
		public void reset()
		{
			command = -1;
			buffer = null;
		}
	}
}