using System.Collections;

namespace EB.Sequence.Runtime
{
	public class EventList : ArrayList
	{
		private class EventCompare : IComparer
		{
			public int Compare(object e1, object e2)
			{
				return ((Event)e2).Priority - ((Event)e1).Priority;
			}
		}

		private bool _dirty;

		private static EventCompare _compare = new EventCompare();

		public void Add(Event e)
		{
			_dirty = true;
			base.Add(e);
		}

		public void Remove(Event e)
		{
			base.Remove(e);
		}

		public void Check()
		{
			if (_dirty)
			{
				_dirty = false;
				base.Sort(_compare);
			}
		}
	}
}
