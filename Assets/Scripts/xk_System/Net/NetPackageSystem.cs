using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using xk_System.Debug;
using UnityEngine;
using xk_System.Crypto;
using System.Collections;
using Google.Protobuf;
using game.protobuf.data;

namespace xk_System.Net
{
	public class Protobuf : Package
    {
        private GameProtocols serializer = new GameProtocols();
        private MemoryStream mst = new MemoryStream();

        public override byte[] SerializePackage(int command,object data)
        {
            //DebugSystem.Log("Send MemoryStream Length: "+mst.Length);
            mst.Position = 0;
           // mst.SetLength(0);
            serializer.Serialize(mst, data);
			stream = mst.ToArray();
           /* if (data_byte.Length==0)
            {
                DebugSystem.LogError("序列化失败");
            }*/
			
			return NetStream.GetOutStream(command,stream);
        }

        public override void DeSerializeStream(byte[] msg)
        {
			NetStream.GetInputStream(msg,out command,out stream);
           // DebugSystem.Log("接受命令："+command+" | "+data_byte.Length);
        }

        public override T  getData<T>()
        {
           // DebugSystem.Log("Receive MemoryStream Length: " + mst.Length);
			mst.SetLength(stream.Length);
            mst.Position = 0;
			mst.Write(stream,0,stream.Length);
            mst.Position = 0;
            return (T)serializer.Deserialize(mst,null, typeof(T)); //反序列化    
        }

        public override void reset()
        {
			base.reset();
            mst.Close();
        }
    }

    public class xk_Protobuf : Package
    {
        public override byte[] SerializePackage(int command, object data)
        {
            Google.Protobuf.IMessage data1 = data as Google.Protobuf.IMessage;
			stream = data1.ToByteArray();
			return NetStream.GetOutStream(command, stream);
        }

        public override void DeSerializeStream(byte[] msg)
        {
			NetStream.GetInputStream(msg,out command,out stream);
            // DebugSystem.Log("接受命令："+command+" | "+data_byte.Length);
        }

        public override T getData<T>()
        {
            T t = new T();          
            IMessage m =(IMessage)t;
			Google.Protobuf.CodedInputStream mStream = new CodedInputStream(stream);
            m.MergeFrom(mStream);
           // m= m.Descriptor.Parser.ParseFrom(data_byte);
            return (T)m;
        }

		public override void reset()
		{
			base.reset();
		}
    }

	public class NetSendSystem_Protobuf : NetSendSystem
	{
		public NetSendSystem_Protobuf(SocketSystem socketSys):base(socketSys)
		{
			
		}
	}

	public class NetReceiveSystem_Protobuf:NetReceiveSystem
	{
		public NetReceiveSystem_Protobuf(SocketSystem socket):base(socket)
		{

		}
	}

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

		public static byte[] DeEncryption(byte[] data)
        {
            if(data.Length-msg_head_BodyLength-stream_head_Length-stream_tail_Length<=0)
            {
				return null;
            }

            byte[] mStreamHeadArray1 = new byte[stream_head_Length];
           // Array.Copy(data,0, mStreamHeadArray1,0,stream_head_Length);

            byte[] msg_BodyLength_Array= new byte[msg_head_BodyLength];
            Array.Copy(data, stream_head_Length,msg_BodyLength_Array, 0,msg_head_BodyLength);

            byte[] mStreamTailArray1 = new byte[stream_tail_Length];
            //Array.Copy(data,stream_head_Length+msg_head_BodyLength,mStreamTailArray1,0,stream_tail_Length);

            int Length = msg_BodyLength_Array[0] | msg_BodyLength_Array[1] << 8 | msg_BodyLength_Array[2] << 16 | msg_BodyLength_Array[3] << 24;
            if(Length<=0 || data.Length-msg_head_BodyLength-stream_head_Length-stream_tail_Length-Length<0)
            {
				return null;
            }
            var BodyData = new byte[Length];
            Array.Copy(data,stream_head_Length+msg_head_BodyLength,BodyData,0,Length);
			return BodyData;
        }


		public static byte[] Encryption(byte[] data)
		{
			byte[] Encryption_data = null;
			int buffer_Length = data.Length;
			byte[] byte_head_BufferLength = new byte[msg_head_BodyLength];
			byte_head_BufferLength[0] = (byte)buffer_Length;
			byte_head_BufferLength[1] = (byte)(buffer_Length >> 8);
			byte_head_BufferLength[2] = (byte)(buffer_Length >> 16);
			byte_head_BufferLength[3] = (byte)(buffer_Length >> 24);

			Encryption_data = new byte[buffer_Length + msg_head_BodyLength+stream_head_Length+stream_tail_Length];
			Array.Copy(mStreamHeadArray, Encryption_data, stream_head_Length);
			Array.Copy(byte_head_BufferLength, 0,Encryption_data,stream_head_Length,msg_head_BodyLength);
			Array.Copy(data,0,Encryption_data,stream_head_Length+msg_head_BodyLength,buffer_Length);
			Array.Copy(mStreamTailArray,0,Encryption_data,msg_head_BodyLength+stream_head_Length+buffer_Length,stream_tail_Length);

			return Encryption_data;
		}
    }
		
    //begin~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~网络字节输入输出流系统~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

	public static class NetStream
    {
		public  const  int msg_head_command_length = 4;
		public  const string  Encrytption_Key= "1234567891234567";
		public  const string  Encrytption_iv = "1234567891234567";

		public static void GetInputStream(byte[] data,out int command,out byte[] buffer)
		{
			byte[] msg = EncryptionSystem.getSingle<Encryption_AES>().Decryption(data, Encrytption_Key, Encrytption_iv);

			int buffer_Length = msg.Length - msg_head_command_length;
			if(buffer_Length<=0)
			{
				command = -1;
				buffer = new byte[msg.Length];
				DebugSystem.LogError("接受数据异常："+msg.Length);
			}

			byte[] byte_head_command = new byte[msg_head_command_length];
			Array.Copy(msg, 0, byte_head_command, 0, msg_head_command_length);
			command = byte_head_command[0] | byte_head_command[1] << 8 | byte_head_command[2] << 16 | byte_head_command[3] << 24;

			buffer = new byte[buffer_Length];
			Array.Copy(msg, msg_head_command_length, buffer,0,buffer_Length);
		}


		public static byte[] GetOutStream(int command, byte[] msg)
		{
			if (msg == null || msg.Length == 0)
			{
				DebugSystem.LogError("发送数据失败：msg is Null Or Length is zero");
				return null;
			}

			int buffer_Length = msg.Length;
			int sum_Length = msg_head_command_length + buffer_Length;
			var data = new byte[sum_Length];

			byte[] byte_head_command = new byte[msg_head_command_length];
			byte_head_command[0] = (byte)command;
			byte_head_command[1] = (byte)(command >> 8);
			byte_head_command[2] = (byte)(command >> 16);
			byte_head_command[3] = (byte)(command >> 24);

			Array.Copy(byte_head_command, 0, data, 0, msg_head_command_length);
			Array.Copy(msg, 0, data, msg_head_command_length, buffer_Length);
			data = EncryptionSystem.getSingle<Encryption_AES>().Encryption(data, Encrytption_Key, Encrytption_iv);

			return data;
		}
    }
}
