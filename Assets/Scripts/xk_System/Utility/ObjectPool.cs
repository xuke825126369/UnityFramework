using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using xk_System.Debug;

namespace xk_System
{
	public interface ObjectPoolInterface
	{
		void reset ();
	}

	//Object 池子
	public class ObjectPool<T> where T:new()
	{
		private int nMaxObjectCount = 0;
		Queue<T> mObjectPool = null;

		public ObjectPool()
		{
			this.nMaxObjectCount = int.MaxValue;
			mObjectPool = new Queue<T> ();
		}

		public T Pop()
		{
			if (mObjectPool.Count > 0) {
				return mObjectPool.Dequeue ();
			} else {
				return new T ();
			}
		}

		public void recycle(T t)
		{
			if (t is ObjectPoolInterface) {
				ObjectPoolInterface mInterface = t as ObjectPoolInterface;
				mInterface.reset ();
			}
			mObjectPool.Enqueue (t);
		}

		public void release()
		{
			mObjectPool.Clear ();
			mObjectPool = null;
		}
	}
		
	public class ArrayGCPool<T>
	{
		Dictionary<int, List<T[]>> mPoolQueue = new Dictionary<int, List<T[]>> ();

		public void recycle(T[] array)
		{
			Array.Clear (array, 0, array.Length);
			if (!mPoolQueue.ContainsKey (array.Length)) {
				mPoolQueue [array.Length] = new List<T[]> ();
			}

			if (!mPoolQueue [array.Length].Contains (array)) {
				mPoolQueue [array.Length].Add (array);
			}
		}

		public T[] Pop(int Length)
		{
			if (!mPoolQueue.ContainsKey (Length)) {
				mPoolQueue [Length] = new List<T[]> ();
			}

			if (mPoolQueue [Length].Count == 0) {
				mPoolQueue [Length].Add (new T[Length]);
			}
			var v = mPoolQueue [Length] [0];
			mPoolQueue [Length].Remove (v);
			return v;
		}

		public void  release()
		{
			mPoolQueue.Clear ();
		}
	}

	public class ListGCPool<T>
	{
		Queue<List<T>> mPoolQueue = new Queue<List<T>> ();
		public void recycle(List<T> list)
		{
			list.Clear ();
			if (!mPoolQueue.Contains (list)) {
				mPoolQueue.Enqueue (list);
			}
		}

		public List<T> Pop()
		{
			List<T> list = null;
			if (mPoolQueue.Count == 0) {
				list = new List<T> ();
			} else {
				list = mPoolQueue.Dequeue();
			}

			return list;
		}

		public void  release()
		{
			mPoolQueue.Clear ();
		}
	}
}