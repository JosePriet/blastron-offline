using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using EB.TestFlight;
using UnityEngine;

namespace EB
{
	public static class Assets
	{
		private class AssetPack
		{
			public string id;

			public string folder;

			public string version;

			public List<AssetBundleInfo> bundles;

			public Hashtable folders;
		}

		private class AssetBundleInfo
		{
			public AssetPack pack;

			public AssetBundle bundle;

			public string id;

			public int retry;

			public string[] paths;

			public string parent;

			public long size;

			public int asyncLoadingCount;

			public bool unload;

			public int hash;

			public bool isLoaded
			{
				get
				{
					return bundle != null;
				}
			}
		}

		private class AssetSceneInfo
		{
			public AssetPack pack;

			public string id;
		}

		private const int kMaxWWW = 2;

		private const bool DefaultSurrogate = false;

		private static Dictionary<string, AssetBundleInfo> _bundles = new Dictionary<string, AssetBundleInfo>();

		private static Dictionary<string, AssetBundleInfo> _pathToBundle = new Dictionary<string, AssetBundleInfo>();

		private static Dictionary<string, WWW> _www = new Dictionary<string, WWW>();

		private static Dictionary<string, AssetPack> _packs = new Dictionary<string, AssetPack>();

		private static Dictionary<string, AssetSceneInfo> _scenes = new Dictionary<string, AssetSceneInfo>();

		private static List<AssetBundleInfo> _unloads = new List<AssetBundleInfo>();

		private static AsyncOperation _cleanupTask = null;

		private static bool _uncompressed = false;

		private static GameObject _missingObject;

		private static Texture2D _missingTexture;

		private static List<string> _dlcPacks = new List<string>();

		private static bool _fatalLoadError = false;

		public static string ContentVersion
		{
			get
			{
				string text = string.Empty;
				List<string> list = new List<string>();
				foreach (string key in _packs.Keys)
				{
					list.Add(key);
				}
				list.Sort();
				foreach (string item in list)
				{
					AssetPack assetPack = _packs[item];
					text += string.Format("/{0}:{1}", assetPack.id, assetPack.version);
				}
				return text;
			}
		}

		public static string[] Scenes
		{
			get
			{
				string[] array = new string[_scenes.Count];
				_scenes.Keys.CopyTo(array, 0);
				Array.Sort(array);
				return array;
			}
		}

		public static string CurrentScene { get; private set; }

		public static bool IsLoadingScene { get; private set; }

		private static Texture MissingTexture
		{
			get
			{
				if (_missingTexture == null)
				{
					_missingTexture = new Texture2D(2, 2);
					_missingTexture.wrapMode = TextureWrapMode.Repeat;
					_missingTexture.filterMode = FilterMode.Point;
					_missingTexture.SetPixel(0, 0, Color.green);
					_missingTexture.SetPixel(1, 1, Color.green);
					_missingTexture.SetPixel(1, 0, Color.magenta);
					_missingTexture.SetPixel(0, 1, Color.magenta);
					_missingTexture.Apply();
				}
				return _missingTexture;
			}
		}

		private static GameObject MissingObject
		{
			get
			{
				if (_missingObject == null)
				{
					_missingObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
					_missingObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
					_missingObject.renderer.material.shader = Shader.Find("Diffuse");
					_missingObject.renderer.material.mainTexture = MissingTexture;
					UnityEngine.Object.Destroy(_missingObject.GetComponent<BoxCollider>());
				}
				return _missingObject;
			}
		}

		public static string FindDiskAsset(string path)
		{
			int num = path.IndexOfAny(new char[2] { '/', '\\' });
			string name = path;
			if (num >= 0)
			{
				name = path.Substring(0, num);
			}
			foreach (KeyValuePair<string, AssetPack> pack in _packs)
			{
				ArrayList arrayList = Dot.Array(name, pack.Value.folders, null);
				if (arrayList == null)
				{
					continue;
				}
				foreach (string item in arrayList)
				{
					if (item.StartsWith(path))
					{
						return Loader.GetBaseURL(pack.Key) + item;
					}
				}
			}
			Debug.LogWarning("Failed to find asset for path " + path);
			return string.Empty;
		}

		public static string AssetName(string path)
		{
			path = path.Replace('\\', '/').ToLower();
			return Hash.StringHash(path).ToString();
		}

		public static void AddDlcPacks(string[] packs)
		{
			foreach (string item in packs)
			{
				_dlcPacks.Add(item);
			}
		}

		public static bool HasDLCPack(string packName)
		{
			return _dlcPacks.Contains(packName);
		}

		public static bool HasPack(string packName)
		{
			return _packs.ContainsKey(packName);
		}

		public static Coroutine LoadPacks(bool mount)
		{
			return StartCoroutine(DoLoadPacks(mount));
		}

		private static IEnumerator DoLoadPacks(bool mount)
		{
			string dataPath = Loader.DataPath;
			string text = string.Empty;
			while (string.IsNullOrEmpty(text))
			{
				string tocPath = dataPath + "packs.txt";
				WWW www = new WWW(tocPath);
				yield return www;
				if (www.error == null)
				{
					text = www.text;
				}
				else
				{
					Debug.LogError("Failed to load pack at path " + tocPath + " DID YOU BUNDLE?");
				}
			}
			ArrayList packs = (ArrayList)JSON.Parse(text);
			if (packs == null)
			{
				Debug.LogError("FATAL: Failed to decode packs toc!: " + text);
				yield break;
			}
			foreach (string pack in _dlcPacks)
			{
				yield return LoadPack(pack.ToString(), mount);
			}
			foreach (object pack2 in packs)
			{
				yield return LoadPack(pack2.ToString(), mount);
			}
		}

		public static Coroutine LoadPack(string name, bool mount)
		{
			return StartCoroutine(DoLoadPack(name, mount));
		}

		private static IEnumerator DoLoadPack(string name, bool mount)
		{
			AssetPack pack = null;
			if (_packs.TryGetValue(name, out pack))
			{
				Debug.Log("pack already loaded skipping " + name);
				if (mount)
				{
					yield return StartCoroutine(DoLoadBundles(pack.bundles));
				}
				yield break;
			}
			pack = new AssetPack
			{
				id = name,
				folder = Loader.GetBaseURL(name)
			};
			string text = string.Empty;
			while (string.IsNullOrEmpty(text))
			{
				string tocPath = pack.folder + "toc.txt";
				WWW www = new WWW(tocPath);
				yield return www;
				if (www.error == null)
				{
					text = www.text;
				}
				else
				{
					Debug.LogError("Failed to load toc at path " + tocPath);
				}
			}
			_packs[name] = pack;
			Hashtable toc = (Hashtable)JSON.Parse(text);
			if (toc == null)
			{
				Debug.LogError("FATAL: Failed to decode asset toc!: " + text);
				yield break;
			}
			Hashtable scenes = Dot.Object("scenes", toc, new Hashtable());
			foreach (DictionaryEntry entry in scenes)
			{
				string id = entry.Key.ToString();
				ArrayList list = (ArrayList)entry.Value;
				foreach (string scene in list)
				{
					AssetSceneInfo sceneInfo = new AssetSceneInfo
					{
						pack = pack,
						id = id
					};
					_scenes.Add(scene, sceneInfo);
				}
			}
			pack.folders = Dot.Object("folders", toc, new Hashtable());
			Hashtable bundles = Dot.Object("bundles", toc, new Hashtable());
			pack.version = Dot.String("version", toc, string.Empty);
			pack.bundles = new List<AssetBundleInfo>();
			_uncompressed = Dot.Bool("uncompressed", toc, false);
			ArrayList defaultPaths = new ArrayList();
			foreach (DictionaryEntry entry2 in bundles)
			{
				Hashtable bundle = (Hashtable)entry2.Value;
				AssetBundleInfo info = new AssetBundleInfo
				{
					pack = pack,
					id = entry2.Key.ToString(),
					parent = Dot.String("parent", bundle, string.Empty),
					paths = (string[])Dot.Array("paths", bundle, defaultPaths).ToArray(typeof(string)),
					size = Dot.Long("size", bundle, 0L),
					hash = Dot.Integer("hash", bundle, 0)
				};
				_bundles.Add(info.id, info);
				string[] paths = info.paths;
				foreach (string path in paths)
				{
					if (_pathToBundle.ContainsKey(path))
					{
						Debug.LogError(string.Format("Path collision: {0} is in {1} and in {2} ", path, info.id, _pathToBundle[path].id));
					}
					_pathToBundle[path] = info;
				}
				pack.bundles.Add(info);
			}
			if (mount)
			{
				yield return StartCoroutine(DoLoadBundles(pack.bundles));
			}
			Debug.Log("Loaded " + _bundles.Count + " bundles");
			Debug.Log("Loaded " + _pathToBundle.Count + " assets");
			Debug.Log(name + " Version: " + pack.version);
		}

		public static AsyncOperation ProcessDeferedUnloads(bool runCleanup = true)
		{
			if (_unloads.Count == 0)
			{
				return _cleanupTask;
			}
			bool flag = false;
			int num = 0;
			while (num < _unloads.Count)
			{
				AssetBundleInfo assetBundleInfo = _unloads[num];
				if (!assetBundleInfo.isLoaded || !assetBundleInfo.unload)
				{
					_unloads.RemoveAt(num);
				}
				else if (assetBundleInfo.asyncLoadingCount == 0)
				{
					Debug.Log("***** UNLOADING BUNDLE: " + assetBundleInfo.id);
					assetBundleInfo.bundle.Unload(false);
					UnityEngine.Object.DestroyImmediate(assetBundleInfo.bundle, true);
					assetBundleInfo.bundle = null;
					_unloads.RemoveAt(num);
					flag = true;
				}
				else
				{
					Debug.LogWarning("Waiting for bundle to stop loading: " + assetBundleInfo.id + " count:" + assetBundleInfo.asyncLoadingCount);
					num++;
				}
			}
			if (flag && runCleanup)
			{
				UnloadUnusedAssets();
			}
			return _cleanupTask;
		}

		public static AsyncOperation FlushAllAndUnload()
		{
			CoreAssets.Instance.Loaded.Clear();
			return UnloadUnusedAssets();
		}

		public static AsyncOperation UnloadUnusedAssets()
		{
			if (_cleanupTask == null || _cleanupTask.isDone)
			{
				Debug.Log("Running cleanup task");
				_cleanupTask = Resources.UnloadUnusedAssets();
			}
			return _cleanupTask;
		}

		private static AssetBundleInfo GetBundleInfo(string id)
		{
			AssetBundleInfo value;
			if (_bundles.TryGetValue(id.ToLower(), out value))
			{
				return value;
			}
			return null;
		}

		private static Coroutine StartCoroutine(IEnumerator co)
		{
			return Coroutines.Run(co);
		}

		private static void GatherBundleLoadList(AssetBundleInfo info, List<AssetBundleInfo> loadList)
		{
			if (!info.isLoaded)
			{
				if (!string.IsNullOrEmpty(info.parent))
				{
					AssetBundleInfo bundleInfo = GetBundleInfo(info.parent);
					GatherBundleLoadList(bundleInfo, loadList);
				}
				if (!loadList.Contains(info))
				{
					Debug.Log("Need to load: " + info.id);
					loadList.Add(info);
				}
			}
			else if (info.unload)
			{
				Debug.LogError("********************** WARNING: skipping unload of " + info.id + " because it was requested to load again");
				info.unload = false;
			}
		}

		private static void PrintBundleLoadList(List<AssetBundleInfo> loadList)
		{
			Debug.Log("Dumping bundle load list...");
			foreach (AssetBundleInfo load in loadList)
			{
				Debug.Log("bundleInfo : {0}, {1}", load.id, load.isLoaded);
			}
		}

		private static void PrintBundleList()
		{
			Debug.LogError("DUMPING BUNDLE LIST...{0}", _bundles.Count);
			foreach (KeyValuePair<string, AssetBundleInfo> bundle in _bundles)
			{
				Debug.Log("bundle : {0}", bundle.Key);
			}
		}

		private static IEnumerator DoLoadBundle(string id)
		{
			AssetBundleInfo info = GetBundleInfo(id);
			if (info == null)
			{
				Debug.LogError("ERROR: Bundle " + id + " does not exist!");
				yield break;
			}
			info.unload = false;
			if (!info.isLoaded)
			{
				List<AssetBundleInfo> loadList = new List<AssetBundleInfo>();
				GatherBundleLoadList(info, loadList);
				yield return StartCoroutine(DoLoadBundles(loadList));
			}
		}

		private static WWW GetWWW(AssetBundleInfo info)
		{
			WWW value = null;
			if (_www.TryGetValue(info.id, out value) && value.isDone && value.error != null)
			{
				Debug.LogError("WWW error: " + value.error);
				info.retry++;
				value.Dispose();
				value = null;
				if (info.retry == 5)
				{
					FatalLoadError(info);
				}
			}
			if (value == null)
			{
				if (_www.Count >= 2)
				{
					return null;
				}
				value = Loader.LoadBundle(info.pack.id, info.id);
				_www[info.id] = value;
			}
			return value;
		}

		private static void FatalLoadError(AssetBundleInfo info)
		{
			Debug.LogError("Something went VERY WRONG loading bundle: " + info.id);
			_fatalLoadError = true;
		}

		private static IEnumerator DoLoadBundles(List<AssetBundleInfo> loadList)
		{
			bool done = false;
			do
			{
				done = true;
				foreach (AssetBundleInfo info in loadList)
				{
					info.unload = false;
					if (info.isLoaded)
					{
						continue;
					}
					done = false;
					if (!string.IsNullOrEmpty(info.parent) && !IsBundleLoaded(info.parent))
					{
						continue;
					}
					if (_uncompressed)
					{
						string path = Loader.GetBaseURL(info.pack.id) + Loader.GetBundlePath(info.id);
						if (path.StartsWith("file://"))
						{
							path = path.Replace("file://", string.Empty);
							Debug.Log("Loading uncompressed bundle at: " + path);
							info.bundle = AssetBundle.CreateFromFile(path);
							if (!info.isLoaded)
							{
								FatalLoadError(info);
								break;
							}
							info.bundle.name = info.id;
							UnityEngine.Object.DontDestroyOnLoad(info.bundle);
							continue;
						}
						if (path.StartsWith("jar:"))
						{
							Debug.Log("Trying to load from Jar: " + path);
							path = path.Replace("jar:file://", string.Empty);
							string[] parts = path.Split('!');
							if (parts.Length == 2)
							{
								string jarPath = parts[0];
								string filePath = parts[1];
								if (filePath.StartsWith("/"))
								{
									filePath = filePath.Substring(1);
								}
								string outputPath = Path.Combine(Application.persistentDataPath, Path.GetFileName(filePath));
								bool success = false;
								yield return StartCoroutine(JarExtractor.SyncLoadFromJar(jarPath, filePath, info.size, outputPath, info.hash, (bool s) =>
								{
									success = s;
								}));
								if (success)
								{
									info.bundle = AssetBundle.CreateFromFile(outputPath);
								}
								if (!info.isLoaded)
								{
									FatalLoadError(info);
									break;
								}
								Debug.Log("Successfully Loaded from " + outputPath);
								info.bundle.name = info.id;
								UnityEngine.Object.DontDestroyOnLoad(info.bundle);
								continue;
							}
						}
					}
					WWW www = GetWWW(info);
					if (www != null && www.isDone)
					{
						info.bundle = www.assetBundle;
						www.Dispose();
						_www.Remove(info.id);
						if (!info.isLoaded)
						{
							FatalLoadError(info);
							break;
						}
						info.bundle.name = info.id;
						UnityEngine.Object.DontDestroyOnLoad(info.bundle);
						Debug.Log("***** BUNDLE LOADED : {0}", info.id);
					}
				}
				yield return new WaitForFixedUpdate();
			}
			while (!done && !_fatalLoadError);
			while (_fatalLoadError)
			{
				yield return new WaitForFixedUpdate();
			}
		}

		public static Coroutine WaitForIdle()
		{
			return StartCoroutine(DoWaitForIdle());
		}

		private static IEnumerator DoWaitForIdle()
		{
			while (_www.Count > 0)
			{
				yield return new WaitForFixedUpdate();
			}
		}

		public static bool IsBundleLoaded(string id)
		{
			AssetBundleInfo bundleInfo = GetBundleInfo(id);
			return bundleInfo != null && bundleInfo.isLoaded;
		}

		public static Coroutine LoadBundle(string id)
		{
			return StartCoroutine(DoLoadBundle(id));
		}

		public static void UnloadBundle(string id)
		{
			AssetBundleInfo bundleInfo = GetBundleInfo(id);
			UnloadBundle(bundleInfo);
		}

		private static void UnloadBundle(AssetBundleInfo info)
		{
			if (info != null && info.isLoaded)
			{
				info.unload = true;
				_unloads.Remove(info);
				_unloads.Add(info);
			}
		}

		public static Coroutine LoadBundleFromResourcePath(string path)
		{
			AssetBundleInfo value = null;
			string key = path.ToLower();
			if (_pathToBundle.TryGetValue(key, out value))
			{
				return LoadBundle(value.id);
			}
			return null;
		}

		public static void UnloadBundleFromResourcePath(string path)
		{
			AssetBundleInfo value = null;
			string key = path.ToLower();
			if (_pathToBundle.TryGetValue(key, out value))
			{
				UnloadBundle(value);
			}
		}

		public static Coroutine ForceLoadLevel(string sceneName)
		{
			return StartCoroutine(_LoadScene(sceneName, false, true));
		}

		public static Coroutine LoadLevel(string sceneName)
		{
			return StartCoroutine(_LoadScene(sceneName, false, false));
		}

		public static Coroutine ForceLoadLevelAdditive(string sceneName)
		{
			return StartCoroutine(_LoadScene(sceneName, true, true));
		}

		public static Coroutine LoadLevelAdditive(string sceneName)
		{
			return StartCoroutine(_LoadScene(sceneName, true, false));
		}

		private static IEnumerator _LoadScene(string sceneName, bool additive, bool forceLoad)
		{
			Plugin.Checkpoint("Loading scene : " + sceneName);
			while (IsLoadingScene)
			{
				yield return 1;
			}
			if (CurrentScene == sceneName && !forceLoad)
			{
				yield break;
			}
			IsLoadingScene = true;
			AssetBundle bundle = null;
			AssetSceneInfo sceneInfo = null;
			if (_scenes.TryGetValue(sceneName, out sceneInfo))
			{
				int retry = 0;
				while (bundle == null)
				{
					WWW www = Loader.LoadScene(sceneInfo.pack.id, sceneInfo.id);
					yield return www;
					if (!string.IsNullOrEmpty(www.error))
					{
						Debug.LogError("Failed to load bundle " + sceneName + " " + www.error);
						if (retry++ == 5)
						{
							Debug.LogError("FATAL: can't load bundle for scene " + sceneName);
							_fatalLoadError = true;
							while (_fatalLoadError)
							{
								yield return new WaitForFixedUpdate();
							}
						}
					}
					else
					{
						bundle = www.assetBundle;
					}
				}
			}
			if (additive)
			{
				AsyncOperation async = Application.LoadLevelAdditiveAsync(sceneName);
				while (!async.isDone)
				{
					yield return async;
				}
			}
			else
			{
				AsyncOperation async2 = Application.LoadLevelAsync(sceneName);
				while (!async2.isDone)
				{
					yield return async2;
				}
			}
			CurrentScene = sceneName;
			if (bundle != null)
			{
				bundle.Unload(false);
				UnityEngine.Object.DestroyImmediate(bundle, true);
				FixupEditorMaterials();
			}
			if (RequiresUnloadAssets())
			{
				yield return UnloadUnusedAssets();
			}
			IsLoadingScene = false;
		}

		public static void FixupEditorMaterials(GameObject obj)
		{
		}

		public static void FixupEditorMaterials()
		{
		}

		public static bool RequiresUnloadAssets()
		{
			return true;
		}

		public static List<string> GetLoadedAssets()
		{
			List<string> list = new List<string>();
			for (int i = 0; i < CoreAssets.Instance.Loaded.Count; i++)
			{
				list.Add(CoreAssets.Instance.Loaded[i].path);
			}
			return list;
		}

		public static bool Unload(string path)
		{
			return CoreAssets.Instance.Unload(path.ToLower());
		}

		public static bool Unload(UnityEngine.Object asset)
		{
			return CoreAssets.Instance.Unload(asset);
		}

		private static void Track(string path, UnityEngine.Object obj, AssetSource src)
		{
			if (obj != null)
			{
				CoreAssets.Instance.Add(path, obj, src);
			}
		}

		private static UnityEngine.Object Find(string path)
		{
			return CoreAssets.Instance.Find(path);
		}

		public static UnityEngine.Object ReturnType(UnityEngine.Object asset, Type type)
		{
			if (asset == null)
			{
				return null;
			}
			if (type.IsInstanceOfType(asset))
			{
				return asset;
			}
			if (asset is GameObject)
			{
				GameObject gameObject = (GameObject)asset;
				if (type.IsSubclassOf(typeof(Component)))
				{
					return gameObject.GetComponent(type);
				}
			}
			return null;
		}

		public static T Load<T>(string path) where T : UnityEngine.Object
		{
			return Load(path, typeof(T), false) as T;
		}

		public static T Load<T>(string path, bool bReturnSurrogate) where T : UnityEngine.Object
		{
			return Load(path, typeof(T), bReturnSurrogate) as T;
		}

		public static UnityEngine.Object Load(string path)
		{
			return Load<UnityEngine.Object>(path, false);
		}

		public static UnityEngine.Object Load(string path, bool bReturnSurrogate)
		{
			return Load<UnityEngine.Object>(path, bReturnSurrogate);
		}

		public static UnityEngine.Object Load(string path, Type type)
		{
			return Load(path, type, false);
		}

		public static UnityEngine.Object Load(string path, Type type, bool bReturnSurrogate)
		{
			string text = path.ToLower();
			UnityEngine.Object obj = Find(text);
			if (obj != null)
			{
				return ReturnType(obj, type);
			}
			AssetBundleInfo value = null;
			if (_pathToBundle.TryGetValue(text, out value))
			{
				if (!value.isLoaded)
				{
					Debug.LogError("ERROR: object at path " + path + " exists in bundle " + value.id + " but the bundle isn't loaded");
					return null;
				}
				string name = AssetName(text);
				UnityEngine.Object obj2 = value.bundle.Load(name, type);
				if (obj2 != null)
				{
					Track(text, obj2, AssetSource.Bundle);
					return ReturnType(obj2, type);
				}
				Debug.LogError("Failed to load " + text + " from bundle " + value.id);
				return null;
			}
			obj = Resources.Load(path, type);
			if (obj == null)
			{
				if (bReturnSurrogate)
				{
					obj = ReturnType(MissingObject, type);
				}
				if (!(obj == null))
				{
				}
			}
			else
			{
				Track(text, obj, AssetSource.Resources);
			}
			return obj;
		}

		public static UnityEngine.Object[] LoadAllBundle(string id)
		{
			AssetBundleInfo bundleInfo = GetBundleInfo(id);
			if (bundleInfo != null && bundleInfo.isLoaded)
			{
				return bundleInfo.bundle.LoadAll();
			}
			Debug.LogError("Failed to load all bundle : " + bundleInfo);
			return new UnityEngine.Object[0];
		}

		public static Coroutine LoadBundleAssetsAndUnload(string id)
		{
			return StartCoroutine(DoLoadBundleAssetsAndUnload(id));
		}

		private static IEnumerator DoLoadBundleAssetsAndUnload(string id)
		{
			yield return LoadBundle(id);
			foreach (KeyValuePair<string, AssetBundleInfo> kvp in _pathToBundle)
			{
				if (kvp.Value.id == id)
				{
					Load(kvp.Key);
				}
			}
			UnloadBundle(id);
			yield return ProcessDeferedUnloads();
		}

		public static UnityEngine.Object[] LoadAll(string path, Type type)
		{
			List<UnityEngine.Object> list = new List<UnityEngine.Object>();
			UnityEngine.Object[] array = Resources.LoadAll(path, type);
			foreach (UnityEngine.Object obj in array)
			{
				string path2 = (path + "/" + obj.name).ToLower();
				UnityEngine.Object obj2 = Find(path2);
				if (obj2 != null)
				{
					list.Add(obj2);
					continue;
				}
				Track(path2, obj, AssetSource.Resources);
				list.Add(obj);
			}
			string value = path.ToLower();
			foreach (KeyValuePair<string, AssetBundleInfo> item in _pathToBundle)
			{
				if (item.Value.isLoaded && item.Key.StartsWith(value))
				{
					UnityEngine.Object obj3 = Load(item.Key, type, false);
					if (obj3 != null)
					{
						list.Add(obj3);
					}
				}
			}
			return list.ToArray();
		}

		public static Coroutine LoadMultiAsync(string[] paths, Type type, Action<UnityEngine.Object[]> callback)
		{
			return StartCoroutine(DoLoadMultiAsync(paths, type, callback));
		}

		private static IEnumerator DoLoadMultiAsync(string[] paths, Type type, Action<UnityEngine.Object[]> callback)
		{
			List<AssetBundleInfo> loadList = new List<AssetBundleInfo>();
			foreach (string path in paths)
			{
				string path_lower = path.ToLower();
				AssetBundleInfo info = null;
				if (_pathToBundle.TryGetValue(path_lower, out info))
				{
					GatherBundleLoadList(info, loadList);
				}
			}
			if (loadList.Count > 0)
			{
				yield return StartCoroutine(DoLoadBundles(loadList));
			}
			UnityEngine.Object[] objectList = new UnityEngine.Object[paths.Length];
			for (int j = 0; j < paths.Length; j++)
			{
				yield return LoadAsync(paths[j], type, (UnityEngine.Object asset) =>
				{
					objectList[j] = asset;
				});
			}
			if (callback != null)
			{
				callback(objectList);
			}
		}

		public static Coroutine LoadAsync(string path, Type type, Action<UnityEngine.Object> callback)
		{
			return StartCoroutine(DoLoadAsync(path, type, callback));
		}

		private static IEnumerator DoLoadAsync(string path, Type type, Action<UnityEngine.Object> callback)
		{
			UnityEngine.Object asset = null;
			AssetBundleInfo info = null;
			string path_lower = path.ToLower();
			asset = Find(path_lower);
			if (asset != null)
			{
				asset = ReturnType(asset, type);
			}
			else if (_pathToBundle.TryGetValue(path_lower, out info))
			{
				info.asyncLoadingCount++;
				info.unload = false;
				if (!info.isLoaded)
				{
					Debug.Log(path_lower + " requires bundle " + info.id + " to be loaded");
					yield return LoadBundle(info.id);
				}
				string assetName = AssetName(path_lower);
				AssetBundleRequest async = info.bundle.LoadAsync(assetName, type);
				yield return async;
				if (info.isLoaded)
				{
					asset = async.asset;
					if (asset == null)
					{
						Debug.LogError("Failed to load " + path_lower + " from bundle " + info.id);
					}
					else
					{
						Track(path_lower, asset, AssetSource.Bundle);
					}
				}
				info.asyncLoadingCount--;
			}
			else
			{
				asset = Resources.Load(path, type);
				if (asset != null)
				{
					Track(path_lower, asset, AssetSource.Resources);
				}
			}
			if (callback != null)
			{
				callback(asset);
			}
		}

		public static bool Exists(string path)
		{
			string key = path.ToLower();
			AssetBundleInfo value = null;
			if (_pathToBundle.TryGetValue(key, out value))
			{
				return true;
			}
			return Resources.Load(path) != null;
		}

		public static void PrintLoadedBundles()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Bundles Loaded:");
			List<string> list = new List<string>();
			foreach (AssetBundleInfo value in _bundles.Values)
			{
				if (value.isLoaded)
				{
					list.Add(value.id);
				}
			}
			list.Sort();
			foreach (string item in list)
			{
				stringBuilder.AppendFormat("\tbundle: {0}\n", item);
			}
			Debug.Log(stringBuilder.ToString());
		}
	}
}
