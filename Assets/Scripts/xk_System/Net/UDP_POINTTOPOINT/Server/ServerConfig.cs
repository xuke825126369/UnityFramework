using System.Collections;
using System.Collections.Generic;
using System;

namespace xk_System.Net.UDP.POINTTOPOINT.Server
{
	public class ServerConfig
	{
		public const bool IsLittleEndian = false;
		public const bool bNeedCheckPackage = true;

		public const int nUdpPackageFixedSize = 512;
		public const int nUdpPackageFixedHeadSize = 10;
		public const int nUdpPackageFixedBodySize = nUdpPackageFixedSize - nUdpPackageFixedHeadSize;
	}
}