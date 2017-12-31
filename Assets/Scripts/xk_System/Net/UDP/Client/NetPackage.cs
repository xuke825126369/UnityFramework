using System.Collections;
using System.Collections.Generic;
using System;
using Google.Protobuf;

namespace xk_System.Net.UDP.Client
{
	public class NetPackageUtility
	{
		public static UInt16 getPackageId(UInt32 uniqueId)
		{
			return uniqueId / 10 / 10;
		}

		public static UInt16 getOrderId(UInt32 uniqueId)
		{
			return uniqueId / 10 % 10;
		}

		public static UInt16 getGroupCount(UInt32 uniqueId)
		{
			return uniqueId % 10;
		}

		public static UInt32 getUniqueId(UInt16 nPackageId, UInt16 orderId, UInt16 groupCount)
		{
			return (nPackageId * 10 + orderId) * 10 + groupCount;
		}
	}

	public class NetPackageGroup
	{
		List<NetReceivePackage> mGroupNetPackageQueue = new List<NetReceivePackage> ();

		public void AddToGroup(NetReceivePackage mNetPackage)
		{
			mGroupNetPackageQueue.Add (mNetPackage);
		}
	}

	public class NetReceivePackage
	{
		public UInt32 nUniqueId;
		public UInt16 nPackageId;
		public UInt16 nOrderId;
		public UInt16 nGroupCount;

		public ArraySegment<byte> buffer;

		public void InitData()
		{

		}

		public virtual void reset()
		{
			nUniqueId = -1;
			buffer = null;
		}
	}
}