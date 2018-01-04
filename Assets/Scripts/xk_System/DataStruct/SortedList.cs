using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace xk_System.DataStructure
{
	public class SortedList<T>
	{
		public List<T> mlist = null;

		public SortedList(int capacity = 4)
		{
			mlist = new List<T> (capacity);

			//System.Collections.Generic.SortedList<int,int> mm;
			//mm [1];

		}
	}
}