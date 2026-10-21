using System;
using System.Collections;
using System.Runtime.InteropServices;
using EB;
using UnityEngine;

public class APKSignature
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

	[DllImport("apkprotect")]
	private static extern bool APK_Check_Start(string jarPath);

	[DllImport("apkprotect")]
	private static extern bool APK_Check_Start_Salt(string jarPath, string salt);

	[DllImport("apkprotect")]
	private static extern bool APK_Check_Step(int maxBytesToRead, int maxFilesToExamine, int stepsBeforeLog);

	[DllImport("apkprotect")]
	private static extern string APK_Check_Complete();

	public static IEnumerator SyncGenerateSHA1SignatureFromAPK(string jarPath, EB.Action<string> callback)
	{
		string signature = string.Empty;
		if (APK_Check_Start(jarPath))
		{
			bool complete = false;
			do
			{
				yield return 1;
			}
			while (!APK_Check_Step(51200, 20, 10));
			signature = APK_Check_Complete();
		}
		callback(signature);
	}

	public static IEnumerator SyncGenerateHMACSignatureFromAPK(string jarPath, string salt, EB.Action<string> callback)
	{
		string signature = string.Empty;
		if (APK_Check_Start_Salt(jarPath, salt))
		{
			bool complete = false;
			do
			{
				yield return 1;
			}
			while (!APK_Check_Step(51200, 20, 10));
			signature = APK_Check_Complete();
		}
		callback(signature);
	}
}
