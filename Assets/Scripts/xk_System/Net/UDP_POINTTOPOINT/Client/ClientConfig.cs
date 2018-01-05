using System.Collections;
using System.Collections.Generic;
using System;

namespace xk_System.Net.UDP.POINTTOPOINT.Client
{
	public class ClientConfig
	{
		public const int nMaxBufferSize = 1024;
		public const bool IsLittleEndian = false;
		public const bool bNeedCheckPackage = true;

		public const int nUdpPackageFixedSize = 1024;
		public const int nUdpPackageFixedHeadSize = 12;
		public const int nUdpPackageFixedBodySize = nUdpPackageFixedSize - nUdpPackageFixedHeadSize;
	}
}