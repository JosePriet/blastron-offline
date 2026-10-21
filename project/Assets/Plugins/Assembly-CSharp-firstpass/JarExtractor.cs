using System;
using System.Collections;
using System.IO;
using EB;
using UnityEngine;

public class JarExtractor
{
	private static AndroidJavaClass DetectAndroidJNI;

	public static bool IsValid
	{
		get
		{
			if (DetectAndroidJNI == null)
			{
				DetectAndroidJNI = new AndroidJavaClass("android.os.Build");
			}
			return DetectAndroidJNI.GetRawClass() != IntPtr.Zero;
		}
	}

	private static int GenerateJarLoadID(string jarPath, string filePath)
	{
		return Hash.StringHash(jarPath + ":" + filePath);
	}

	public static IEnumerator SyncLoadFromJar(string jarPath, string filePath, long fileSize, string outputPath, int expectedHash, EB.Action<bool> callback)
	{
		bool success = false;
		if (IsValid)
		{
			long currentOutputSize = 0L;
			try
			{
				currentOutputSize = new FileInfo(outputPath).Length;
			}
			catch
			{
				currentOutputSize = 0L;
			}
			UnityEngine.Debug.Log(string.Format("SyncLoadFromJar JarSize:{0} OutputSize:{1}", fileSize, currentOutputSize));
			string hashfilePath = Path.Combine(Application.persistentDataPath, Path.GetFileName(filePath + ".hash"));
			FileInfo hashfileInfo = new FileInfo(hashfilePath);
			bool shouldExtract = false;
			if (hashfileInfo.Exists)
			{
				int readHash = ReadHashData(hashfileInfo);
				shouldExtract = readHash != expectedHash;
				EB.Debug.Log("expectedHash: " + expectedHash + ", got " + readHash + ", " + ((!shouldExtract) ? "match" : "mismatch"));
			}
			else
			{
				shouldExtract = true;
				EB.Debug.Log("no hash information found");
			}
			if (shouldExtract)
			{
				AndroidJavaObject jarExtractionJNI = new AndroidJavaObject("com.explodingbarrel.android.JarExtraction", jarPath, filePath, outputPath);
				if (jarExtractionJNI != null)
				{
					success = jarExtractionJNI.Call<bool>("StartExtractFromJar", new object[0]);
					if (success)
					{
						bool complete = false;
						do
						{
							yield return 1;
						}
						while (!jarExtractionJNI.Call<bool>("StepExtractFromJar", new object[1] { 51200 }));
						jarExtractionJNI.Call("CompleteExtractFromJar");
						EB.Debug.Log("writing hash information for " + filePath + " => " + hashfilePath);
						WriteHashData(hashfileInfo, expectedHash);
					}
				}
			}
			else
			{
				success = true;
			}
		}
		callback(success);
	}

	private static void WriteHashData(FileInfo fi, int hash)
	{
		try
		{
			fi.Delete();
			StreamWriter streamWriter = new StreamWriter(fi.Create());
			streamWriter.Write(hash);
			streamWriter.Close();
		}
		catch (Exception ex)
		{
			EB.Debug.LogError("WriteHashData caught an exception, skipping write: " + ex.Message);
		}
	}

	private static int ReadHashData(FileInfo fi)
	{
		int result = 0;
		try
		{
			StreamReader streamReader = new StreamReader(fi.OpenRead());
			string s = streamReader.ReadToEnd();
			int.TryParse(s, out result);
		}
		catch (Exception ex)
		{
			EB.Debug.LogError("ReadHashData caught an exception, skipping read: " + ex.Message);
		}
		return result;
	}
}
