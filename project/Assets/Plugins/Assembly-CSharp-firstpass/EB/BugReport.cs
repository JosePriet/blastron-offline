using System;
using System.Collections;
using UnityEngine;

namespace EB
{
	public static class BugReport
	{
		private static string _url;

		private static Hashtable _data = new Hashtable();

		private static bool _sent;

		public static bool DidCrash
		{
			get
			{
				return _sent;
			}
		}

		public static event Action OnBugReport;

		public static void Init(string url)
		{
			_url = url.Replace("https://", "http://");
			Application.RegisterLogCallback(LogCallback);
			AddData("os", SystemInfo.operatingSystem);
			AddData("sl", SystemInfo.graphicsShaderLevel);
			AddData("graphics", SystemInfo.graphicsDeviceName);
			AddData("unity", Application.unityVersion);
			AddData("version", Version.GetVersion());
			AddData("changelist", Version.GetChangeList());
			AddData("locale", Version.GetLocale());
			AddData("device", SystemInfo.deviceModel);
			AddData("processor", SystemInfo.processorType);
		}

		public static void AddData(string key, object value)
		{
			_data[key] = value;
		}

		private static IEnumerator SendBugReport()
		{
			Debug.Dump(_data);
			yield return new WaitForEndOfFrame();
			TakeScreenShot();
			try
			{
				if (OnBugReport != null)
				{
					OnBugReport();
				}
			}
			catch
			{
			}
			if (!Application.isEditor)
			{
				string json = JSON.Stringify(_data);
				byte[] bytes = Encoding.GetBytes(json);
				Hashtable headers = new Hashtable();
				headers["Content-Type"] = "application/json";
				yield return new WWW(_url, bytes, headers);
			}
		}

		private static void TakeScreenShot()
		{
			try
			{
				if (Camera.main != null)
				{
					Texture2D texture2D = new Texture2D(Screen.width, Screen.height, TextureFormat.ARGB32, false);
					texture2D.ReadPixels(new Rect(0f, 0f, Screen.width, Screen.height), 0, 0);
					texture2D.Apply();
					float scale = 360f / (float)Screen.width;
					JPGEncoder jPGEncoder = new JPGEncoder(texture2D, 25f, scale);
					byte[] bytes = jPGEncoder.GetBytes();
					UnityEngine.Object.Destroy(texture2D);
					AddData("screen", Encoding.ToBase64String(bytes));
				}
			}
			catch (Exception ex)
			{
				UnityEngine.Debug.LogWarning("Failed to create screenshot " + ex);
			}
		}

		private static void LogCallback(string condition, string stackTrace, LogType type)
		{
			if (!_sent && type == LogType.Exception)
			{
				_sent = true;
				AddData("type", "exception");
				AddData("condition", condition);
				AddData("stack", stackTrace);
				Coroutines.Run(SendBugReport());
			}
		}
	}
}
