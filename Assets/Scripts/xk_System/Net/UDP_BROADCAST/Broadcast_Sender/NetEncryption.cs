using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using xk_System.Crypto;
using xk_System.Debug;
using xk_System.DataStructure;

namespace xk_System.Net.UDP.BROADCAST.Client
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
			if (data.Length <= 8) {
				return false;
			}

			for (int i = 0; i < 4; i++) {
				if (data [i] != mCheck [i]) {
					return false;
				}
			}

			byte[] commandBytes = new byte[2];
			data.CopyTo (4, commandBytes, 0, 2);
			mPackage.nUniqueId = BitConverter.ToUInt16 (commandBytes, 0);

			byte[] bodyLengthBytes = new byte[2];
			data.CopyTo (6, bodyLengthBytes, 0, 2);
			UInt16 nBodyLength1 = BitConverter.ToUInt16 (bodyLengthBytes, 0);

			if (nBodyLength1 < 0 || nBodyLength1 + 8 > data.Length) {
				return false;
			}

			data.CopyTo (8, mPackage.buffer.Array, mPackage.buffer.Offset, nBodyLength1);
			data.ClearBuffer (nBodyLength1 + 8);

			ArraySegment<byte> mChe = new ArraySegment<byte> (mPackage.buffer.Array, mPackage.buffer.Offset, nBodyLength1);
			mPackage.buffer = mChe;
			return true;
		}

		public static ArraySegment<byte> EncryptionGroup (UInt16 uniqueId, byte[] msg,int offset, int Length)
		{
			if (mSendBuffer.Length < 8 + Length) {
				mSendBuffer = new byte[8 + Length];
			}

			UInt16 command = uniqueId;
			Array.Copy (mCheck, 0, mSendBuffer, 0, 4);

			byte[] byCom = BitConverter.GetBytes (command);
			Array.Copy (byCom, 0, mSendBuffer, 4, byCom.Length);

			UInt16 buffer_Length = (UInt16)Length;
			byte[] byBuLen = BitConverter.GetBytes (buffer_Length);
			Array.Copy (byBuLen, 0, mSendBuffer, 6, byBuLen.Length);

			Array.Copy (msg, offset, mSendBuffer, 8, Length);

			return new ArraySegment<byte> (mSendBuffer, 0, buffer_Length + 8);
		}
	}
}
