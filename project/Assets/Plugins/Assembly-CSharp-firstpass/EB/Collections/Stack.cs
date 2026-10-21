using System.Collections.Generic;

namespace EB.Collections
{
	public class Stack<T> : List<T>
	{
		public void Push(T obj)
		{
			Add(obj);
		}

		public T Peek()
		{
			return this[Count - 1];
		}

		public T Pop()
		{
			T result = this[Count - 1];
			RemoveAt(Count - 1);
			return result;
		}
	}
}
