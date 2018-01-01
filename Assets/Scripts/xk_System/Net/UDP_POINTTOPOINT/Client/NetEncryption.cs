using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using xk_System.Crypto;
using xk_System.Debug;
using xk_System.DataStructure;

namespace xk_System.Net.UDP.POINTTOPOINT.Client
{
	/// <summary>
	/// 把数据拿出来
	/// </summary>
	public static class NetEncryptionStream
	{
		private static byte[] mCheck = new byte[4] { (byte)'A', (byte)'B', (byte)'C', (byte)'D' };
		private static byte[] mReceiveBuffer = new byte[ClientConfig.nMaxBufferSize];
		private static byte[] mSendBuffer = new byte[ClientConfig.nMaxBufferSize];

		public static bool DeEncryption (CircularBuffer<byte> data, NetReceivePackage mPackage)
		{
			if (data.Length <= 12) {
				return false;
			}

			for (int i = 0; i < 4; i++) {
				if (data [i] != mCheck [i]) {
					return false;
				}
			}

			int UnqiueId = data [4] | data [5] << 8 | data [6] << 16 | data [7] << 24;
			mPackage.nUniqueId = (UInt32)UnqiueId;

			int nBodyLength1 = data [8] | data [9] << 8 | data [10] << 16 | data [11] << 24;
			if (nBodyLength1 <= 0 || nBodyLength1 + 8 > data.Length) {
				return false;
			}

			data.CopyTo (12, mPackage.buffer.Array, mPackage.buffer.Offset, nBodyLength1);
			data.ClearBuffer (nBodyLength1 + 12);

			ArraySegment<byte> mChe = new ArraySegment<byte> (mPackage.buffer.Array, mPackage.buffer.Offset, nBodyLength1);
			mPackage.buffer = mChe;
			return true;
		}

		public static ArraySegment<byte> EncryptionGroup (UInt32 uniqueId, byte[] msg,int offset, int Length)
		{
			if (mSendBuffer.Length < 12 + Length) {
				mSendBuffer = new byte[12 + Length];
			}

			UInt32 command = uniqueId;

			Array.Copy (mCheck, mSendBuffer, 4);

			byte[] byCom = BitConverter.GetBytes (command);
			Array.Copy (byCom, 0, mSendBuffer, 4, byCom.Length);

			int buffer_Length = Length;
			byte[] byBuLen = BitConverter.GetBytes (buffer_Length);
			Array.Copy (byBuLen, 0, mSendBuffer, 8, byBuLen.Length);

			Array.Copy (msg, offset, mSendBuffer, 12, Length);

			return new ArraySegment<byte> (mSendBuffer, 0, buffer_Length + 12);
		}
	}
}
