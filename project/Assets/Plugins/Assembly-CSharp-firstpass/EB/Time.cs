using System;
using UnityEngine;

namespace EB
{
	public static class Time
	{
		private static int _offset = -1;

		private static DateTime _epoc = new DateTime(1970, 1, 1);

		public static bool Valid
		{
			get
			{
				return _offset > 0;
			}
		}

		public static int Now
		{
			get
			{
				return GetNow();
			}
			set
			{
				SetNow(value);
			}
		}

		public static float deltaTime
		{
			get
			{
				return UnityEngine.Time.deltaTime;
			}
		}

		public static float timeScale
		{
			get
			{
				return UnityEngine.Time.timeScale;
			}
			set
			{
				UnityEngine.Time.timeScale = value;
			}
		}

		public static float realtimeSinceStartup
		{
			get
			{
				return UnityEngine.Time.realtimeSinceStartup;
			}
		}

		private static int GetNow()
		{
			if (!Valid)
			{
				return ToPosixTime(DateTime.UtcNow);
			}
			try
			{
				return Mathf.FloorToInt(UnityEngine.Time.realtimeSinceStartup) + _offset;
			}
			catch
			{
				return ToPosixTime(DateTime.UtcNow);
			}
		}

		public static int Since(DateTime dt)
		{
			int b = (int)(DateTime.Now - dt).TotalSeconds;
			return Mathf.Max(0, b);
		}

		public static int Since(int t)
		{
			return Mathf.Max(0, Now - t);
		}

		private static void SetNow(int value)
		{
			_offset = value - Mathf.FloorToInt(UnityEngine.Time.realtimeSinceStartup);
		}

		public static int ToPosixTime(DateTime time)
		{
			return (int)(time - _epoc).TotalSeconds;
		}

		public static DateTime FromPosixTime(int time)
		{
			return _epoc + TimeSpan.FromSeconds(time);
		}
	}
}
