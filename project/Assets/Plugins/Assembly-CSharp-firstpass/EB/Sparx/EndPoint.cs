using System;
using System.Collections;
using UnityEngine;

namespace EB.Sparx
{
	public abstract class EndPoint : IDisposable
	{
		public EndPointOptions Options { get; private set; }

		public string Url { get; private set; }

		protected bool HasInternetConnectivity
		{
			get
			{
				return Application.internetReachability != NetworkReachability.NotReachable;
			}
		}

		public EndPoint(string url, EndPointOptions options)
		{
			Url = url;
			Options = options;
			if (Hub.Instance != null)
			{
				Hub.Instance.OnUpdate += Update;
			}
		}

		public virtual void StartKeepAlive()
		{
		}

		public virtual void StopKeepAlive()
		{
		}

		public virtual void Dispose()
		{
			try
			{
				if (Hub.Instance != null)
				{
					Hub.Instance.OnUpdate -= Update;
				}
			}
			catch
			{
			}
		}

		public virtual void RPC(string name, ArrayList args, Action<string, object> callback)
		{
			throw new NotSupportedException();
		}

		public virtual Request Get(string path)
		{
			return new Request(Url + path, false);
		}

		public virtual Request Post(string path)
		{
			return new Request(Url + path, true);
		}

		public virtual void AddData(string key, string value)
		{
		}

		public virtual string GetData(string key)
		{
			return string.Empty;
		}

		public virtual void Update()
		{
		}

		public abstract int Service(Request request, Action<Response> callback);

		public abstract bool IsDone(int requestId);

		public Coroutine ServiceCoroutine(Request request, Action<Response> callback)
		{
			int requestId = Service(request, callback);
			return Wait(requestId);
		}

		public Coroutine Wait(int requestId)
		{
			return Coroutines.Run(_Wait(requestId));
		}

		private IEnumerator _Wait(int requestId)
		{
			while (!IsDone(requestId))
			{
				yield return 1;
			}
		}
	}
}
