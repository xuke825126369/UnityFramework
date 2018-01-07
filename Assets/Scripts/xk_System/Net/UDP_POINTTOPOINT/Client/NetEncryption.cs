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
	public static class NetPackageEncryption
	{
		private static byte[] mCheck = new byte[4] { (byte)'A', (byte)'B', (byte)'C', (byte)'D' };
	   
		public static bool DeEncryption (ListBuffer<byte> data, NetUdpFixedSizePackage mPackage)
		{
			if (data.Length < 12) {
				return false;
			}

			for (int i = 0; i < 4; i++) {
				if (data.Buffer [i] != mCheck [i]) {
					return false;
				}
			}

			mPackage.nOrderId = BitConverter.ToUInt16 (data.Buffer, 4);
			mPackage.nGroupCount = BitConverter.ToUInt16 (data.Buffer, 6);
			mPackage.nPackageId = BitConverter.ToUInt16 (data.Buffer, 8);

			UInt16 nBodyLength = BitConverter.ToUInt16 (data.Buffer, 10);

			if (nBodyLength + 12 > data.Length) {
				return false;
			}

			mPackage.Offset = 0;
			mPackage.Length = nBodyLength;
			Array.Copy (data.Buffer, 12, mPackage.buffer, mPackage.Offset, nBodyLength);
			return true;
		}

		public static void Encryption (NetUdpFixedSizePackage mPackage)
		{
			UInt16 nOrderId = mPackage.nOrderId;
			UInt16 nGroupCount = mPackage.nGroupCount;
			UInt16 nPackageId = mPackage.nPackageId;

			Array.Copy (mCheck, 0, mPackage.buffer, 0, 4);

			byte[] byCom = BitConverter.GetBytes (nOrderId);
			Array.Copy (byCom, 0, mPackage.buffer, 4, byCom.Length);
			byCom = BitConverter.GetBytes (nGroupCount);
			Array.Copy (byCom, 0, mPackage.buffer, 6, byCom.Length);
			byCom = BitConverter.GetBytes (nOrderId);
			Array.Copy (byCom, 0, mPackage.buffer, 8, byCom.Length);

			UInt16 buffer_Length = (UInt16)mPackage.Length;
			byte[] byBuLen = BitConverter.GetBytes (buffer_Length);
			Array.Copy (byBuLen, 0, mPackage.buffer, 10, byBuLen.Length);

			mPackage.Offset = 0;
		}
	}
}
