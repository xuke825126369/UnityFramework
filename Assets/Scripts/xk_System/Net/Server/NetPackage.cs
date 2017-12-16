using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace xk_System.Net.Server
{
	public class NetPackage:ObjectPoolInterface
	{
		public int clientId;
		public int command;
		private int realLength = 0;
		public byte[] buffer = new byte [ServerConfig.nMaxPackageSize];

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

	public class ClientNetBuffer
	{
		private int clientId;
		private byte[] buffer;
		private int dataLength;

		public int ClientId {
			get {
				return clientId;
			}
		}

		public int Length {
			get {
				return dataLength;
			}
		}

		public byte[] Buffer {
			get {
				return buffer;
			}
		}

		public void WriteFrom(int clientId, byte[] otherBuffer,int offset,int count)
		{
			this.clientId = clientId;
			if (count > dataLength) {
				buffer = new byte[count];
			}

			Array.Copy (otherBuffer, offset, buffer, 0, count);
			dataLength = count;
		}
	}
}