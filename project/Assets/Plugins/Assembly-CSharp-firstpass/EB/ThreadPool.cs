using System;
using System.Collections.Generic;
using System.Threading;

namespace EB
{
	public class ThreadPool
	{
		public class AsyncTask
		{
			public Exception exception;

			public bool done;

			public System.Action<object> action;

			public object state;
		}

		private readonly int _maxThreads;

		private Queue<AsyncTask> _queue;

		private AutoResetEvent _event;

		private bool _running;

		public ThreadPool(int maxThreads)
		{
			_maxThreads = maxThreads;
			_queue = new Queue<AsyncTask>(16);
			_event = new AutoResetEvent(false);
			Start();
		}

		public AsyncTask Queue(System.Action<object> action, object state)
		{
			AsyncTask asyncTask = new AsyncTask();
			asyncTask.action = action;
			asyncTask.state = state;
			lock (_queue)
			{
				_queue.Enqueue(asyncTask);
			}
			Wakeup();
			return asyncTask;
		}

		public void Wait()
		{
			while (JoinForOne())
			{
			}
		}

		private bool JoinForOne()
		{
			AsyncTask asyncTask = null;
			lock (_queue)
			{
				if (_queue.Count > 0)
				{
					asyncTask = _queue.Dequeue();
				}
			}
			if (asyncTask == null)
			{
				return false;
			}
			try
			{
				asyncTask.action(asyncTask.state);
			}
			catch (Exception exception)
			{
				asyncTask.exception = exception;
			}
			asyncTask.done = true;
			return true;
		}

		private void _Thread()
		{
			try
			{
				Thread.CurrentThread.Name = "threadpool";
			}
			catch
			{
			}
			while (_running)
			{
				if (!JoinForOne())
				{
					_event.WaitOne();
				}
			}
		}

		private void Start()
		{
			if (!_running)
			{
				_running = true;
				for (int i = 0; i < _maxThreads; i++)
				{
					Thread thread = new Thread(_Thread, 262144);
					thread.Start();
				}
			}
		}

		private void Wakeup()
		{
			try
			{
				_event.Set();
			}
			catch
			{
			}
		}
	}
}
