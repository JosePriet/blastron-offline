using System.Collections.Generic;
using UnityEngine;

namespace EB
{
	public static class Loader
	{
		public static bool CanUseCache = false;

		private static string _dataPath = string.Empty;

		private static Dictionary<string, string> _packs = new Dictionary<string, string>();

		public static string Folder
		{
			get
			{
				return "android/";
			}
		}

		public static string DataPath
		{
			get
			{
				if (string.IsNullOrEmpty(_dataPath))
				{
					if (Application.isWebPlayer)
					{
						_dataPath = Application.dataPath + "/" + Folder;
					}
					else if (Application.platform == RuntimePlatform.Android)
					{
						_dataPath = "jar:file://" + Application.dataPath + "!/assets/";
					}
					else if (Application.platform == RuntimePlatform.IPhonePlayer)
					{
						_dataPath = "file://" + Application.dataPath + "/" + Folder;
					}
					else if (Application.platform == RuntimePlatform.OSXPlayer)
					{
						_dataPath = "file://" + Application.dataPath + "/../../" + Folder;
					}
					else
					{
						_dataPath = "file://" + Application.dataPath + "/../" + Folder;
					}
				}
				return _dataPath;
			}
		}

		public static void OverridePath(string pack, string path)
		{
			_packs[pack] = path;
		}

		public static string GetBaseURL(string pack)
		{
			string value = string.Empty;
			if (_packs.TryGetValue(pack, out value))
			{
				return value;
			}
			return DataPath + pack + "/";
		}

		public static WWW Load(string pack, string path)
		{
			string text = GetBaseURL(pack) + path;
			if (CanUseCache)
			{
				Debug.Log("Loading " + text + " from cache");
				return WWW.LoadFromCacheOrDownload(text, 1);
			}
			Debug.Log("Loading " + text);
			return new WWW(text);
		}

		public static string GetBundlePath(string bundleId)
		{
			string arg = string.Empty;
			string arg2 = bundleId;
			int num = bundleId.LastIndexOf('/');
			if (num >= 0)
			{
				arg = bundleId.Substring(0, num + 1);
				arg2 = bundleId.Substring(num + 1);
			}
			return string.Format("{0}bundle_{1}.assetbundle", arg, arg2);
		}

		public static WWW LoadBundle(string pack, string bundleId)
		{
			string bundlePath = GetBundlePath(bundleId);
			return Load(pack, bundlePath);
		}

		public static WWW LoadScene(string pack, string name)
		{
			string path = string.Format("scene_{0}.assetbundle", name);
			return Load(pack, path);
		}
	}
}
