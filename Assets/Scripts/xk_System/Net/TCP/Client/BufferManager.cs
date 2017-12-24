using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;

namespace xk_System.Net.TCP.Client
{
	class BufferManager
	{
		int Length;
		byte[] m_buffer;
		Stack<int> m_freeIndexPool;
		int nReadIndex = 0;
		int nBufferSize = 0;

		public BufferManager (int totalBytes,int nBufferSize)
		{
			this.Length = totalBytes;
			this.nReadIndex = 0;
			this.m_freeIndexPool = new Stack<int> ();
			this.m_buffer = new byte[Length];
			this.nBufferSize = nBufferSize;
		}
			
		public bool SetBuffer (SocketAsyncEventArgs args)
		{
			if (m_freeIndexPool.Count > 0) {
				args.SetBuffer (m_buffer, m_freeIndexPool.Pop (), nBufferSize);
			} else {
				if (nReadIndex + nBufferSize > Length) {
					return false;
				}
				args.SetBuffer (m_buffer, nReadIndex, nBufferSize);
				nReadIndex += nBufferSize;
			}
			return true;
		}

		public void FreeBuffer (SocketAsyncEventArgs args)
		{
			m_freeIndexPool.Push (args.Offset);
			args.SetBuffer (null, 0, 0);
		}
	}
}