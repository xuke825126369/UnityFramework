using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using xk_System.Crypto;
using xk_System.Debug;
using xk_System.DataStructure;

namespace xk_System.Net.UDP.Client
{
	/// <summary>
	/// 把数据拿出来
	/// </summary>
	public static class NetEncryptionStream
	{
		private static byte[] mCheck = new byte[4] { (byte)'A', (byte)'B', (byte)'C', (byte)'D' };
		private static byte[] mReceiveBuffer = new byte[ClientConfig.nMaxBufferSize];
		private static byte[] mSendBuffer = new byte[ClientConfig.nMaxBufferSize];

		public static bool DeEncryption (CircularBuffer<byte> data, NetPackage mPackage)
		{
			if (data.Length <= 8) {
				return false;
			}

			for (int i = 0; i < 4; i++) {
				if (data [i] != mCheck [i]) {
					return false;
				}
			}

			int nBodyLength1 = data [4] | data [5] << 8 | data [6] << 16 | data [7] << 24;
			if (nBodyLength1 <= 0 || nBodyLength1 + 8 > data.Length) {
				return false;
			}

			if (nBodyLength1 > mReceiveBuffer.Length) {
				mReceiveBuffer = new byte[nBodyLength1];
			}

			data.CopyTo (8, mReceiveBuffer, 0, nBodyLength1);
			data.ClearBuffer (nBodyLength1 + 8);

			UInt32 command = BitConverter.ToUInt32 (mReceiveBuffer, 8);
			int nBodyLength2 = msg.Length - 4;

			byte[] buffer = new byte[nBodyLength2];
			Array.Copy (msg, 4, buffer, 0, nBodyLength2);

			mPackage.uniqueId = command;
			mPackage.buffer = buffer;
			return true;
		}

		public static byte[] EncryptionGroup (UInt32 uniqueId, byte[] msg,int offset, int Length)
		{
			if (mSendBuffer.Length < 12 + Length) {
				mSendBuffer = new byte[12 + Length];
			}

			UInt32 command = uniqueId;
			UInt32 sum_Length = 4 + msg.Length;

			Array.Copy (mCheck, mSendBuffer, 4);

			byte[] byCom = BitConverter.GetBytes (command);
			Array.Copy (byCom, 0, mSendBuffer, 4, byCom.Length);

			UInt32 buffer_Length = Length;
			byte[] byBuLen = BitConverter.GetBytes (buffer_Length);
			Array.Copy (byBuLen, 0, mSendBuffer, 8, byBuLen.Length);

			Array.Copy (msg, offset, mSendBuffer, 12, Length);

			return new ArraySegment<byte> (mSendBuffer, 0, buffer_Length + 12);
		}
	}
}
