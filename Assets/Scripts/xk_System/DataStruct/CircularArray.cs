using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using xk_System.Debug;
using System.Text;

namespace xk_System.DataStructure
{
	/// <summary>
	/// 适用于 频繁的修改数组
	/// </summary>
	public class CircularBuffer<T>
	{
		private T[] Buffer = null;
		private int dataLength;
		private int nBeginReadIndex;
		private int nBeginWriteIndex;

		public CircularBuffer (int bufferSize)
		{
			nBeginReadIndex = 0;
			nBeginWriteIndex = 0;
			dataLength = 0;
			Buffer = new T[bufferSize];
		}

		public void reset()
		{
			dataLength = 0;
			nBeginReadIndex = 0;
			nBeginWriteIndex = 0;
		}

		public void release()
		{
			Buffer = null;
			this.reset ();
		}

		public int Capacity
		{
			get {
				return this.Buffer.Length;
			}
		}

		public int Length
		{
			get {
				return this.dataLength;
			}
		}

		public T this [int index] {
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
			
		public void WriteFrom (T[] writeBuffer, int offset, int count)
		{
			if (writeBuffer.Length < count) {
				count = writeBuffer.Length;
			}

			if (count <= 0) {
				return;
			}

			if (this.Capacity - this.Length >= count) {                          
				if (nBeginWriteIndex + count <= this.Capacity) {        
					Array.Copy (writeBuffer, offset, this.Buffer, nBeginWriteIndex, count);
				} else {
					int Length1 = this.Buffer.Length - nBeginWriteIndex;
					int Length2 = count - Length1;
					Array.Copy (writeBuffer, offset, this.Buffer, nBeginWriteIndex, Length1);
					Array.Copy (writeBuffer, offset + Length1, this.Buffer, 0, Length2);
				}

				dataLength += count;
				nBeginWriteIndex += count;
				if (nBeginWriteIndex >= this.Capacity) {
					nBeginWriteIndex -= this.Capacity;
				}
			} else {
				throw new Exception ("环形缓冲区 写 溢出");
			}
		}

		public void WriteFrom (CircularBuffer<T> writeBuffer, int count)
		{
			if (writeBuffer.Length < count) {
				count = writeBuffer.Length;
			}

			if (count <= 0) {
				return;
			}

			if (this.Capacity - this.Length >= count) {                          
				if (nBeginWriteIndex + count <= this.Capacity) {
					for (int i = 0; i < count; i++) {
						this.Buffer [nBeginWriteIndex + i] = writeBuffer [i];
					}
				} else {    
					int Length1 = this.Capacity - nBeginWriteIndex;
					int Length2 = count - Length1;

					for (int i = 0; i < Length1; i++) {
						this.Buffer [nBeginWriteIndex + i] = writeBuffer [i];
					}
						
					for (int i = 0; i < Length2; i++) {
						this.Buffer [i] = writeBuffer [Length1 + i];
					}
				}

				dataLength += count;
				nBeginWriteIndex += count;
				if (nBeginWriteIndex >= this.Capacity) {
					nBeginWriteIndex -= this.Capacity;
				}

				writeBuffer.ClearBuffer (count);
			} else {
				throw new Exception ("环形缓冲区 写 溢出");
			}
		}

		public int WriteTo (int index, T[] readBuffer, int offset, int count)
		{
			int readCount = CopyTo (index, readBuffer, offset, count);
			this.ClearBuffer (index + count);
			return readCount;
		}

		public int CopyTo(int index, T[] readBuffer, int offset, int copyLength)
		{
			if (copyLength > this.Length - index) {
				copyLength = this.Length - index;
			}

			if (copyLength <= 0) {
				return 0;
			}

			int tempBeginIndex = nBeginReadIndex + index;

			if (tempBeginIndex + copyLength <= this.Capacity) {
				Array.Copy (this.Buffer, tempBeginIndex, readBuffer, offset, copyLength);
			} else {
				int Length1 = this.Capacity - tempBeginIndex;
				int Length2 = copyLength - Length1;
				if (Length1 > 0) {
					Array.Copy (this.Buffer, tempBeginIndex, readBuffer, offset, Length1);
				}
				Array.Copy (this.Buffer, 0, readBuffer, offset + Length1, Length2);
			}
			return copyLength;
		}

		public void ClearBuffer (int readLength)
		{
			if (readLength >= this.Length) {
				this.reset ();
			} else {
				dataLength -= readLength;
				nBeginReadIndex += readLength;
				if (nBeginReadIndex >= this.Capacity) {
					nBeginReadIndex -= this.Capacity;
				}
			}
		}

		public void Print()
		{
			StringBuilder aaStr = new StringBuilder ();
			aaStr.Append ("<color=red>");
			aaStr.Append (this.GetType ().Name + ": ");
			aaStr.Append ("</color>");
			aaStr.Append ("<color=yellow>");
			for (int i = 0; i < Length; i++) {
				aaStr.Append (this [i] + " | ");
			}
			aaStr.Append ("</color>");
			DebugSystem.Log (aaStr);
		}
	}
}








