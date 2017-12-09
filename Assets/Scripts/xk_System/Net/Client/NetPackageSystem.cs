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
	public class NetPackage
	{
		public int command;
		public byte[] buffer;
	}
		
	public interface PackageInterface
	{
		byte[] SerializePackage();
		void DeSerializeStream(byte[] msg);
		T getData<T>() where T : new();
		void reset();

		int getCommand ();
		void setCommand (int command);
		void setObjectData (object data);
		object getObjectData ();
	}

	public abstract class Package:PackageInterface
	{
		protected int command = -1;
		protected object data = null;
		protected byte[] stream = null;

		public abstract byte[] SerializePackage();

		public abstract void DeSerializeStream(byte[] msg);

		public abstract T getData<T>() where T : new();

		public int getCommand ()
		{
			return command;
		}

		public void setCommand (int command)
		{
			this.command = command;
		}

		public void setObjectData (object data)
		{
			this.data = data;
		}

		public object getObjectData ()
		{
			return data;
		}

		public virtual void reset()
		{
			data = null;
			stream = null;
			command = -1;
		}
	}

	public class Protobuf : Package
    {
        private GameProtocols serializer = new GameProtocols();
        private MemoryStream mst = new MemoryStream();

        public override byte[] SerializePackage()
        {
            mst.Position = 0;
            serializer.Serialize(mst, data);
			stream = mst.ToArray();
			return NetStream.GetOutStream(command,stream);
        }

        public override void DeSerializeStream(byte[] msg)
        {
			NetStream.GetInputStream(msg,out command,out stream);
        }

        public override T  getData<T>()
        {
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

    public class Protobuf3 : Package
    {
        public override byte[] SerializePackage()
        {
            Google.Protobuf.IMessage data1 = data as Google.Protobuf.IMessage;
			stream = data1.ToByteArray();
			return NetStream.GetOutStream(command, stream);
        }

        public override void DeSerializeStream(byte[] msg)
        {
			NetStream.GetInputStream(msg,out command,out stream);
        }

        public override T getData<T>()
        {
            T t = new T();          
            IMessage m =(IMessage)t;
			Google.Protobuf.CodedInputStream mStream = new CodedInputStream(stream);
            m.MergeFrom(mStream);
            return (T)m;
        }

		public override void reset()
		{
			base.reset();
		}
    }
}
