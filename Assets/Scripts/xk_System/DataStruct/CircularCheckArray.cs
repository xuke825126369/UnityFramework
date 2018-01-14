using System.Collections;
using System.Collections.Generic;
using System;

namespace xk_System.DataStructure
{
	public class CircularMap
	{
		private UInt16[] values;
		private int nBeginCheckIndex;
		private int nCheckLength;

		public CircularMap(int initCapacity = 32)
		{
			values = new ushort[initCapacity];
			nBeginCheckIndex = 0;
			nCheckLength = 0;
		}

		/*public UInt16 this [int index] {
			get {
				if (index >= this.Length) {
					throw new Exception ("环形缓冲区异常，索引溢出");
				}
				if (nBeginReadIndex + index < this.Capacity) {
					return this.Buffer [nBeginReadIndex + index];
				} else {
					return this.Buffer [nBeginReadIndex + index - this.Capacity];
				}
			}
		}

		public void Remove(UInt16 value)
		{
			if (Contains (value)) {
				checkPos [value] = false;
			}
		}

		public void Add(UInt16 value)
		{
			values [nCanAddPos] = value;
			keys [value] = nCanAddPos;
			checkPos [value] = true;
		}

		public bool Contains(UInt16 value)
		{
			int index = keys [value];
			values [index];
		}
		*/

	}
}