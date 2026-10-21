using System;
using System.Collections;
using System.IO;
using Ionic.Zlib;
using UnityEngine;
using tar_cs;

namespace EB.Sparx
{
	public class ObbManager
	{
		private static ThreadPool _pool = new ThreadPool(1);

		public string Error { get; private set; }

		public Action<string> ProgessCallback { get; set; }

		public bool IsMounted { get; private set; }

		private string BasePath
		{
			get
			{
				return Path.Combine(Application.persistentDataPath, "Content");
			}
		}

		public Coroutine Check()
		{
			// Content packs ship with the game; there is no OBB to fetch.
			IsMounted = true;
			return null;
		}

		private IEnumerator DoCheck()
		{
			Error = string.Empty;
			IsMounted = false;
			Notify(Localizer.GetString("ID_SPARX_CONTENT_CHECKING"));
			yield return 1;
			if (!GooglePlayDownloader.RunningOnAndroid())
			{
				yield break;
			}
			string expPath = GooglePlayDownloader.GetExpansionFilePath();
			if (string.IsNullOrEmpty(expPath))
			{
				Error = "ID_SPARX_OBB_ERROR";
				yield break;
			}
			UnityEngine.Debug.Log("Ext Folder " + expPath);
			string extractFolder = Path.Combine(BasePath, GooglePlayDownloader.ObbVersion.ToString());
			if (CheckContent(extractFolder))
			{
				Mount(extractFolder);
				yield break;
			}
			string mainPath = GooglePlayDownloader.GetMainOBBPath(expPath);
			if (string.IsNullOrEmpty(mainPath))
			{
				UnityEngine.Debug.Log("Fetching main OBB ");
				GooglePlayDownloader.FetchOBB();
				yield return new WaitForSeconds(3f);
			}
			mainPath = GooglePlayDownloader.GetMainOBBPath(expPath);
			if (string.IsNullOrEmpty(mainPath))
			{
				UnityEngine.Debug.Log("Failed to download OBB");
				Error = "ID_SPARX_CONTENT_FAILED";
				yield break;
			}
			UnityEngine.Debug.Log("Main Path: " + mainPath);
			Notify(Localizer.GetString("ID_SPARX_CONTENT_EXTRACTING"));
			yield return new WaitForFixedUpdate();
			yield return Coroutines.Run(Extract(extractFolder, mainPath));
			Mount(extractFolder);
		}

		private void Notify(string txt)
		{
			if (ProgessCallback != null)
			{
				ProgessCallback(txt);
			}
		}

		private object ReadJsonFile(string path)
		{
			try
			{
				byte[] bytes = File.ReadAllBytes(path);
				return JSON.Parse(Encoding.GetString(bytes));
			}
			catch
			{
				UnityEngine.Debug.Log("failed to load json file at path: " + path);
			}
			return null;
		}

		private void DeleteFolder(string path)
		{
			try
			{
				Directory.Delete(path, true);
			}
			catch
			{
			}
		}

		private bool CheckContent(string path)
		{
			UnityEngine.Debug.Log("Checking content " + path);
			object obj = ReadJsonFile(Path.Combine(path, "files.json"));
			if (obj == null)
			{
				Debug.Log("Missing json file, path is no good");
				return false;
			}
			Hashtable hashtable = Dot.Object("files", obj, null);
			if (hashtable == null)
			{
				Debug.Log("Missing files entry, path is no good");
				return false;
			}
			foreach (DictionaryEntry item in hashtable)
			{
				string text = item.Key.ToString();
				int num = int.Parse(item.Value.ToString());
				try
				{
					FileInfo fileInfo = new FileInfo(Path.Combine(path, text));
					if (fileInfo.Length != num)
					{
						UnityEngine.Debug.Log("File size is bad " + text);
						return false;
					}
				}
				catch
				{
				}
			}
			return true;
		}

		private bool Mount(string directory)
		{
			object obj = ReadJsonFile(Path.Combine(directory, "files.json"));
			if (obj == null)
			{
				Debug.Log("Missing json file, path is no good");
				return false;
			}
			ArrayList arrayList = Dot.Array("packs", obj, null);
			if (arrayList == null)
			{
				UnityEngine.Debug.Log("Missing packs entry, path is no good");
				return false;
			}
			foreach (string item in arrayList)
			{
				Loader.OverridePath(item, "file://" + Path.Combine(directory, item + Path.DirectorySeparatorChar));
			}
			Assets.AddDlcPacks((string[])arrayList.ToArray(typeof(string)));
			IsMounted = true;
			return true;
		}

		private IEnumerator Extract(string directory, string localPath)
		{
			Debug.Log("extracting " + localPath + " to " + directory);
			bool result = true;
			string directory2 = default(string);
			string localPath2 = default(string);
			ThreadPool.AsyncTask async = _pool.Queue((object obj) =>
			{
				try
				{
					DeleteFolder(directory2);
					ArrayList arrayList = new ArrayList();
					FileStream stream = new FileStream(localPath2, FileMode.Open, FileAccess.Read);
					GZipStream tarredData = new GZipStream(stream, CompressionMode.Decompress);
					TarReader tarReader = new TarReader(tarredData);
					while (tarReader.MoveNext(false))
					{
						ITarHeader fileInfo = tarReader.FileInfo;
						string text = Path.Combine(directory2, fileInfo.FileName);
						string fileName = Path.GetFileName(text);
						string path = text.Remove(text.Length - fileName.Length);
						Directory.CreateDirectory(path);
						Debug.Log("extracting " + fileName);
						using (FileStream dataDestanation = new FileStream(text, FileMode.Create, FileAccess.Write))
						{
							tarReader.Read(dataDestanation);
						}
						arrayList.Add(new object[2] { fileInfo.FileName, fileInfo.SizeInBytes });
					}
				}
				catch (Exception ex)
				{
					UnityEngine.Debug.LogError("extraction failed " + ex);
					result = false;
				}
			}, null);
			while (!async.done)
			{
				yield return 1;
			}
			if (!result)
			{
				UnityEngine.Debug.Log("Failed to extract");
				Error = "ID_SPARX_CONTENT_FAILED_EXTRACT";
				DeleteFolder(directory);
			}
		}
	}
}
