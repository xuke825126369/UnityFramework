using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using xk_System.Debug;

namespace xk_System.Event
{
	public class DataBind<T>
	{
		public T bindData = default(T);
		private event Action<T> bindEvent = null;

		public DataBind ()
		{
			
		}

		public DataBind (T t)
		{
			bindData = t;
		}

		public void release()
		{
			bindData = default(T);
			bindEvent = null;
		}

		public void DispatchEvent ()
		{
			if (bindEvent != null) {
				bindEvent (bindData);
			}
		}

		public void addDataBind (Action<T> fun)
		{
			if (!CheckDataBinFunIsExist (fun)) {
				bindEvent += fun;
			} else {
				DebugSystem.LogError ("addDataBind Error: fun Repetition");
			}
		}

		private bool CheckDataBinFunIsExist (Action<T> fun)
		{
			return DelegateUtility.CheckFunIsExist (bindEvent, fun);
		}

		public void removeDataBind (Action<T> fun)
		{
			bindEvent -= fun;
		}
	}
}