using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientConfig 
{
	public const int receiveBufferSize = 1024 * 64;
	public const int sendBufferSize = 1024 * 64;

	public const int nThreadSaveMaxBuffer = receiveBufferSize * 16;
}
