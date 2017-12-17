using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using xk_System.Debug;
using System.Text;

namespace xk_System.DataStructure
{
	public class QueueArraySegment<T>
	{
		private List<T[]> bufferQueue = null;
		private int nCurrentSegmentLength;
		private int usedSegmentCount;
		private readonly int nSegmentSize;

		public QueueArraySegment (int capacity = 100, int segmentSize = 1024)
		{
			nSegmentSize = segmentSize;
			bufferQueue = new List<T[]> (capacity);
			for (int i = 0; i < capacity; i++) {
				T[] segObj = new T[nSegmentSize];
				bufferQueue.Add (segObj);
			}

			usedSegmentCount = 0;
			nCurrentSegmentLength = 0;
		}

		public void reset ()
		{
			usedSegmentCount = 0;
			nCurrentSegmentLength = 0;
		}

		public void release ()
		{
			bufferQueue.Clear ();
			bufferQueue = null;
		}

		public int Capacity {
			get {
				return bufferQueue.Count * nSegmentSize;
			}
		}

		public int Length {
			get {
				return usedSegmentCount * nSegmentSize + nCurrentSegmentLength;
			}
		}

		public T[] ToArray() {
			if (Length > 0) {
				T[] mBuffer = new T[Length];
				for (int i = 0; i < usedSegmentCount; i++) {
					Array.Copy (bufferQueue [i], 0, mBuffer, i * nSegmentSize, nSegmentSize);
				}

				if (nCurrentSegmentLength > 0) {
					Array.Copy (bufferQueue [usedSegmentCount], 0, mBuffer, usedSegmentCount * nSegmentSize, nCurrentSegmentLength);
				}
				return mBuffer;
			} else {
				return null;
			}
		}

		public void WriteFrom (T[] otherBuffer, int offset, int count)
		{
			int remainCount = count;
			int remainSegmentCount = nSegmentSize - nCurrentSegmentLength;

			if (remainCount <= remainSegmentCount) {
				Array.Copy (otherBuffer, offset, bufferQueue [usedSegmentCount], nCurrentSegmentLength, remainCount);
				nCurrentSegmentLength += remainCount;
				if (nCurrentSegmentLength == nSegmentSize) {
					nCurrentSegmentLength = 0;
					usedSegmentCount++;

					int newSegmentCount = this.bufferQueue.Count - this.usedSegmentCount;
					if (newSegmentCount <= 0) {
						for (int i = newSegmentCount; i <= 0; i++) {
							T[] segObj = new T[nSegmentSize];
							bufferQueue.Add (segObj);
						}
					}
				}
			} else {
				Array.Copy (otherBuffer, offset, bufferQueue [usedSegmentCount], nCurrentSegmentLength, remainSegmentCount);
				usedSegmentCount++;
				nCurrentSegmentLength = 0;

				remainCount -= remainSegmentCount;
				int needSegmentCount = remainCount / nSegmentSize;
				offset += remainSegmentCount;

				int newSegmentCount = this.bufferQueue.Count - this.usedSegmentCount - needSegmentCount;
				if (newSegmentCount < 0) {
					for (int i = newSegmentCount; i < 0; i++) {
						T[] segObj = new T[nSegmentSize];
						bufferQueue.Add (segObj);
					}
				}

				for (int i = 0; i < needSegmentCount; i++) {
					Array.Copy (otherBuffer, offset, bufferQueue [usedSegmentCount], 0, nSegmentSize);
					usedSegmentCount++;

					offset += nSegmentSize;
				}

				remainCount = remainCount % nSegmentSize;
				if (remainCount > 0) {
					newSegmentCount = this.bufferQueue.Count - this.usedSegmentCount;
					if (newSegmentCount <= 0) {
						for (int i = newSegmentCount; i <= 0; i++) {
							T[] segObj = new T[nSegmentSize];
							bufferQueue.Add (segObj);
						}
					}

					Array.Copy (otherBuffer, offset, bufferQueue [usedSegmentCount], 0, remainCount);
					nCurrentSegmentLength = remainCount;
				}
			}
		}

		public override string ToString ()
		{
			T[] mBuffer = ToArray ();

			if (mBuffer != null) {
				StringBuilder aaStr = new StringBuilder ();
				aaStr.Append ("<color=red>");
				aaStr.Append (this.GetType ().Name + ": ");
				aaStr.Append ("</color>");
				aaStr.Append ("<color=yellow>");
				for (int i = 0; i < Length; i++) {
					aaStr.Append (mBuffer[i].ToString() + " | ");
				}
				aaStr.Append ("</color>");
				return aaStr.ToString ();
			} else {
				return this.GetType ().Name + ": empty";
			}
		}
	}
}