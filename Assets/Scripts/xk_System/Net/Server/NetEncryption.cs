using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using xk_System.Crypto;
using xk_System.Debug;
using xk_System.Net.Client;
using xk_System.DataStructure;

namespace xk_System.Net.Server
{
	public static class NetEncryptionStream
	{
		public const int stream_head_Length = 2;
		public const int stream_tail_Length = 2;
		public const int msg_head_BodyLength = 4;
		private static byte[] mStreamHeadArray = new byte[stream_head_Length] { 7, 7 };
		private static byte[] mStreamTailArray = new byte[stream_head_Length] { 7, 7 };

		public  const  int msg_head_command_length = 4;
		public  const string Encrytption_Key = "1234567891234567";
		public  const string Encrytption_iv = "1234567891234567";

		static byte[] msg_Head_Array = new byte[2];
		static byte[] msg_BodyLength_Array = new byte[4];
		static byte[] BodyData = new byte[1024];
		static int BodyLength = 0;

		public static bool DeEncryption (CircularBuffer<byte> data, NetPackage mPackage)
		{
			if (data.Length - 8 <= 0) {
				DebugSystem.LogError ("Server 系统池解析错误： 0000000000000000");
				return false;
			}

			int readLength = data.CopyTo (2, msg_BodyLength_Array, 0, 4);
			if (readLength < 4) {
				DebugSystem.LogError ("Server 系统池解析错误： 1111111111111111");
				return false;
			}

			BodyLength = msg_BodyLength_Array [0] | msg_BodyLength_Array [1] << 8 | msg_BodyLength_Array [2] << 16 | msg_BodyLength_Array [3] << 24;
			if (BodyLength <= 0 || BodyLength + 8 > data.Length) {
				DebugSystem.LogError ("Server 系统池解析错误： 2222222222222222222：" + BodyLength);
				return false;
			}

			if (BodyLength > BodyData.Length) {
				BodyData = new byte[BodyLength];
			}

			data.WriteTo (0, BodyData, 0, BodyLength);
			data.ClearBuffer (BodyLength + 8);

			DebugSystem.LogError ("Server 系统池解析成功: " + BodyLength);

			byte[] aesStream = new byte[BodyLength];
			Array.Copy (BodyData, 0, aesStream, 0, BodyLength);
			byte[] msg = Encryption_AES.Decryption (aesStream, Encrytption_Key, Encrytption_iv);

			int buffer_Length = msg.Length - msg_head_command_length;
			if (buffer_Length <= 0) {
				return false;
			}

			byte[] byte_head_command = new byte[msg_head_command_length];
			Array.Copy (msg, 0, byte_head_command, 0, msg_head_command_length);
			mPackage.command = byte_head_command [0] | byte_head_command [1] << 8 | byte_head_command [2] << 16 | byte_head_command [3] << 24;

			mPackage.Length = buffer_Length;
			Array.Copy (msg, msg_head_command_length, mPackage.buffer, 0, buffer_Length);
			return true;
		}

		public static byte[] Encryption (NetPackage mPackage)
		{
			int command = mPackage.command;
			byte[] msg = mPackage.buffer;

			int buffer_Length = mPackage.Length;
			int sum_Length = msg_head_command_length + buffer_Length;
			byte[] data = new byte[sum_Length];

			byte[] byte_head_command = new byte[msg_head_command_length];
			byte_head_command [0] = (byte)command;
			byte_head_command [1] = (byte)(command >> 8);
			byte_head_command [2] = (byte)(command >> 16);
			byte_head_command [3] = (byte)(command >> 24);

			Array.Copy (byte_head_command, 0, data, 0, msg_head_command_length);
			Array.Copy (msg, 0, data, msg_head_command_length, buffer_Length);
			data = Encryption_AES.Encryption (data, Encrytption_Key, Encrytption_iv);

			byte[] Encryption_data = null;
			buffer_Length = data.Length;
			byte[] byte_head_BufferLength = new byte[msg_head_BodyLength];
			byte_head_BufferLength [0] = (byte)buffer_Length;
			byte_head_BufferLength [1] = (byte)(buffer_Length >> 8);
			byte_head_BufferLength [2] = (byte)(buffer_Length >> 16);
			byte_head_BufferLength [3] = (byte)(buffer_Length >> 24);

			Encryption_data = new byte[buffer_Length + msg_head_BodyLength + stream_head_Length + stream_tail_Length];
			Array.Copy (mStreamHeadArray, Encryption_data, stream_head_Length);
			Array.Copy (byte_head_BufferLength, 0, Encryption_data, stream_head_Length, msg_head_BodyLength);
			Array.Copy (data, 0, Encryption_data, stream_head_Length + msg_head_BodyLength, buffer_Length);
			Array.Copy (mStreamTailArray, 0, Encryption_data, msg_head_BodyLength + stream_head_Length + buffer_Length, stream_tail_Length);

			return Encryption_data;
		}
	}
}
