using System.Collections;
using UnityEngine;

namespace EB
{
	public class Coroutines
	{
		private class IntervalHandle
		{
			public bool Running = true;

			public Coroutine Couroutine;
		}

		private static CoreCoroutines _this;

		public static Coroutine Run(IEnumerator function)
		{
			if (_this == null)
			{
				GameObject gameObject = new GameObject("COROUTINES");
				gameObject.hideFlags = HideFlags.HideAndDontSave;
				Object.DontDestroyOnLoad(gameObject);
				_this = gameObject.AddComponent<CoreCoroutines>();
			}
			return _this.StartCoroutine(function);
		}

		public static object SetInterval(Action cb, int ms)
		{
			IntervalHandle intervalHandle = new IntervalHandle();
			Run(_SetInterval(intervalHandle, cb, ms));
			return intervalHandle;
		}

		public static void ClearInterval(object handle)
		{
			if (handle != null && handle is IntervalHandle)
			{
				((IntervalHandle)handle).Running = false;
			}
		}

		private static IEnumerator _SetInterval(IntervalHandle handle, Action cb, int ms)
		{
			float seconds = (float)ms / 1000f;
			while (true)
			{
				float end = Time.realtimeSinceStartup + seconds;
				while (Time.realtimeSinceStartup < end)
				{
					yield return 1;
				}
				if (handle.Running)
				{
					cb();
					continue;
				}
				break;
			}
		}

		public static object SetTimeout(Action cb, int ms)
		{
			IntervalHandle intervalHandle = new IntervalHandle();
			intervalHandle.Couroutine = Run(_SetTimeout(intervalHandle, cb, ms));
			return intervalHandle;
		}

		public static void ClearTimeout(object handle)
		{
			if (handle != null && handle is IntervalHandle)
			{
				((IntervalHandle)handle).Running = false;
			}
		}

		private static IEnumerator _SetTimeout(IntervalHandle handle, Action cb, int ms)
		{
			float seconds = (float)ms / 1000f;
			yield return new WaitForSeconds(seconds);
			if (handle.Running)
			{
				cb();
			}
		}

		public static Coroutine NextFrame(Action cb)
		{
			return Run(_NextFrame(cb));
		}

		private static IEnumerator _NextFrame(Action cb)
		{
			yield return 1;
			cb();
		}

		public static Coroutine EndOfFrame(Action cb)
		{
			return Run(_EndOfFrame(cb));
		}

		private static IEnumerator _EndOfFrame(Action cb)
		{
			yield return new WaitForEndOfFrame();
			cb();
		}
	}
}
