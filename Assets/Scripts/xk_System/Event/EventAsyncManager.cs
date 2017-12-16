using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace xk_System
{
	public class EventAsyncManager : SingleTonMonoBehaviour<EventAsyncManager>
	{
		Dictionary<int, List<Action<object>>> mEventDic = new Dictionary<int,List<Action<object>>>();
		Queue<EventData> mEventDataQueue = new Queue<EventData> ();
		ListGCPool<Action<object>> mActionPool = new  ListGCPool<Action<object>>();

		private struct EventData
		{
			public int eventId;
			public object data;
		}

		public void AddFunListen(int eventId, Action<object> mEventFunc)
		{
			if (!mEventDic.ContainsKey (eventId)) {
				mEventDic [eventId] = mActionPool.Pop ();
			}
			mEventDic [eventId].Add (mEventFunc);
		}

		public void RemoveFunListen(int eventId, Action<object> mEventFunc)
		{
			var mlist = mEventDic [eventId];
			mlist.Remove (mEventFunc);
			if (mlist.Count == 0)
			{
				mActionPool.recycle (mlist);
				mEventDic.Remove (eventId);
			}
		}

		public void RemoveFunListen(int eventId)
		{
			List<Action<object>> mList = mEventDic [eventId];
			mList.Clear ();
			mActionPool.recycle (mList);
			mEventDic.Remove (eventId);
		}

		public void DispatchEvent(int eventId, object mdata)
		{
			EventData mEventData = new EventData ();
			mEventData.eventId = eventId;
			mEventData.data = mdata;
			mEventDataQueue.Enqueue (mEventData);
		}

		void Update ()
		{
			if (mEventDataQueue.Count > 0) {
				float spendTime = 0.0f;
				while (mEventDataQueue.Count > 0) {
					float lastTime = Time.realtimeSinceStartup;

					EventData mEventData = mEventDataQueue.Dequeue ();
					var mlist = mEventDic [mEventData.eventId];
					var Iter = mlist.GetEnumerator ();
					while (Iter.MoveNext ()) {
						Iter.Current (mEventData.data);
					}

					spendTime += Time.realtimeSinceStartup - lastTime;
					if (spendTime > 0.034f) {
						break;
					}
				}
			}
		}
	}

	public class EventAsyncManager<T>
	{
		Dictionary<int, List<Action<T>>> mEventDic = new Dictionary<int,List<Action<T>>>();
		Queue<EventData> mEventDataQueue = new Queue<EventData> ();
		ListGCPool<Action<T>> mActionPool = new  ListGCPool<Action<T>>();

		private struct EventData
		{
			public int eventId;
			public T data;
		}

		public void AddFunListen(int eventId, Action<T> mEventFunc)
		{
			if (!mEventDic.ContainsKey (eventId)) {
				mEventDic [eventId] = mActionPool.Pop ();
			}
			mEventDic [eventId].Add (mEventFunc);
		}

		public void RemoveFunListen(int eventId, Action<T> mEventFunc)
		{
			var mlist = mEventDic [eventId];
			mlist.Remove (mEventFunc);
			if (mlist.Count == 0)
			{
				mActionPool.recycle (mlist);
				mEventDic.Remove (eventId);
			}
		}

		public void RemoveFunListen(int eventId)
		{
			List<Action<T>> mList = mEventDic [eventId];
			mList.Clear ();
			mActionPool.recycle (mList);
			mEventDic.Remove (eventId);
		}

		public void DispatchEvent(int eventId, T mdata)
		{
			EventData mEventData = new EventData ();
			mEventData.eventId = eventId;
			mEventData.data = mdata;
			mEventDataQueue.Enqueue (mEventData);
		}
	
		void Update ()
		{
			if (mEventDataQueue.Count > 0) {
				float spendTime = 0.0f;
				while (mEventDataQueue.Count > 0) {
					float lastTime = Time.realtimeSinceStartup;

					EventData mEventData = mEventDataQueue.Dequeue ();
					var mlist = mEventDic [mEventData.eventId];
					var Iter = mlist.GetEnumerator ();
					while (Iter.MoveNext ()) {
						Iter.Current (mEventData.data);
					}

					spendTime += Time.realtimeSinceStartup - lastTime;
					if (spendTime > 0.034f) {
						break;
					}
				}
			}
		}
	}
}