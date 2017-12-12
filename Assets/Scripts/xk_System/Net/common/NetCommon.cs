using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using xk_System.Crypto;
using xk_System.Debug;
using xk_System.Net.Client;
using xk_System.DataStructure;

namespace xk_System.Net
{
	//begin~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~网络编码输入输出流系统~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

	/// <summary>
	/// 把数据拿出来
	/// </summary>
	public static class NetEncryptionStream
	{
		public const int stream_head_Length = 2;
		public const int stream_tail_Length = 2;
		public const int msg_head_BodyLength = 4;
		private static byte[] mStreamHeadArray = new byte[stream_head_Length] { 7, 7 };
		private static byte[] mStreamTailArray = new byte[stream_head_Length] { 7, 7 };

		public static bool DeEncryption (CircularBuffer<byte> data, NetPackage mPackage)
		{
			if (data.Length - msg_head_BodyLength - stream_head_Length - stream_tail_Length <= 0) {
				return false;
			}

			byte[] msg_BodyLength_Array = new byte[msg_head_BodyLength];
			data.CopyTo (stream_head_Length, msg_BodyLength_Array, 0, msg_head_BodyLength);

			int Length = msg_BodyLength_Array [0] | msg_BodyLength_Array [1] << 8 | msg_BodyLength_Array [2] << 16 | msg_BodyLength_Array [3] << 24;
			if (Length <= 0 || data.Count - 8 <= 0) {
				return false;
			}

			var BodyData = new byte[Length];
			data.CopyTo (stream_head_Length + msg_head_BodyLength,BodyData, 0, Length);

			NetStream.GetInputStream (BodyData, out mPackage.command, out mPackage.buffer);

			data.RemoveRange (0, BodyData.Length + 8);
			return true;
		}

		public static byte[] Encryption (byte[] data)
		{
			byte[] Encryption_data = null;
			int buffer_Length = data.Length;
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

	//begin~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~网络字节输入输出流系统~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

	public static class NetStream
	{
		public  const  int msg_head_command_length = 4;
		public  const string Encrytption_Key = "1234567891234567";
		public  const string Encrytption_iv = "1234567891234567";

		public static void GetInputStream (byte[] data, out int command, out byte[] buffer)
		{
			
			byte[] msg = Encryption_AES.Decryption (data, Encrytption_Key, Encrytption_iv);

			int buffer_Length = msg.Length - msg_head_command_length;
			if (buffer_Length <= 0) {
				command = -1;
				buffer = new byte[msg.Length];
				DebugSystem.LogError ("接受数据异常：" + msg.Length);
			}

			byte[] byte_head_command = new byte[msg_head_command_length];
			Array.Copy (msg, 0, byte_head_command, 0, msg_head_command_length);
			command = byte_head_command [0] | byte_head_command [1] << 8 | byte_head_command [2] << 16 | byte_head_command [3] << 24;

			buffer = new byte[buffer_Length];
			Array.Copy (msg, msg_head_command_length, buffer, 0, buffer_Length);
		}


		public static byte[] GetOutStream (int command, byte[] msg)
		{
			if (msg == null || msg.Length == 0) {
				DebugSystem.LogError ("发送数据失败：msg is Null Or Length is zero");
				return null;
			}

			int buffer_Length = msg.Length;
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

			return data;
		}
	}
}
