using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace xk_System.Net.Client
{
	public class  NetSendSystem:NetSendSystemInterface
	{
		protected ObjectPool<NetPackage> mCanUseNetPackageQueue;
		protected Queue<NetPackage> mNeedHandleNetPackageQueue;
		protected SocketSystem mSocketSystem;

		public NetSendSystem (SocketSystem socketSys)
		{
			this.mSocketSystem = socketSys;
			mNeedHandleNetPackageQueue = new Queue<NetPackage> ();
			mCanUseNetPackageQueue = new ObjectPool<NetPackage> ();
		}

		public void SendNetData (int id, byte[] buffer)
		{
			NetPackage mNetPackage = mCanUseNetPackageQueue.Pop ();
			mNetPackage.command = id;
			mNetPackage.buffer = buffer;
			mNetPackage.Length = buffer.Length;
			mNeedHandleNetPackageQueue.Enqueue (mNetPackage);
		}

		public void HandleNetPackage ()
		{
			int handlePackageCount = 0;
			while (mNeedHandleNetPackageQueue.Count > 0) {
				var mPackage = mNeedHandleNetPackageQueue.Dequeue ();
				HandleNetStream (mPackage);
				mCanUseNetPackageQueue.recycle (mPackage);
				handlePackageCount++;
			}

			if (handlePackageCount > 5) {
				//DebugSystem.Log ("客户端 发送包的数量： " + handlePackageCount);
			}
		}

		private void HandleNetStream (NetPackage mPackage)
		{
			byte[] stream = NetEncryptionStream.Encryption (mPackage);
			mSocketSystem.SendNetStream (stream, 0, stream.Length);
		}

		public void release ()
		{
			lock (mNeedHandleNetPackageQueue) {
				mNeedHandleNetPackageQueue.Clear ();
			}
		}
	}
}
