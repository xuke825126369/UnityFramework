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

namespace xk_System.Net.Client
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
}
