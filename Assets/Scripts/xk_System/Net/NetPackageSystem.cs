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
        NetOutputStream mOutputStream = new NetOutputStream();
        NetInputStream mInputStream = new NetInputStream();

        public override byte[] SerializePackage(int command,object data)
        {
            //DebugSystem.Log("Send MemoryStream Length: "+mst.Length);
            mst.Position = 0;
           // mst.SetLength(0);
            serializer.Serialize(mst, data);
            data_byte = mst.ToArray();
           /* if (data_byte.Length==0)
            {
                DebugSystem.LogError("序列化失败");
            }*/
            mOutputStream.SetData(command,data_byte);
            return mOutputStream.data;
        }

        public override void DeSerializeStream(byte[] msg)
        {
            mInputStream.SetData(msg);
            data_byte = mInputStream.buffer;
            command = mInputStream.command;
           // DebugSystem.Log("接受命令："+command+" | "+data_byte.Length);
        }

        public override T  getData<T>()
        {
           // DebugSystem.Log("Receive MemoryStream Length: " + mst.Length);
            mst.SetLength(data_byte.Length);
            mst.Position = 0;
            mst.Write(data_byte,0,data_byte.Length);
            mst.Position = 0;
            return (T)serializer.Deserialize(mst,null, typeof(T)); //反序列化    
        }

        public override void Destory()
        {
            base.Destory();
            mst.Close();
        }
    }

    public class xk_Protobuf : Package
    {
        NetOutputStream mOutputStream = new NetOutputStream();
        NetInputStream mInputStream = new NetInputStream();

        public override byte[] SerializePackage(int command, object data)
        {
            Google.Protobuf.IMessage data1 = data as Google.Protobuf.IMessage;
            data_byte = data1.ToByteArray();
            mOutputStream.SetData(command, data_byte);
            return mOutputStream.data;
        }

        public override void DeSerializeStream(byte[] msg)
        {
            mInputStream.SetData(msg);
            data_byte = mInputStream.buffer;
            command = mInputStream.command;
            // DebugSystem.Log("接受命令："+command+" | "+data_byte.Length);
        }

        public override T getData<T>()
        {
            T t = new T();          
            IMessage m =(IMessage)t;
            Google.Protobuf.CodedInputStream mStream = new CodedInputStream(data_byte);
            m.MergeFrom(mStream);
           // m= m.Descriptor.Parser.ParseFrom(data_byte);
            return (T)m;
        }
    }

	public class NetSendSystem_Protobuf : NetSendSystem
	{
		public NetSendSystem_Protobuf():base()
		{

		}

		public override void Send(NetSystem mNetSystem, int command,object data)
		{
			byte[] stream= mPackage.SerializePackage(command,data);
			mSendPool.SendPackage(mNetSystem,stream);
		}
	}

	public class NetReceiveSystem_Protobuf:NetReceiveSystem
	{
		private PackageReceivePool mPackageReceivePool = new PackageReceivePool();
		public NetReceiveSystem_Protobuf()
		{

		}

		public override void Receive(byte[] msg)
		{
			mPackageReceivePool.ReceiveInfo(msg);
			int PackageCout = 0;
			while (true)
			{
				byte[] mPackageByteArray= mPackageReceivePool.GetPackage();
				if (mPackageByteArray != null)
				{
					Package mPackage = null;
					lock (mCanUsePackageQueue)
					{
						if (mCanUsePackageQueue.Count == 0)
						{
							mPackage = new xk_Protobuf();
						}
						else
						{
							mPackage = mCanUsePackageQueue.Dequeue();
						}
					}
					mPackage.DeSerializeStream(mPackageByteArray);
					lock (mNeedHandlePackageQueue)
					{
						mNeedHandlePackageQueue.Enqueue(mPackage);
					}
					PackageCout++;
				}else
				{
					break;
				}
			}
			DebugSystem.LogError("解析包的数量： " + PackageCout);
		}
	}
    //begin~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~网络接受包信息池系统~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    public class PackageReceivePool : NetEncryptionInputStream
    {
        private List<byte> mReceiveStreamQueue = new List<byte>();
       
        public void ReceiveInfo(byte[] mbyteArray)
        {
            mReceiveStreamQueue.AddRange(mbyteArray);
        }

        public byte[] GetPackage()
        {
            byte[] msg = mReceiveStreamQueue.ToArray();
            SetData(msg);
            if(BodyData==null)
            {
                return BodyData;
            }
            int Length = BodyData.Length + stream_head_Length + stream_tail_Length + msg_head_BodyLength;
            mReceiveStreamQueue.RemoveRange(0, Length);
            return BodyData;
        }
    }

    public class PackageSendPool : NetEncryptionOutStream
    {
        public  void SendPackage(NetSystem mNetSystem,byte[] data)
        {
            SetData(data);
            mNetSystem.SendInfo(Encryption_data);
        }
    }


    //begin~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~网络编码输入输出流系统~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public class NetEncryptionStream
    {
        public const int stream_head_Length = 2;
        public const int stream_tail_Length = 2;
        public const int msg_head_BodyLength = 4;
    }

    /// <summary>
    /// 把数据拿出来
    /// </summary>
    public class NetEncryptionInputStream:NetEncryptionStream
    {
       public byte[] BodyData=null;
       private byte[] mStreamHeadArray = new byte[stream_head_Length] { 7, 7 };
       private byte[] mStreamTailArray = new byte[stream_head_Length] { 7, 7 };

        public void SetData(byte[] data)
        {
            BodyData = null;
            if(data.Length-msg_head_BodyLength-stream_head_Length-stream_tail_Length<=0)
            {
                return;
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
                return;
            }
            BodyData = new byte[Length];
            Array.Copy(data,stream_head_Length+msg_head_BodyLength,BodyData,0,Length);
        }
    }

    /// <summary>
    /// 为数据加个标志
    /// </summary>
    public class NetEncryptionOutStream : NetEncryptionStream
    {
        public byte[] Encryption_data;
        byte[] mStreamHeadArray = new byte[stream_head_Length] { 7, 7 };
        byte[] mStreamTailArray = new byte[stream_head_Length] { 7, 7 };
        /// <summary>
        /// data 为加密数据流
        /// </summary>
        /// <param name="data"></param>
        public void SetData(byte[] data)
        {
            Encryption_data = null;
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
        }

    }



    //begin~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~网络字节输入输出流系统~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    public class NetStream
    {
       public const int msg_head_command_length = 4;

       public  string Encrytption_Key= "1234567891234567";

       public  string Encrytption_iv = "1234567891234567";

    }

    public class NetInputStream:NetStream
    {
        public byte[] buffer = null;
        public int command = -1;
        public NetInputStream()
        {
              buffer = null;
              command = -1;
        }

        public NetInputStream(byte[] data)
        {
            SetData(data);
        }

        private NetEncryptionInputStream mEncryptionStream = new NetEncryptionInputStream();
        public void SetData(byte[] data)
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
    }

    public class NetOutputStream:NetStream
    {
        public byte[] data=null;
        private NetEncryptionOutStream mEncryptionStream = new NetEncryptionOutStream();
        public NetOutputStream()
        {
            data = null;
        }

        public NetOutputStream(int command,byte[] msg)
        {
            SetData(command,msg);
        }

        public void SetData(int command, byte[] msg)
        {
            if (msg == null || msg.Length == 0)
            {
                DebugSystem.LogError("发送数据失败：msg is Null Or Length is zero");
                return;
            }
            int buffer_Length = msg.Length;
            int sum_Length = msg_head_command_length + buffer_Length;
            data = new byte[sum_Length];

            byte[] byte_head_command = new byte[msg_head_command_length];
            byte_head_command[0] = (byte)command;
            byte_head_command[1] = (byte)(command >> 8);
            byte_head_command[2] = (byte)(command >> 16);
            byte_head_command[3] = (byte)(command >> 24);

            Array.Copy(byte_head_command, 0, data, 0, msg_head_command_length);
            Array.Copy(msg, 0, data, msg_head_command_length, buffer_Length);
           // DebugSystem.LogColor("发送Protobuf数据：");
           // DebugSystem.LogBitStream(data);
            data = EncryptionSystem.getSingle<Encryption_AES>().Encryption(data, Encrytption_Key, Encrytption_iv);
        }
    }
}
