using System.Collections;

namespace EB.Collections
{
	public class CircularBuffer : IEnumerable
	{
		private object[] _buffer;

		private int _head;

		private int _tail;

		public int Count
		{
			get
			{
				return (_tail + _buffer.Length - _head) % _buffer.Length;
			}
		}

		public CircularBuffer(int size)
		{
			_buffer = new object[size];
		}

		public void Clear()
		{
			_head = (_tail = 0);
		}

		public object[] ToArray()
		{
			object[] array = new object[Count];
			for (int i = 0; i < array.Length; i++)
			{
				int num = (_head + i) % _buffer.Length;
				array[i] = _buffer[num];
			}
			return array;
		}

		public void Push(object obj)
		{
			_buffer[_tail] = obj;
			_tail = (_tail + 1) % _buffer.Length;
			if (_tail == _head)
			{
				_head = (_head + 1) % _buffer.Length;
			}
		}

		public IEnumerator GetEnumerator()
		{
			for (int i = _head; i != _tail; i = (i + 1) % _buffer.Length)
			{
				yield return _buffer[i];
			}
		}
	}
}
