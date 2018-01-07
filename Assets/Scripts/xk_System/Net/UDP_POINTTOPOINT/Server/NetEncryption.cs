using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using xk_System.Crypto;
using xk_System.Debug;
using xk_System.DataStructure;

namespace xk_System.Net.UDP.POINTTOPOINT.Server
{
	/// <summary>
	/// 把数据拿出来
	/// </summary>
	public static class NetPackageEncryption
	{
		private static byte[] mCheck = new byte[4] { (byte)'A', (byte)'B', (byte)'C', (byte)'D' };
	   
		public static bool DeEncryption (NetUdpFixedSizePackage mPackage)
		{
			if (mPackage.Length < ServerConfig.nUdpPackageFixedHeadSize) {
				DebugSystem.LogError ("mPackage Length： " + mPackage.Length);
				return false;
			}

			for (int i = 0; i < 4; i++) {
				if (mPackage.buffer [i] != mCheck [i]) {
					DebugSystem.LogError ("22222222222222222222222222");
					return false;
				}
			}

			mPackage.nOrderId = BitConverter.ToUInt16 (mPackage.buffer, 4);
			mPackage.nGroupCount = BitConverter.ToUInt16 (mPackage.buffer, 6);
			mPackage.nPackageId = BitConverter.ToUInt16 (mPackage.buffer, 8);

			mPackage.Length = mPackage.Length - ServerConfig.nUdpPackageFixedHeadSize;
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
			byCom = BitConverter.GetBytes (nPackageId);
			Array.Copy (byCom, 0, mPackage.buffer, 8, byCom.Length);
		}
	}
}
