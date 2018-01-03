using System.Collections;
using System.Collections.Generic;
using System;
using Google.Protobuf;

namespace xk_System.Net.UDP.BROADCAST.Client
{
	public class NetReceivePackage
	{
		public UInt16 nUniqueId;
		public ArraySegment<byte> buffer;
		private static BufferManager mBufferManager = null;

		public NetReceivePackage()
		{
			if (mBufferManager == null) {
				mBufferManager = new BufferManager (ClientConfig.nMaxBufferSize * 1024, ClientConfig.nMaxBufferSize);
			}
			mBufferManager.SetBuffer (out buffer);
		}
	}

	public class NetSendPackage
	{
		public UInt16 nPackageId;
		public object message;
	}

}