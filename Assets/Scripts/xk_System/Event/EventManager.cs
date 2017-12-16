using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace xk_System.Event
{
	public class EventSyncManager : Singleton<EventSyncManager> 
	{
		Dictionary<int, List<Action<object>>> mEventDic = new Dictionary<int,List<Action<object>>>();
		ListGCPool<Action<object>> mActionPool = new  ListGCPool<Action<object>>();
		public override void Init ()
		{
			base.Init ();
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
			var mlist = mEventDic [eventId];
			var Iter = mlist.GetEnumerator ();
			while (Iter.MoveNext ()) {
				Iter.Current (mdata);
			}
		}
	}

	public class EventSyncManager<T> 
	{
		Dictionary<int, List<Action<T>>> mEventDic = new Dictionary<int,List<Action<T>>>();
		ListGCPool<Action<T>> mActionPool = new  ListGCPool<Action<T>>();

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
			var mlist = mEventDic [eventId];
			var Iter = mlist.GetEnumerator ();
			while (Iter.MoveNext ()) {
				Iter.Current (mdata);
			}
		}
	}
}