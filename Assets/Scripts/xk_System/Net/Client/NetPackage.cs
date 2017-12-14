using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace xk_System.Net.Client
{
	public class NetPackage:ObjectPoolInterface
	{
		public int command;
		private int realLength = 0;
		public byte[] buffer = new byte [1024];

		public int Length {
			set {
				if (value > buffer.Length) {
					int tempLength = buffer.Length * (value / buffer.Length + 1);
					buffer = new byte[tempLength];
				}

				realLength = value;
			}

			get {
				return realLength;
			}
		}

		public void reset()
		{
			command = -1;
			Length = 0;
		}
	}
}