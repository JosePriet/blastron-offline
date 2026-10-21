using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using EB.Net;
using Ionic.Zlib;
using UnityEngine;
using tar_cs;

namespace EB.Sparx
{
	public class ContentManager : IDisposable
	{
		private class ContentPack
		{
			public string folder;

			public List<string> packs = new List<string>();

			public int version;

			public void Delete()
			{
				Debug.Log("Deleteing pack " + folder);
				try
				{
					Directory.Delete(folder, true);
				}
				catch
				{
				}
			}
		}

		private string _baseUrl;

		private HttpEndPoint _endpoint;

		private List<ContentPack> _packs = new List<ContentPack>();

		private static ContentPack _mounted;

		public bool DownloadOverCarrierDataNetwork = true;

		public Action<string> ProgessCallback;

		private string BasePath
		{
			get
			{
				return Path.Combine(Application.temporaryCachePath, "Content");
			}
		}

		public ContentManager(string baseUrl)
		{
			_baseUrl = baseUrl;
		}

		public Coroutine Check()
		{
			return Coroutines.Run(DoCheck());
		}

		private ContentPack GetPack(int version)
		{
			foreach (ContentPack pack in _packs)
			{
				if (pack.version == version)
				{
					return pack;
				}
			}
			return null;
		}

		private void Mount(ContentPack pack)
		{
			if (_mounted != null)
			{
				return;
			}
			_mounted = pack;
			foreach (string pack2 in pack.packs)
			{
				Loader.OverridePath(pack2, "file://" + Path.Combine(pack.folder, pack2 + Path.DirectorySeparatorChar));
			}
			Assets.AddDlcPacks(pack.packs.ToArray());
			foreach (ContentPack pack3 in _packs)
			{
				if (pack3.folder != pack.folder)
				{
					pack3.Delete();
				}
			}
			_packs.Clear();
		}

		private void Notify(string txt)
		{
			if (ProgessCallback != null)
			{
				ProgessCallback(txt);
			}
		}

		private bool CheckFile(string localpath, long size)
		{
			try
			{
				FileInfo fileInfo = new FileInfo(localpath);
				if (fileInfo != null)
				{
					return fileInfo.Length == size;
				}
			}
			catch
			{
			}
			return false;
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
				Debug.Log("failed to load json file at path: " + path);
			}
			return null;
		}

		private bool WriteJsonFile(string path, object data)
		{
			try
			{
				string str = JSON.Stringify(data);
				File.WriteAllBytes(path, Encoding.GetBytes(str));
				return true;
			}
			catch
			{
				Debug.Log("failed to write json file at path: " + path);
				return false;
			}
		}

		private IEnumerator DoCheck()
		{
			Notify(Localizer.GetString("ID_SPARX_CONTENT_CHECKING"));
			yield return Coroutines.Run(DoEnumerate());
			yield return Coroutines.Run(DoCheckServer());
			if (_mounted == null && _packs.Count > 0)
			{
				Mount(_packs[0]);
			}
			Notify(string.Empty);
		}

		private IEnumerator DoEnumerate()
		{
			_packs.Clear();
			string[] directories;
			try
			{
				directories = Directory.GetDirectories(BasePath, "*", SearchOption.TopDirectoryOnly);
			}
			catch
			{
				directories = new string[0];
			}
			string[] array = directories;
			foreach (string directory in array)
			{
				yield return Coroutines.Run(DoVerify(new ContentPack
				{
					folder = directory
				}));
			}
			_packs.Sort((ContentPack x, ContentPack y) => y.version.CompareTo(x.version));
		}

		private IEnumerator DoVerify(ContentPack pack)
		{
			string infoPath = Path.Combine(pack.folder, "info.txt");
			object info = ReadJsonFile(infoPath);
			if (info == null)
			{
				Debug.Log("failed to load info file for content path: " + pack.folder);
				yield break;
			}
			yield return 1;
			pack.version = Dot.Integer("cl", info, -1);
			foreach (object file in Dot.Array("files", info, new ArrayList()))
			{
				pack.packs.Add(Dot.String("pack", file, string.Empty));
			}
			if (pack.version < Version.GetChangeList())
			{
				Debug.LogWarning("pack is too old, deleteing " + pack.version);
				pack.Delete();
				yield break;
			}
			if (pack.packs.Count == 0)
			{
				Debug.LogWarning("pack is empty, removing ");
				pack.Delete();
				yield break;
			}
			foreach (string p in pack.packs)
			{
				string folder = Path.Combine(pack.folder, p);
				if (!Verify(folder))
				{
					Debug.LogWarning("pack verification failed for pack " + p);
					pack.Delete();
					yield break;
				}
			}
			Debug.Log("content is ok at " + pack.folder);
			_packs.Add(pack);
		}

		private bool Verify(string folder)
		{
			string path = Path.Combine(folder, "files.txt");
			ArrayList arrayList = (ArrayList)ReadJsonFile(path);
			if (arrayList == null)
			{
				return false;
			}
			foreach (object item in arrayList)
			{
				string path2 = Dot.String("0", item, string.Empty);
				long size = Dot.Long("1", item, 0L);
				string text = Path.Combine(folder, path2);
				if (!CheckFile(text, size))
				{
					Debug.LogError("failed verification on file  " + text);
					return false;
				}
			}
			return true;
		}

		public static Coroutine CheckForNewContent(Action<bool> callback)
		{
			if (Application.internetReachability == NetworkReachability.NotReachable)
			{
				callback(false);
				return null;
			}
			EndPoint apiEndPoint = Hub.Instance.ApiEndPoint;
			Request request = apiEndPoint.Get("/content");
			request.AddQuery("platform", Device.MobilePlatform);
			request.AddQuery("cl", Mathf.Max(1, Version.GetChangeList()));
			request.AddQuery("udid", Device.UniqueIdentifier);
			request.AddQuery("check", 1);
			return apiEndPoint.ServiceCoroutine(request, (Response r) =>
			{
				if (r.sucessful)
				{
					int num = Dot.Integer("cl", r.hashtable, 0);
					if (num > 0 && (_mounted == null || _mounted.version != num))
					{
						Debug.Log("New Content Available!");
						callback(true);
						return;
					}
				}
				callback(false);
			});
		}

		private IEnumerator DoCheckServer()
		{
			if (string.IsNullOrEmpty(_baseUrl) || Application.internetReachability == NetworkReachability.NotReachable || (!DownloadOverCarrierDataNetwork && Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork))
			{
				yield break;
			}
			_endpoint = new HttpEndPoint(_baseUrl, default(EndPointOptions));
			Request request = _endpoint.Get("/content");
			request.AddQuery("platform", Device.MobilePlatform);
			request.AddQuery("cl", Mathf.Max(1, Version.GetChangeList()));
			request.AddQuery("udid", Device.UniqueIdentifier);
			request.numRetries = 2;
			Response r = null;
			yield return _endpoint.ServiceCoroutine(request, (Response result) =>
			{
				r = result;
			});
			if (r.sucessful)
			{
				if (r.hashtable == null)
				{
					Debug.Log("server has no content packs, removing old ones");
					_packs.Clear();
					yield break;
				}
				int version = Dot.Integer("cl", r.hashtable, 0);
				ContentPack pack = GetPack(version);
				if (pack == null)
				{
					yield return Coroutines.Run(DoDownload(r.hashtable));
				}
				else
				{
					Mount(pack);
				}
			}
			else
			{
				Debug.Log("Check for content failed: " + r.localizedError);
			}
		}

		private IEnumerator DoDownload(Hashtable info)
		{
			int version = Dot.Integer("cl", info, 0);
			ArrayList files = Dot.Array("files", info, null);
			if (files == null || files.Count == 0)
			{
				Debug.LogError("missing file list!");
				yield break;
			}
			ContentPack pack = new ContentPack
			{
				version = version,
				folder = Path.Combine(BasePath, version.ToString()),
				packs = new List<string>()
			};
			try
			{
				Directory.CreateDirectory(pack.folder);
			}
			catch (Exception ex)
			{
				Exception ex2 = ex;
				Debug.LogError("failed to create directory  " + pack.folder + " " + ex2);
				yield break;
			}
			if (!WriteJsonFile(Path.Combine(pack.folder, "info.txt"), info))
			{
				yield break;
			}
			string cdn = Dot.String("cdn", info, string.Empty);
			using (HttpEndPoint endpoint = new HttpEndPoint(cdn, default(EndPointOptions)))
			{
				ArrayList filesToDownload = new ArrayList();
				foreach (object file in files)
				{
					string url = Dot.String("url", file, string.Empty);
					long size = Dot.Long("size", file, 0L);
					string localPath = Path.Combine(path2: Path.GetFileName(url), path1: pack.folder);
					if (!CheckFile(localPath, size))
					{
						filesToDownload.Add(file);
					}
					pack.packs.Add(Dot.String("pack", file, string.Empty));
				}
				for (int i = 0; i < filesToDownload.Count; i++)
				{
					object file2 = filesToDownload[i];
					string url2 = Dot.String("url", file2, string.Empty);
					long size2 = Dot.Long("size", file2, 0L);
					string localPath2 = Path.Combine(path2: Path.GetFileName(url2), path1: pack.folder);
					yield return Coroutines.Run(DoDownloadFile(i, filesToDownload.Count, size2, endpoint, localPath2, url2));
					if (!CheckFile(localPath2, size2))
					{
						Debug.LogError("Failed to download file! " + url2);
						yield break;
					}
				}
			}
			Notify(Localizer.GetString("ID_SPARX_CONTENT_EXTRACTING"));
			foreach (string p in pack.packs)
			{
				string folder = Path.Combine(pack.folder, p);
				string localPath3 = Path.Combine(pack.folder, p + ".tar.gz");
				if (!Extract(folder, localPath3))
				{
					Debug.LogError("Extraction failed!");
					yield break;
				}
				yield return 1;
			}
			Mount(pack);
		}

		private bool Extract(string directory, string localPath)
		{
			try
			{
				ArrayList arrayList = new ArrayList();
				FileStream stream = new FileStream(localPath, FileMode.Open, FileAccess.Read);
				GZipStream tarredData = new GZipStream(stream, CompressionMode.Decompress);
				TarReader tarReader = new TarReader(tarredData);
				while (tarReader.MoveNext(false))
				{
					ITarHeader fileInfo = tarReader.FileInfo;
					string text = Path.Combine(directory, fileInfo.FileName);
					string fileName = Path.GetFileName(text);
					string path = text.Remove(text.Length - fileName.Length);
					Directory.CreateDirectory(path);
					using (FileStream dataDestanation = new FileStream(text, FileMode.Create, FileAccess.Write))
					{
						tarReader.Read(dataDestanation);
					}
					arrayList.Add(new object[2] { fileInfo.FileName, fileInfo.SizeInBytes });
				}
				if (!WriteJsonFile(Path.Combine(directory, "files.txt"), arrayList))
				{
					return false;
				}
				return true;
			}
			catch (Exception ex)
			{
				Debug.LogError("extraction failed " + ex);
				return false;
			}
		}

		private IEnumerator DoDownloadFile(int index, int count, long size, EndPoint endpoint, string localPath, string remoteUrl)
		{
			Uri uri = new Uri(remoteUrl);
			FileStream stream = null;
			Digest md5 = Digest.MD5();
			string etag = string.Empty;
			long downloaded = 0L;
			Debug.Log("downloading to  " + localPath);
			string localPath2 = default(string);
			Action<WebRequest> onHeaders = (WebRequest r) =>
			{
				if (r.statusCode != 200)
				{
					Debug.LogError("NON-200 " + r.statusCode);
					return;
				}
				etag = r.GetResponseHeader("etag").Replace("\"", string.Empty).ToUpper();
				md5.Reset();
				downloaded = 0L;
				Debug.Log("etag: " + etag);
				try
				{
					if (stream != null)
					{
						stream.Close();
						stream = null;
					}
					stream = new FileStream(localPath2, FileMode.Create, FileAccess.ReadWrite);
				}
				catch (Exception ex)
				{
					Debug.LogError("Failed to create file " + localPath2 + " " + ex);
				}
			};
			Action<byte[]> onData = (byte[] data) =>
			{
				try
				{
					if (stream != null)
					{
						stream.Write(data, 0, data.Length);
						md5.Update(data, 0, data.Length);
						downloaded += data.Length;
					}
				}
				catch (Exception ex)
				{
					Debug.Log("Write Failed!! " + ex.ToString());
				}
			};
			Request request = endpoint.Get(uri.Path);
			request.dataCallback = onData;
			request.headersCallback = onHeaders;
			int id = endpoint.Service(request, (Response r) =>
			{
				if (stream != null)
				{
					stream.Flush();
					stream.Close();
				}
				if (r.sucessful)
				{
					byte[] bytes = md5.Final();
					string text = Encoding.ToHexString(bytes);
					if (!(text != etag))
					{
						return;
					}
					Debug.LogError("md5 failure " + text + "!=" + etag);
				}
				Debug.LogError("download failed, removing file " + localPath2);
				try
				{
					File.Delete(localPath2);
				}
				catch
				{
				}
			});
			WaitForFixedUpdate waiter = new WaitForFixedUpdate();
			string info = string.Empty;
			do
			{
				int progress = (int)(100f * (float)downloaded / (float)size);
				info = Localizer.Format("ID_SPARX_CONTENT_DOWNLOAD_PROGRESS", index + 1, count, progress);
				Notify(info);
				yield return waiter;
			}
			while (!endpoint.IsDone(id));
		}

		public void Dispose()
		{
			if (_endpoint != null)
			{
				_endpoint.Dispose();
			}
		}
	}
}
