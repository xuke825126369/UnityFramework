using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ObjectPoolInterface
{
	void reset();
}

//Object 池子
public class ObjectPool<T> : Singleton<ObjectPool<T>> where T : ObjectPoolInterface,new()
{
	Queue<T> mObjectPool = new Queue<T>();
	public T Pop()
	{
		if (mObjectPool.Count > 0) {
			return mObjectPool.Dequeue ();
		} else {
			return new T ();
		}
	}

	public void Push(T t)
	{
		t.reset ();
		mObjectPool.Enqueue (t);
	}
}

public class SystemObjectPool<T> : Singleton<SystemObjectPool<T>> where T:new()
{
	Queue<T> mObjectPool = new Queue<T>();
	public T Pop()
	{
		if (mObjectPool.Count > 0) {
			return mObjectPool.Dequeue ();
		} else {
			return new T ();
		}
	}

	public void Push(T t)
	{
		mObjectPool.Enqueue (t);
	}
}

//Unity GameObject 池子
public class GameObjectPool<T> : Singleton<GameObjectPool> where T : MonoBehaviour,ObjectPoolInterface
{
	Dictionary<GameObject,Queue<T>> mFreeObjectPool = new Dictionary<GameObject, Queue<T>> ();
	Dictionary<GameObject,Queue<T>> mUseObjectPool = new Dictionary<GameObject, Queue<T>> ();

	public void Init(GameObject prefab,int count = 1)
	{
		Debug.Assert (prefab.GetComponent<T> () != null);

		if (!mFreeObjectPool.ContainsKey (prefab)) {
			Queue<T> mPool = new Queue<T> ();
			mFreeObjectPool [prefab] = mPool;
		}

		for (int i = 0; i < count; i++) {
			GameObject obj = MonoBehaviour.Instantiate(prefab) as GameObject;
			mFreeObjectPool[prefab].Enqueue (obj.GetComponent<T>());
		}
	}

	public T Pop(GameObject prefab)
	{
		T obj = null;
		if (mFreeObjectPool.Count > 0) {
			obj = mFreeObjectPool[prefab].Dequeue ();
		} else {
			if (!mFreeObjectPool.ContainsKey (prefab)) {
				Queue<T> mPool = new Queue<T> ();
				mFreeObjectPool [prefab] = mPool;
			}

			GameObject temp = MonoBehaviour.Instantiate(prefab) as GameObject;
			obj = temp.GetComponent<T> ();
		}

		if (!mUseObjectPool.ContainsKey(prefab))
		{
			mUseObjectPool [prefab] = new Queue<T> ();
		}

		mUseObjectPool[prefab].Enqueue (obj);
		return obj;
	}

	public void Push(T t)
	{
		t.reset ();
		foreach (var keyvaue in mUseObjectPool) {
			if (keyvaue.Value.Contains (t)) {
				mFreeObjectPool [keyvaue.Key].Enqueue (t);
			}
		}
	}
}


//Unity GameObject 池子
public class GameObjectPool : Singleton<GameObjectPool>
{
	Dictionary<GameObject,Queue<GameObject>> mFreeObjectPool = new Dictionary<GameObject, Queue<GameObject>> ();
	Dictionary<GameObject,Queue<GameObject>> mUseObjectPool = new Dictionary<GameObject, Queue<GameObject>> ();

	public void Init(GameObject prefab,int count = 1)
	{
		if (!mFreeObjectPool.ContainsKey (prefab)) {
			Queue<GameObject> mPool = new Queue<GameObject> ();
			mFreeObjectPool [prefab] = mPool;
		}

		for (int i = 0; i < count; i++) {
			GameObject obj = MonoBehaviour.Instantiate(prefab) as GameObject;
			mFreeObjectPool[prefab].Enqueue (obj);
		}
	}

	public GameObject Pop(GameObject prefab)
	{
		GameObject obj = null;
		if (mFreeObjectPool.Count > 0) {
			obj = mFreeObjectPool[prefab].Dequeue ();
		} else {
			if (!mFreeObjectPool.ContainsKey (prefab)) {
				Queue<GameObject> mPool = new Queue<GameObject> ();
				mFreeObjectPool [prefab] = mPool;
			}
				
			obj = MonoBehaviour.Instantiate(prefab) as GameObject;
		}

		if (!mUseObjectPool.ContainsKey(prefab))
		{
			mUseObjectPool [prefab] = new Queue<GameObject> ();
		}

		mUseObjectPool[prefab].Enqueue (obj);
		return obj;
	}

	public void Push(GameObject t)
	{
		foreach (var keyvaue in mUseObjectPool) {
			if (keyvaue.Value.Contains (t)) {
				mFreeObjectPool [keyvaue.Key].Enqueue (t);
			}
		}
	}
}
