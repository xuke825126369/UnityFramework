using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xk_System.Debug;

namespace xk_System
{
	public class GameObjectPool<T> where T : MonoBehaviour
	{
		private T prefab = null;
		private Queue<T> mObjectPool = new Queue<T> ();

		public void Init(T prefab, int count = 1)
		{
			DebugSystem.Assert (prefab != null);
			this.prefab = prefab;
			for (int i = 0; i < count; i++) {
				T obj = UnityEngine.Object.Instantiate(prefab);
				mObjectPool.Enqueue (obj);
			}
		}

		public T Pop()
		{
			T obj = null;
			if (mObjectPool.Count > 0) {
				obj = mObjectPool.Dequeue ();
			} else {
				obj = UnityEngine.Object.Instantiate(prefab);
			}

			return obj;
		}

		public void recycle(T t)
		{
			if (t.gameObject.activeSelf) {
				t.gameObject.SetActive (false);
			}
			mObjectPool.Enqueue (t);
		}

		public void release()
		{
			while (mObjectPool.Count > 0) {
				var obj = mObjectPool.Dequeue ();
				UnityEngine.Object.Destroy (obj);
			}
		}
	}
		
	public class GameObjectPool
	{
		Dictionary<GameObject, Queue<GameObject>> mFreeObjectPool = new Dictionary<GameObject, Queue<GameObject>> ();
		Dictionary<GameObject, GameObject> mUseObjectPool = new Dictionary<GameObject, GameObject> ();

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

			mUseObjectPool[obj] = prefab;
			return obj;
		}

		public void recycle(GameObject t)
		{
			var prefab = mUseObjectPool [t];
			mFreeObjectPool [prefab].Enqueue (t);
		}

		public void release()
		{
			foreach (var v in mFreeObjectPool.Keys) {
				while (mFreeObjectPool [v].Count > 0) {
					var obj = mFreeObjectPool [v].Dequeue ();
					UnityEngine.Object.Destroy (obj);
				}
				UnityEngine.Object.Destroy (v);
			}
			mFreeObjectPool.Clear ();

			foreach (var v in mUseObjectPool.Keys) {
				UnityEngine.Object.Destroy (v);
			}
			mUseObjectPool.Clear ();
		}
	}
}