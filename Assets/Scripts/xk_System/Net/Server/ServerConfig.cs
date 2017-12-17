using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerConfig 
{
	//public const int nMaxPackageSize = 1024;
	//public const int nPerFrameHandlePackageCount = 16;

	public const int numConnections = 1024;
	public const int receiveBufferSize = 1024 * 64;
	public const int sendBufferSize = 1024 * 64;
}
