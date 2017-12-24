using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace xk_System.Net.UDP.Server
{
	public class NetPackage:ObjectPoolInterface
	{
		public int clientId;
		public int command;
		public byte[] buffer = null;

		public void reset()
		{
			command = -1;
			buffer = null;
		}
	}

	public class ClientNetBuffer: ObjectPoolInterface
	{
		private int clientId;
		private byte[] buffer;
		private int dataLength;

		public void reset()
		{
			clientId = -1;
			dataLength = 0;
		}

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