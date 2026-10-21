using System.Collections.Generic;
using System.Text;
using EB;
using UnityEngine;

public static class FacebookPortraitManager
{
	private class PortraitInfo
	{
		public float time;

		public string path;

		public Texture2D texture;
	}

	private const int POOL_SIZE = 50;

	private static List<PortraitInfo> _texturePool = new List<PortraitInfo>(50);

	private static Texture2D unknownPortraitTexture;

	public static void Init()
	{
		string path = "Textures/fb_unknownportrait";
		unknownPortraitTexture = Resources.Load(path) as Texture2D;
	}

	public static void Load(string path, Action<Texture2D> cb)
	{
		PortraitInfo pi = _texturePool.Find((PortraitInfo o) => o.path.Equals(path));
		if (pi != null)
		{
			pi.time = UnityEngine.Time.realtimeSinceStartup;
			cb(pi.texture);
			return;
		}
		EB.Debug.Log("Loading texture for " + path);
		TexturePoolManager.Instance.LoadTexture(path, (Texture2D loadedTexture) =>
		{
			if (loadedTexture == null)
			{
				GameObject gameObject = null;
				gameObject.SetActive(true);
			}
			if (IsValidPortrait(loadedTexture))
			{
				pi = new PortraitInfo();
				pi.time = UnityEngine.Time.realtimeSinceStartup;
				pi.path = path;
				pi.texture = loadedTexture;
				_texturePool.Add(pi);
				if (_texturePool.Count > 50)
				{
					Evict();
				}
				DebugPrintInfo();
				cb(pi.texture);
			}
			else
			{
				cb(unknownPortraitTexture);
			}
		});
	}

	private static bool IsValidPortrait(Texture2D tex)
	{
		return tex.width != 8 || tex.height != 8;
	}

	public static void FlushAll()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("FacebookPortraitManager flushing:");
		foreach (PortraitInfo item in _texturePool)
		{
			stringBuilder.AppendLine(" PortraitInfo [" + item.path + "]: t[" + item.time + "], s[" + Profiler.GetRuntimeMemorySize(item.texture) + "]");
			Object.Destroy(item.texture);
		}
		EB.Debug.Log(stringBuilder.ToString());
		_texturePool.Clear();
	}

	private static void DebugPrintInfo()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Currently loaded portraits:");
		foreach (PortraitInfo item in _texturePool)
		{
			stringBuilder.AppendLine("PortraitInfo [" + item.path + "]: t[" + item.time + "], s[" + Profiler.GetRuntimeMemorySize(item.texture) + "]");
		}
		EB.Debug.Log(stringBuilder.ToString());
	}

	private static void Evict()
	{
		if (_texturePool.Count > 0)
		{
			_texturePool.Sort((PortraitInfo x, PortraitInfo y) => x.time.CompareTo(y.time));
			PortraitInfo portraitInfo = _texturePool[0];
			EB.Debug.Log("Evicting PortraitInfo [" + portraitInfo.path + "]: t[" + portraitInfo.time + "], s[" + Profiler.GetRuntimeMemorySize(portraitInfo.texture) + "]");
			Object.Destroy(portraitInfo.texture);
			_texturePool.RemoveAt(0);
		}
	}
}
