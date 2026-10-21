using System.Collections.Generic;

namespace EB.Collections
{
	public class Queue<T> : List<T>
	{
		public Queue()
		{
		}

		public Queue(int capacity)
			: base(capacity)
		{
		}

		public void Enqueue(T obj)
		{
			Add(obj);
		}

		public T Peek()
		{
			return this[0];
		}

		public T Dequeue()
		{
			T result = this[0];
			RemoveAt(0);
			return result;
		}
	}
}
