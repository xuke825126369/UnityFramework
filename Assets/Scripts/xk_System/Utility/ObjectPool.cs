using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using xk_System.Debug;
using System.Collections.Concurrent;

namespace xk_System
{
	public interface ObjectPoolInterface
	{
		void reset ();
	}

	//Object 池子
	public class ObjectPool<T> where T:class, new()
	{
		Queue<T> mObjectPool = null;

		public ObjectPool(int initCapacity = 0)
		{
			mObjectPool = new Queue<T> ();
			for (int i = 0; i < initCapacity; i++) {
				mObjectPool.Enqueue (new T ());
			}
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

	public class SafeObjectPool<T> where T:class,new()
	{
		private int nMaxObjectCount = 0;
		private ConcurrentQueue<T> mObjectPool = null;

		public SafeObjectPool(int initCapacity = 0)
		{
			this.nMaxObjectCount = int.MaxValue;
			mObjectPool = new ConcurrentQueue<T> ();
			for (int i = 0; i < initCapacity; i++) {
				mObjectPool.Enqueue (new T ());
			}
		}

		public T Pop()
		{
			T t = default(T);
			if (!mObjectPool.TryDequeue (out t)) {
				return new T ();
			}
			return t;
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