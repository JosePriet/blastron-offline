using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework
{
	public class CurveKeyCollection : IEnumerable, ICollection<CurveKey>, IEnumerable<CurveKey>
	{
		private List<CurveKey> innerlist;

		private bool isReadOnly;

		public CurveKey this[int index]
		{
			get
			{
				return innerlist[index];
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException();
				}
				if (index >= innerlist.Count)
				{
					throw new IndexOutOfRangeException();
				}
				if (innerlist[index].Position == value.Position)
				{
					innerlist[index] = value;
					return;
				}
				innerlist.RemoveAt(index);
				innerlist.Add(value);
			}
		}

		public int Count
		{
			get
			{
				return innerlist.Count;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return isReadOnly;
			}
		}

		public CurveKeyCollection()
		{
			innerlist = new List<CurveKey>();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return innerlist.GetEnumerator();
		}

		public void Add(CurveKey item)
		{
			if (item == null)
			{
				throw new ArgumentNullException("Value cannot be null.", (Exception)null);
			}
			if (innerlist.Count == 0)
			{
				innerlist.Add(item);
				return;
			}
			for (int i = 0; i < innerlist.Count; i++)
			{
				if (item.Position < innerlist[i].Position)
				{
					innerlist.Insert(i, item);
					return;
				}
			}
			innerlist.Add(item);
		}

		public void Clear()
		{
			innerlist.Clear();
		}

		public bool Contains(CurveKey item)
		{
			return innerlist.Contains(item);
		}

		public void CopyTo(CurveKey[] array, int arrayIndex)
		{
			innerlist.CopyTo(array, arrayIndex);
		}

		public IEnumerator<CurveKey> GetEnumerator()
		{
			return innerlist.GetEnumerator();
		}

		public bool Remove(CurveKey item)
		{
			return innerlist.Remove(item);
		}

		public CurveKeyCollection Clone()
		{
			CurveKeyCollection curveKeyCollection = new CurveKeyCollection();
			foreach (CurveKey item in innerlist)
			{
				curveKeyCollection.Add(item);
			}
			return curveKeyCollection;
		}

		public int IndexOf(CurveKey item)
		{
			return innerlist.IndexOf(item);
		}

		public void RemoveAt(int index)
		{
			if (index != Count && index > -1)
			{
				innerlist.RemoveAt(index);
				return;
			}
			throw new ArgumentOutOfRangeException("Index was out of range. Must be non-negative and less than the size of the collection.\r\nParameter name: index", (Exception)null);
		}
	}
}
