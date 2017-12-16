using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace xk_System.DataStructure
{
	public class ListBuffer<T>
	{
		private T[] buffer = null;
		private int dataLength;

		public ListBuffer (int bufferSize = 1024)
		{
			dataLength = 0;
			buffer = new T[bufferSize];
		}

		public void reset ()
		{
			dataLength = 0;
		}

		public void release ()
		{
			buffer = null;
			this.reset ();
		}

		public int Length {
			get {
				return dataLength;
			}

			set {
				dataLength = value;
			}
		}

		public T[] Buffer {
			get {
				return buffer;
			}
		}

		public int nBeginWriteIndex {
			get {
				return dataLength;
			}
		}



		public void WriteFrom (T[] otherBuffer, int offset, int count)
		{
			if (count > dataLength) {
				buffer = new T[count];
			}

			Array.Copy (otherBuffer, offset, buffer, 0, count);
			dataLength = count;
		}
	}
}