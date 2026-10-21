using System.Collections;
using System.Collections.Generic;
using EB;
using EB.Collections;
using UnityEngine;

public class TexturePoolManager : MonoBehaviour
{
	private class TextureInfo
	{
		public string textureName;

		public Texture2D texture;

		public float time;

		public List<Action<Texture2D>> callbacks = new List<Action<Texture2D>>();

		public int refCount;

		public int size
		{
			get
			{
				if (texture != null)
				{
					return texture.width * texture.height;
				}
				return 0;
			}
		}

		public void DoCallbacks()
		{
			while (callbacks.Count > 0)
			{
				Action<Texture2D> action = callbacks[callbacks.Count - 1];
				callbacks.RemoveAt(callbacks.Count - 1);
				if (action != null)
				{
					action(texture);
				}
			}
		}
	}

	private const int kMaxConcurrentRequests = 4;

	private const int kPoolSize = 16;

	private const int kGCAfterEvictions = 4;

	private static TexturePoolManager _this;

	private int _concurrentRequests;

	private int _evictions;

	private List<TextureInfo> _all = new List<TextureInfo>();

	private EB.Collections.Queue<string> _loads = new EB.Collections.Queue<string>();

	public static TexturePoolManager Instance
	{
		get
		{
			return _this;
		}
	}

	private void Awake()
	{
		_this = this;
	}

	private void FixedUpdate()
	{
		if (_loads.Count > 0 && _concurrentRequests < 4)
		{
			string textureName = _loads.Dequeue();
			TextureInfo textureInfo = GetTextureInfo(textureName);
			if (textureInfo != null && textureInfo.texture == null)
			{
				_concurrentRequests++;
				StartCoroutine(ProcessNextTexture(textureInfo));
			}
		}
	}

	private TextureInfo GetTextureInfo(string textureName)
	{
		foreach (TextureInfo item in _all)
		{
			if (item.textureName == textureName)
			{
				return item;
			}
		}
		return null;
	}

	private void Print()
	{
	}

	private void Evict()
	{
		List<TextureInfo> list = new List<TextureInfo>();
		foreach (TextureInfo item in _all)
		{
			if (item.refCount <= 0)
			{
				list.Add(item);
			}
		}
		if (list.Count > 16)
		{
			list.Sort((TextureInfo ti1, TextureInfo ti2) =>
			{
				if (ti1.size == ti2.size)
				{
					if (ti1.time > ti2.time)
					{
						return 1;
					}
					if (ti1.time < ti2.time)
					{
						return -1;
					}
					return 0;
				}
				return ti2.size - ti1.size;
			});
			Print();
			int num = list.Count - 16;
			for (int num2 = 0; num2 < list.Count && num2 < num; num2++)
			{
				TextureInfo textureInfo = list[num2];
				Object.Destroy(textureInfo.texture);
				_all.Remove(textureInfo);
				_evictions++;
			}
		}
		if (_evictions >= 4)
		{
			_evictions = 0;
		}
	}

	public static string GetUrl(string texture)
	{
		if (texture.StartsWith("http://") || texture.StartsWith("https://") || texture.StartsWith("file://"))
		{
			return texture;
		}
		return Assets.FindDiskAsset(texture);
	}

	private IEnumerator ProcessNextTexture(TextureInfo info)
	{
		string url = GetUrl(info.textureName);
		Evict();
		WWW textureLoader = null;
		for (int i = 0; i < 5; i++)
		{
			textureLoader = new WWW(url);
			yield return textureLoader;
			if (string.IsNullOrEmpty(textureLoader.error) || url.StartsWith("file://"))
			{
				break;
			}
			EB.Debug.LogWarning("COULD NOT FIND TEXTURE: " + url);
			yield return new WaitForSeconds(0.5f * (float)i);
		}
		if (Application.isEditor)
		{
			yield return new WaitForSeconds(0.15f);
		}
		if (string.IsNullOrEmpty(textureLoader.error))
		{
			info.texture = textureLoader.textureNonReadable;
			info.texture.wrapMode = TextureWrapMode.Clamp;
			info.texture.name = info.textureName;
			info.DoCallbacks();
		}
		textureLoader.Dispose();
		_concurrentRequests--;
	}

	public void ClearPool(bool forceGarbageCollect = false)
	{
		FacebookPortraitManager.FlushAll();
		foreach (TextureInfo item in _all)
		{
			Object.Destroy(item.texture);
		}
		_all.Clear();
		_evictions = 0;
		if (forceGarbageCollect && Assets.RequiresUnloadAssets())
		{
			Assets.UnloadUnusedAssets();
		}
	}

	public void ReleaseTexture(string textureName)
	{
		if (!string.IsNullOrEmpty(textureName))
		{
			TextureInfo textureInfo = GetTextureInfo(textureName);
			if (textureInfo != null)
			{
				textureInfo.refCount--;
			}
		}
	}

	public void LoadTexture(string textureName, MonoBehaviour obj, Action<Texture2D> cb)
	{
		LoadTexture(textureName, SafeAction.Wrap(obj, cb));
	}

	public void LoadTexture(string textureName, Action<Texture2D> cb)
	{
		if (string.IsNullOrEmpty(textureName))
		{
			UnityEngine.Debug.LogWarning("Attempt to load empty texture.");
			cb(null);
			return;
		}
		TextureInfo textureInfo = GetTextureInfo(textureName);
		if (textureInfo != null)
		{
			textureInfo.refCount++;
			textureInfo.time = UnityEngine.Time.realtimeSinceStartup;
			if (textureInfo.texture != null)
			{
				if (cb != null)
				{
					cb(textureInfo.texture);
				}
			}
			else
			{
				textureInfo.callbacks.Add(cb);
				_loads.Add(textureName);
			}
		}
		else
		{
			textureInfo = new TextureInfo();
			textureInfo.textureName = textureName;
			textureInfo.refCount = 1;
			textureInfo.time = UnityEngine.Time.realtimeSinceStartup;
			textureInfo.callbacks.Add(cb);
			_all.Add(textureInfo);
			_loads.Add(textureName);
		}
	}
}
