using System.Collections;
using UnityEngine;

namespace EB.Sparx
{
	public static class Protect
	{
		private static string _sha1 = string.Empty;

		public static IEnumerator CalculateSha1(Action<string> cb)
		{
			if (!(UnityEngine.Application.platform == UnityEngine.RuntimePlatform.Android))
			{
				_sha1 = "offline";
			}
			if (!string.IsNullOrEmpty(_sha1))
			{
				cb(_sha1);
				yield break;
			}
			string jarPath = Application.dataPath.Replace("jar:file://", string.Empty).Split('!')[0];
			yield return Coroutines.Run(APKSignature.SyncGenerateSHA1SignatureFromAPK(jarPath, (string sha1) =>
			{
				_sha1 = sha1;
			}));
			cb(_sha1);
		}

		public static IEnumerator CalculateHmac(string salt, Action<string> cb)
		{
			string chal = string.Empty;
			if (!(UnityEngine.Application.platform == UnityEngine.RuntimePlatform.Android))
			{
				cb("offline");
				yield break;
			}
			string jarPath = Application.dataPath.Replace("jar:file://", string.Empty).Split('!')[0];
			yield return Coroutines.Run(APKSignature.SyncGenerateHMACSignatureFromAPK(jarPath, salt, (string hmac) =>
			{
				chal = hmac;
			}));
			cb(chal);
		}
	}
}
