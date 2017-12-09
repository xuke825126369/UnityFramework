using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf;
using xk_System.Net.Client;
using System;

public class ProtobufUtility
{	
	public static byte[] SerializePackage(IMessage data)
	{
		Google.Protobuf.IMessage data1 = data as Google.Protobuf.IMessage;
		byte[] stream = data1.ToByteArray();
		return stream;
	}

	public static T getData<T>(NetPackage mPackage) where T:IMessage, new()
	{
		byte[] stream = mPackage.buffer;
		T t = new T();
		Google.Protobuf.CodedInputStream mStream = new CodedInputStream(stream);
		t.MergeFrom(mStream);
		return t;
	}

	public static T getData<T>(T t, NetPackage mPackage) where T:IMessage, new()
	{
		byte[] stream = mPackage.buffer;
		Google.Protobuf.CodedInputStream mStream = new CodedInputStream(stream);
		t.MergeFrom(mStream);
		return t;
	}
}

public class Protobuf3Event:NetSystem
{
	private Dictionary<int, Action<NetPackage>> mLogicFuncDic = new Dictionary<int, Action<NetPackage>>();
	public Protobuf3Event()
	{
		base.addNetListenFun (DeSerialize);
	}

	public void sendNetData(int command, IMessage data)
	{
		base.sendNetData(command, ProtobufUtility.SerializePackage (data));
	}

	public void DeSerialize(NetPackage mPackage)
	{
		mLogicFuncDic [mPackage.command] (mPackage);
	}
}
