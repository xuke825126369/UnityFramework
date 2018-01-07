using System.Collections;
using System.Collections.Generic;
using System;
using Google.Protobuf;

namespace xk_System.Net.UDP.POINTTOPOINT.Client
{
	public class NetPackage
	{
		public UInt16 nOrderId;
		public UInt16 nGroupCount;
		public UInt16 nPackageId;

		public byte[] buffer;
		public int Length;

		public NetPackage()
		{
			nOrderId = 0;
			nGroupCount = 0;
			nPackageId = 0;

			buffer = null;
			Length = 0;
		}
	}

	public class NetUdpFixedSizePackage : NetPackage
	{
		public NetUdpFixedSizePackage()
		{
			buffer = new byte[ClientConfig.nUdpPackageFixedSize];
		}
	}

	public class NetCombinePackage : NetPackage
	{
		public NetCombinePackage()
		{
			buffer = new byte[ClientConfig.nUdpPackageFixedSize];
		}			
	}
}

