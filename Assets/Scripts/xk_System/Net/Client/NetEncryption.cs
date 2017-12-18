using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using xk_System.Crypto;
using xk_System.Debug;
using xk_System.Net.Client;
using xk_System.DataStructure;

namespace xk_System.Net.Client
{
	/// <summary>
	/// 把数据拿出来
	/// </summary>
	public static class NetEncryptionStream
	{
		// 4位标识符，4位 包长
		private const int nPackHeadLength = 8;
		//4包ID
		private const int nPack1HeadLength = 4;

		private const byte[] mCheck = new byte[4] { 'A', 'B', 'C', 'D' };
		private const Encryption_AES mAES = new Encryption_AES ("1234567891234567", "1234567891234567");
		private static byte[] mReceiveBuffer = new byte[1024];
		private static byte[] mSendBuffer = new byte[1024];

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

			data.CopyTo (6, mReceiveBuffer, 0, nBodyLength1);
			data.ClearBuffer (nBodyLength1 + 8);

			byte[] msg = Encryption_AES.Decryption (mReceiveBuffer, 0, nBodyLength1);
			if (msg.Length < 4) {
				DebugSystem.LogError ("解包失败");
				return false;
			}

			int command = msg [0] | msg [1] << 8 | msg [2] << 16 | msg [3] << 24;
			int nBodyLength2 = msg.Length - 4;

			byte[] buffer = new byte[nBodyLength2];
			Array.Copy (msg, 4, buffer, 0, nBodyLength2);
			mPackage.buffer = buffer;
			return true;
		}

		public static ArraySegment<byte> Encryption (NetPackage mPackage)
		{
			int command = mPackage.command;
			byte[] msg = mPackage.buffer;

			int buffer_Length = mPackage.buffer.Length;
			int sum_Length = 8 + buffer_Length;

			mSendBuffer [0] = (byte)command;
			mSendBuffer [1] = (byte)(command >> 8);
			mSendBuffer [2] = (byte)(command >> 16);
			mSendBuffer [3] = (byte)(command >> 24);

			Array.Copy (msg, 0, mSendBuffer, 4, buffer_Length);

			byte[] data = mAES.Encryption (mSendBuffer, 0, sum_Length);

			Array.Copy (data, 8);

			mSendBuffer [0] = (byte)buffer_Length;
			mSendBuffer [1] = (byte)(buffer_Length >> 8);
			mSendBuffer [2] = (byte)(buffer_Length >> 16);
			mSendBuffer [3] = (byte)(buffer_Length >> 24)

			mSendBuffer [0] = (byte)buffer_Length;
			mSendBuffer [1] = (byte)(buffer_Length >> 8);
			mSendBuffer [2] = (byte)(buffer_Length >> 16);
			mSendBuffer [3] = (byte)(buffer_Length >> 24);

			Encryption_data = new byte[buffer_Length + msg_head_BodyLength + stream_head_Length + stream_tail_Length];
			Array.Copy (mStreamHeadArray, Encryption_data, stream_head_Length);
			Array.Copy (byte_head_BufferLength, 0, Encryption_data, stream_head_Length, msg_head_BodyLength);
			Array.Copy (data, 0, Encryption_data, stream_head_Length + msg_head_BodyLength, buffer_Length);
			Array.Copy (mStreamTailArray, 0, Encryption_data, msg_head_BodyLength + stream_head_Length + buffer_Length, stream_tail_Length);

			return new ArraySegment<byte> (mSendBuffer, 0, buffer_Length);
		}
	}
}
