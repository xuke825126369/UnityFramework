using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using xk_System.Debug;

namespace xk_System.DataStructure
{
	/// <summary>
	/// 适用于 频繁的修改数组
	/// </summary>
	public class CircularBuffer<T>
	{
		private T[] Buffer = null;
		private int dataLength;
		private int nBeginIndex;
		private int nEndIndex;

		public CircularBuffer (int bufferSize)
		{
			nBeginIndex = -1;
			nEndIndex = -1;
			dataLength = 0;
			Buffer = new T[bufferSize];
		}

		public void reset()
		{
			dataLength = 0;
			nBeginIndex = -1;
			nEndIndex = -1;
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
				if (nBeginIndex + index < this.Capacity) {
					return this.Buffer [nBeginIndex + index];
				} else {
					return this.Buffer [nBeginIndex + index - this.Capacity];
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

			if (this.Buffer.Length - dataLength >= count) {                          
				if (nEndIndex + count < this.Capacity) {        
					Array.Copy (writeBuffer, offset, this.Buffer, nEndIndex + 1, count);
					nEndIndex += count;
				} else {    
					int Length1 = this.Buffer.Length - nEndIndex - 1;
					int Length2 = count - Length1;
					Array.Copy (writeBuffer, offset, this.Buffer, nEndIndex + 1, Length1);
					Array.Copy (writeBuffer, offset + Length1, this.Buffer, 0, Length2);
					nEndIndex = Length2 - 1;
				}

				dataLength += count;
			} else {
				throw new Exception ("环形缓冲区 溢出");
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

			int initIndex1 = -1;
			int initIndex2 = -1;
			if (this.Buffer.Length - dataLength >= count) {                          
				if (nEndIndex + count < this.Capacity) {
					initIndex1 = nEndIndex + 1;
					for (int i = 0; i < count; i++) {
						this.Buffer [initIndex1 + i] = writeBuffer [i];
					}
					nEndIndex += count;
					writeBuffer.nBeginIndex += count;
				} else {    
					int Length1 = this.Capacity - nEndIndex - 1;
					int Length2 = count - Length1;

					initIndex1 = nEndIndex + 1;
					for (int i = 0; i < Length1; i++) {
						this.Buffer [nEndIndex + 1 + i] = writeBuffer [i];
					}

					initIndex2 = Length1;
					for (int i = 0; i < Length2; i++) {
						this.Buffer [i] = writeBuffer [initIndex2 + i];
					}

					nEndIndex = Length2 - 1;
					writeBuffer.nBeginIndex += count;
					if (writeBuffer.nBeginIndex >= writeBuffer.Capacity) {
						writeBuffer.nBeginIndex -= writeBuffer.Capacity;
					}
				}

				dataLength += count;
				writeBuffer.dataLength -= count;
			} else {
				throw new Exception ("环形缓冲区 溢出");
			}
		}

		public int WriteTo (T[] readBuffer, int offset, int count)
		{
			if (count > this.Length) {
				count = this.Length;
			}

			if (count <= 0) {
				return 0;
			}

			if (nBeginIndex + count < this.Capacity) {
				Array.Copy (this.Buffer, nBeginIndex, readBuffer, offset, count);
				nBeginIndex += count;
			} else {
				int Length1 = this.Capacity - nBeginIndex - 1;
				int Length2 = count - Length1;
				if (Length1 > 0) {
					Array.Copy (this.Buffer, nBeginIndex, readBuffer, offset, Length1);
				}
				Array.Copy (this.Buffer, 0, readBuffer, offset + Length1, Length2);

				nBeginIndex = Length2;
			}

			dataLength -= count;
			return count;
		}

		public int CopyTo(int index, T[] readBuffer, int offset, int copyLength)
		{
			if (copyLength > this.Length - index) {
				copyLength = this.Length - index;
			}

			if (copyLength <= 0) {
				return 0;
			}

			int tempBeginIndex = nBeginIndex + index;

			if (tempBeginIndex + copyLength < this.Capacity) {
				Array.Copy (this.Buffer, tempBeginIndex, readBuffer, offset, copyLength);
			} else {
				int Length1 = this.Capacity - tempBeginIndex - 1;
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
				if (nBeginIndex + readLength < this.Capacity) {
					nBeginIndex += readLength;
				} else {
					nBeginIndex = readLength - (this.Capacity - nBeginIndex);
				}

				dataLength -= readLength;
			}
		}
	}
}








