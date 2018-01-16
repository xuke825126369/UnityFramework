using System.Collections;
using System.Collections.Generic;
using System;

namespace xk_System.Net.UDP.POINTTOPOINT.Server
{
	public class ServerConfig
	{
		//Byte stream 
		public const bool IsLittleEndian = false;

		//or Need Check Package
		public const bool bNeedCheckPackage = true;

		//Udp Package OrderId
		public const UInt16 nUdpMinOrderId = 1;
		public const UInt16 nUdpMaxOrderId = UInt16.MaxValue;

		//PackageSize
		public const int nUdpCombinePackageFixedSize = 4096;
		public const int nUdpPackageFixedSize = 512;
		public const int nUdpPackageFixedHeadSize = 10;
		public const int nUdpPackageFixedBodySize = nUdpPackageFixedSize - nUdpPackageFixedHeadSize;

		//Frame Handle Package Count
		public const int FrameHandlePackageCount = 100;
		public const double FrameSpendMaxTime = 0.3;
	}
}