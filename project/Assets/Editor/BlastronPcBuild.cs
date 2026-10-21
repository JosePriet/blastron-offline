using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

// Builds the Windows version of Blastron from the AssetRipper export.
// The game streams most content from asset "packs" (see EB.Loader / EB.Assets). The Android packs
// can't be read by the Windows player, so each bundle is rebuilt for Windows from the exported
// assets listed in BlastronBuild/bundles.json, next to the player in android/<pack>/ (the folder
// name the game's own Loader expects on desktop).
//
// Batch: Unity.exe -batchmode -quit -projectPath <project> -executeMethod BlastronPcBuild.BuildAll -logFile build.log
public static class BlastronPcBuild
{
	const BuildTarget Target = BuildTarget.StandaloneWindows;
	const BuildAssetBundleOptions BundleOptions = BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets | BuildAssetBundleOptions.UncompressedAssetBundle | BuildAssetBundleOptions.DeterministicAssetBundle;

	static string ProjectDir { get { return Path.GetDirectoryName(Application.dataPath); } }
	static string SourceDir { get { return Path.Combine(ProjectDir, "BlastronBuild"); } }
	static string OutDir { get { return Path.Combine(Path.GetDirectoryName(ProjectDir), "BlastronPC"); } }

	[MenuItem("Blastron/Build Windows (packs + player)")]
	public static void BuildAll()
	{
		BuildPacks();
		BuildPlayer();
	}

	[MenuItem("Blastron/Build packs only")]
	public static void BuildPacks()
	{
		Hashtable manifest = (Hashtable)EB.JSON.Parse(File.ReadAllText(Path.Combine(SourceDir, "bundles.json")));
		ArrayList order = (ArrayList)EB.JSON.Parse(File.ReadAllText(Path.Combine(SourceDir, "packs.txt")));
		string root = Path.Combine(OutDir, "android");
		Directory.CreateDirectory(root);
		File.Copy(Path.Combine(SourceDir, "packs.txt"), Path.Combine(root, "packs.txt"), true);
		int errors = 0;
		foreach (object packName in order)
		{
			string pack = (string)packName;
			Hashtable info = (Hashtable)manifest[pack];
			string packOut = Path.Combine(root, pack);
			Directory.CreateDirectory(packOut);
			Hashtable toc = (Hashtable)EB.JSON.Parse(File.ReadAllText(Path.Combine(Path.Combine(SourceDir, "packs"), pack + "/toc.txt")));
			Hashtable tocBundles = (Hashtable)toc["bundles"];
			Hashtable bundles = (Hashtable)info["bundles"];

			// parents first, children inside the parent's dependency scope
			List<string> roots = new List<string>();
			foreach (DictionaryEntry e in bundles)
			{
				string parent = (string)((Hashtable)e.Value)["parent"];
				if (string.IsNullOrEmpty(parent) || !bundles.ContainsKey(parent))
				{
					roots.Add((string)e.Key);
				}
			}
			foreach (string id in roots)
			{
				errors += BuildBundleTree(id, bundles, packOut, tocBundles);
			}

			Hashtable scenes = (Hashtable)info["scenes"];
			foreach (DictionaryEntry e in scenes)
			{
				List<string> levels = new List<string>();
				foreach (object sceneName in (ArrayList)e.Value)
				{
					levels.Add(PrepareScene((string)sceneName));
				}
				string path = Path.Combine(packOut, "scene_" + e.Key + ".assetbundle");
				string err = BuildPipeline.BuildStreamedSceneAssetBundle(levels.ToArray(), path, Target);
				if (!string.IsNullOrEmpty(err))
				{
					Debug.LogError("Blastron: scene bundle " + e.Key + " failed: " + err);
					errors++;
				}
			}

			// loose files (UI images read through WWW) are platform independent: copy as-is
			CopyLooseFiles(Path.Combine(Path.Combine(SourceDir, "packs"), pack), packOut);
			File.WriteAllText(Path.Combine(packOut, "toc.txt"), EB.JSON.Stringify(toc));
		}
		if (errors > 0)
		{
			throw new Exception("Blastron: " + errors + " pack build errors, see log");
		}
		Debug.Log("Blastron: packs built in " + root);
	}

	static int BuildBundleTree(string id, Hashtable bundles, string packOut, Hashtable tocBundles)
	{
		int errors = 0;
		BuildPipeline.PushAssetDependencies();
		ArrayList assets = (ArrayList)((Hashtable)bundles[id])["assets"];
		List<UnityEngine.Object> objects = new List<UnityEngine.Object>();
		foreach (object entry in assets)
		{
			ArrayList pair = (ArrayList)entry;
			string assetPath = "Assets/" + (string)pair[1];
			UnityEngine.Object obj = AssetDatabase.LoadMainAssetAtPath(assetPath);
			if (obj == null)
			{
				Debug.LogError("Blastron: missing asset " + assetPath + " for bundle " + id);
				errors++;
				continue;
			}
			objects.Add(obj);
		}
		string path = Path.Combine(packOut, EB.Loader.GetBundlePath(id));
		Directory.CreateDirectory(Path.GetDirectoryName(path));
		// the game loads bundle contents by object name and type (AssetBundle.Load(name, type)),
		// which is how BuildAssetBundle names them; a prefab and a texture may share a name
		if (!BuildPipeline.BuildAssetBundle(null, objects.ToArray(), path, BundleOptions, Target))
		{
			Debug.LogError("Blastron: bundle " + id + " failed");
			errors++;
		}
		else if (tocBundles != null && tocBundles.ContainsKey(id))
		{
			((Hashtable)tocBundles[id])["size"] = new FileInfo(path).Length;
		}
		foreach (DictionaryEntry e in bundles)
		{
			if ((string)((Hashtable)e.Value)["parent"] == id)
			{
				errors += BuildBundleTree((string)e.Key, bundles, packOut, tocBundles);
			}
		}
		BuildPipeline.PopAssetDependencies();
		return errors;
	}

	// AssetRipper names bundle scenes "buildplayer-<scene>"; Application.LoadLevel needs the original name.
	static string PrepareScene(string sceneName)
	{
		string dst = "Assets/BlastronScenes/" + sceneName + ".unity";
		if (!File.Exists(Path.Combine(ProjectDir, dst)))
		{
			Directory.CreateDirectory(Path.Combine(ProjectDir, "Assets/BlastronScenes"));
			string src = "Assets/Scenes/buildplayer-" + sceneName + ".unity";
			if (!AssetDatabase.CopyAsset(src, dst))
			{
				throw new Exception("Blastron: cannot copy scene " + src);
			}
			AssetDatabase.Refresh();
		}
		return dst;
	}

	static void CopyLooseFiles(string src, string dst)
	{
		foreach (string dir in Directory.GetDirectories(src))
		{
			string sub = Path.Combine(dst, Path.GetFileName(dir));
			Directory.CreateDirectory(sub);
			CopyLooseFiles(dir, sub);
		}
		foreach (string file in Directory.GetFiles(src))
		{
			if (Path.GetFileName(file) != "toc.txt")
			{
				File.Copy(file, Path.Combine(dst, Path.GetFileName(file)), true);
			}
		}
	}

	[MenuItem("Blastron/Build player only")]
	public static void BuildPlayer()
	{
		PlayerSettings.companyName = "Blastron Offline";
		PlayerSettings.productName = "Blastron";
		PlayerSettings.defaultIsFullScreen = false;
		PlayerSettings.defaultScreenWidth = 1280;
		PlayerSettings.defaultScreenHeight = 720;
		PlayerSettings.resizableWindow = true;
		PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Enabled;
		PlayerSettings.runInBackground = true;
		string[] levels = new string[] { "Assets/Scenes/2_boot.unity", "Assets/Scenes/3_empty.unity" };
		string err = BuildPipeline.BuildPlayer(levels, Path.Combine(OutDir, "Blastron.exe"), Target, BuildOptions.None);
		if (!string.IsNullOrEmpty(err))
		{
			throw new Exception("Blastron: player build failed: " + err);
		}
		Debug.Log("Blastron: player built in " + OutDir);
	}
}
