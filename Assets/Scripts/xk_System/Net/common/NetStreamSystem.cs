using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace xk_System.Net.Client
{
	public class NetBitStream
	{
		private int max_length = 4096;

		private int _Length = 0;

		private const int BYTE_LEN = 1;

		private const int INT32_LEN = 4;

		private const int SHORT16_LEN = 2;

		private const int FLOAT_LEN = 4;

		private byte[] _bytes = null;

		public NetBitStream(int maxLength = 0)
		{
			if (maxLength == 0) {
				maxLength = int.MaxValue;
			}
			this.max_length = maxLength;
			_Length = 0;
			_bytes = new byte[max_length];
		}

		public NetBitStream(byte[] _bytes,int maxLength = 0)
		{
			if (maxLength == 0) {
				maxLength = int.MaxValue;
			}

			this.max_length = maxLength;
			_Length = 0;
			_bytes = new byte[max_length];
		}

		// 写一个byte
		public void WriteByte(byte bt)
		{
			if (_Length> max_length)
				return;

			_bytes[_Length] = bt;
			_Length += BYTE_LEN;
		}


		// 写布尔型
		public void WriteBool(bool flag)
		{
			if (_Length + BYTE_LEN > max_length)
				return;

			// bool型实际是发送一个byte的值,判断是true或false
			byte b = (byte)'1';
			if (!flag)
				b = (byte)'0';

			_bytes[ _Length] = b;

			_Length += BYTE_LEN;
		}

		// 写整型
		public void WriteInt(int number)
		{
			if (_Length + INT32_LEN > max_length)
				return;

			byte[] bs = System.BitConverter.GetBytes(number);

			bs.CopyTo(_bytes,  _Length);

			_Length += INT32_LEN;
		}

		// 写无符号整型
		public void WriteUInt(uint number)
		{
			if (_Length + INT32_LEN > max_length)
				return;

			byte[] bs = System.BitConverter.GetBytes(number);

			bs.CopyTo(_bytes,  _Length);

			_Length += INT32_LEN;
		}


		// 写短整型
		public void WriteShort(short number)
		{
			if (_Length + SHORT16_LEN > max_length)
				return;

			byte[] bs = System.BitConverter.GetBytes(number);

			bs.CopyTo(_bytes,  _Length);

			_Length += SHORT16_LEN;
		}

		// 写无符号短整型
		public void WriteUShort(ushort number)
		{
			if (_Length + SHORT16_LEN > max_length)
				return;

			byte[] bs = System.BitConverter.GetBytes(number);

			bs.CopyTo(_bytes,  _Length);

			_Length += SHORT16_LEN;
		}


		//写浮点型 
		public void WriteFloat(float number)
		{
			if (_Length + FLOAT_LEN > max_length)
				return;

			byte[] bs = System.BitConverter.GetBytes(number);

			bs.CopyTo(_bytes,  _Length);

			_Length += FLOAT_LEN;
		}


		// 写字符串
		public void WriteString(string str)
		{
			ushort len = (ushort)System.Text.Encoding.UTF8.GetByteCount(str);
			this.WriteUShort(len);

			if (_Length + len > max_length)
				return;

			System.Text.Encoding.UTF8.GetBytes(str, 0, str.Length, _bytes,  + _Length);
			_Length += len;

		}

		// 读一个字节
		public void ReadByte(out byte bt)
		{
			bt = 0;

			if (_Length + BYTE_LEN > max_length)
				return;

			bt = _bytes[_Length];

			_Length += BYTE_LEN;

		}

		// 读 bool
		public void ReadBool(out bool flag)
		{
			flag = false;

			if (_Length + BYTE_LEN > max_length)
				return;

			byte bt = _bytes[_Length];

			if (bt == (byte)'1')
				flag = true;
			else
				flag = false;

			_Length += BYTE_LEN;

		}



		// 读 int
		public void ReadInt(out int number)
		{
			number = 0;

			if (_Length + INT32_LEN > max_length)
				return;

			number = System.BitConverter.ToInt32(_bytes,  _Length);

			_Length += INT32_LEN;

		}

		// 读 uint
		public void ReadUInt(out uint number)
		{
			number = 0;

			if (_Length + INT32_LEN > max_length)
				return;

			number = System.BitConverter.ToUInt32(_bytes,  _Length);

			_Length += INT32_LEN;

		}

		// 读 short
		public void ReadShort(out short number)
		{
			number = 0;

			if (_Length + SHORT16_LEN > max_length)
				return;


			number = System.BitConverter.ToInt16(_bytes,  _Length);

			_Length += SHORT16_LEN;

		}

		// 读 ushort
		public void ReadUShort(out ushort number)
		{
			number = 0;

			if (_Length + SHORT16_LEN > max_length)
				return;


			number = System.BitConverter.ToUInt16(_bytes , _Length);

			_Length += SHORT16_LEN;
		}



		// 读取一个float
		public void ReadFloat(out float number)
		{
			number = 0;

			if (_Length + FLOAT_LEN > max_length)
				return;

			number = System.BitConverter.ToSingle(_bytes, _Length);

			_Length += FLOAT_LEN;

		}

		// 读取一个字符串
		public void ReadString(out string str)
		{
			str = "";

			ushort len = 0;
			ReadUShort(out len);

			if (_Length + len > max_length)
				return;

			//str = Encoding.UTF8.GetString(_bytes,  + _Length, (int)len);
			str = Encoding.Default.GetString(_bytes,  _Length, len);
			_Length += len;

		}
	}
}
