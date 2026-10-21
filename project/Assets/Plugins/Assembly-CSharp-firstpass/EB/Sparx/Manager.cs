using System;
using System.Collections;
using UnityEngine;

namespace EB.Sparx
{
	public abstract class Manager : IDisposable
	{
		protected Hub Hub
		{
			get
			{
				return Hub.Instance;
			}
		}

		public abstract void Initialize(Config config);

		public abstract IEnumerator LoadOfflineData();

		protected Coroutine StartCoroutine(IEnumerator fn)
		{
			return Hub.StartCoroutine(fn);
		}

		public virtual void OnLoggedIn()
		{
		}

		public virtual void OnEnteredBackground()
		{
		}

		public virtual void OnEnteredForeground()
		{
		}

		public virtual void Async(string message, object payload)
		{
		}

		public virtual void Dispose()
		{
		}
	}
}
