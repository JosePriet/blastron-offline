using System;
using System.Collections.Generic;
using EB;
using UnityEngine;

internal class CoreAssets : MonoBehaviour
{
	[System.Serializable]
	public class Info
	{
		public string path;

		public UnityEngine.Object asset;

		public int refCount;

		public AssetSource source;
	}

	public List<Info> Loaded = new List<Info>();

	private static CoreAssets _this;

	public static CoreAssets Instance
	{
		get
		{
			if (_this == null)
			{
				GameObject gameObject = new GameObject("_Assets");
				UnityEngine.Object.DontDestroyOnLoad(gameObject);
				gameObject.hideFlags = HideFlags.HideAndDontSave;
				_this = gameObject.AddComponent<CoreAssets>();
			}
			return _this;
		}
	}

	private int IndexOf(string path)
	{
		for (int i = 0; i < Loaded.Count; i++)
		{
			if (Loaded[i].path == path)
			{
				return i;
			}
		}
		return -1;
	}

	private int IndexOf(UnityEngine.Object asset)
	{
		for (int i = 0; i < Loaded.Count; i++)
		{
			if (Loaded[i].asset == asset)
			{
				return i;
			}
		}
		return -1;
	}

	public UnityEngine.Object Find(string path)
	{
		int num = IndexOf(path);
		if (num >= 0)
		{
			Info info = Loaded[num];
			info.refCount++;
			return info.asset;
		}
		return null;
	}

	private void OnDestroy()
	{
		Loaded.Clear();
	}

	public void Add(string path, UnityEngine.Object asset, AssetSource source)
	{
		int num = IndexOf(path);
		if (num == -1)
		{
			num = Loaded.Count;
			Loaded.Add(new Info
			{
				path = path,
				asset = asset,
				source = source
			});
		}
		else
		{
			EB.Debug.LogError("Error: loaded asset more than once for path " + path);
		}
		Loaded[num].refCount++;
	}

	public bool Unload(UnityEngine.Object asset)
	{
		int num = IndexOf(asset);
		if (num >= 0)
		{
			Info info = Loaded[num];
			info.refCount--;
			if (info.refCount == 0)
			{
				Unload(num);
				return true;
			}
		}
		return false;
	}

	public bool Unload(string name)
	{
		int num = IndexOf(name);
		if (num >= 0)
		{
			Info info = Loaded[num];
			info.refCount--;
			if (info.refCount == 0)
			{
				Unload(num);
				return true;
			}
		}
		return false;
	}

	private void Unload(int index)
	{
		Info info = Loaded[index];
		if (info.asset != null)
		{
			switch (info.source)
			{
			case AssetSource.Resources:
				if (!(info.asset is GameObject))
				{
					Resources.UnloadAsset(info.asset);
				}
				break;
			}
			info.asset = null;
		}
		Loaded[index] = Loaded[Loaded.Count - 1];
		Loaded.RemoveAt(Loaded.Count - 1);
	}
}
