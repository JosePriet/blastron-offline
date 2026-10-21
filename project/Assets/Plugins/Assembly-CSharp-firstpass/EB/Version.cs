using System.Collections;
using EB.Sparx;
using UnityEngine;

namespace EB
{
	public static class Version
	{
		private static string _version = string.Empty;

		public static string GetUDID()
		{
			return SystemInfo.deviceUniqueIdentifier;
		}

		public static Hashtable GetDeviceInfo()
		{
			Hashtable hashtable = new Hashtable();
			hashtable["mac"] = GetMACAddress();
			hashtable["device_id"] = GetAndroidDeviceID();
			hashtable["model"] = SystemInfo.deviceModel;
			return hashtable;
		}

		public static string GetVersion()
		{
			if (string.IsNullOrEmpty(_version))
			{
				_version = Assets.Load<TextAsset>("version").text;
			}
			return _version;
		}

		public static string GetLocale()
		{
			return GetLanguageCode() + "_" + GetCountryCode();
		}

		public static Language GetDefaultLanguageFromLanguageCode()
		{
			string preferredLanguageCode = GetPreferredLanguageCode();
			Debug.Log("----------Language: " + preferredLanguageCode);
			if (Symbols.LanguageCode.ContainsKey(preferredLanguageCode))
			{
				return Symbols.LanguageCode[preferredLanguageCode];
			}
			return Symbols.LanguageCode["en"];
		}

		public static bool IsCountryUS()
		{
			string countryCode = GetCountryCode();
			if (Symbols.CountryCode.ContainsKey(countryCode) && Symbols.CountryCode[countryCode] == Country.USA)
			{
				return true;
			}
			return false;
		}

		public static Country GetCountryFromCountryCode(string country)
		{
			Debug.Log("----------Country: " + country);
			if (Symbols.CountryCode.ContainsKey(country))
			{
				return Symbols.CountryCode[country];
			}
			return Symbols.CountryCode["US"];
		}

		public static string GetPreferredLanguageCode()
		{
			return GetLanguageCode();
		}

		public static string GetLanguageCode()
		{
			switch (Application.systemLanguage)
			{
			case SystemLanguage.English:
				return Localizer.GetLanguageCode(Language.English);
			case SystemLanguage.French:
				return Localizer.GetLanguageCode(Language.French);
			case SystemLanguage.Italian:
				return Localizer.GetLanguageCode(Language.Italian);
			case SystemLanguage.German:
				return Localizer.GetLanguageCode(Language.German);
			case SystemLanguage.Spanish:
				return Localizer.GetLanguageCode(Language.Spanish);
			case SystemLanguage.Portuguese:
				return Localizer.GetLanguageCode(Language.Portuguese);
			case SystemLanguage.Russian:
				return Localizer.GetLanguageCode(Language.Russian);
			case SystemLanguage.Korean:
				return Localizer.GetLanguageCode(Language.Korean);
			case SystemLanguage.Chinese:
			{
				if (!(UnityEngine.Application.platform == UnityEngine.RuntimePlatform.Android))
				{
					return Localizer.GetLanguageCode(Language.ChineseSimplified);
				}
				AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
				AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
				AndroidJavaObject androidJavaObject2 = androidJavaObject.Call<AndroidJavaObject>("getResources", new object[0]);
				AndroidJavaObject androidJavaObject3 = androidJavaObject2.Call<AndroidJavaObject>("getConfiguration", new object[0]);
				AndroidJavaObject androidJavaObject4 = androidJavaObject3.Get<AndroidJavaObject>("locale");
				switch (androidJavaObject4.Call<string>("toString", new object[0]))
				{
				case "zh":
				case "zh_CN":
				case "zh_rCN":
				case "zh_Hans":
				case "zh_Hans_CN":
				case "zh_Hans_HK":
				case "zh_Hans_MO":
				case "zh_Hans_SG":
				case "zh_SG":
					return Localizer.GetLanguageCode(Language.ChineseSimplified);
				case "zh_TW":
				case "zh_rTW":
				case "zh_Hant":
				case "zh_Hant_HK":
				case "zh_HK":
				case "zh_Hant_MO":
				case "zh_MO":
				case "zh_Hant_TW":
					return Localizer.GetLanguageCode(Language.ChineseTraditional);
				default:
					return Localizer.GetLanguageCode(Language.ChineseSimplified);
				}
			}
			case SystemLanguage.Japanese:
				return Localizer.GetLanguageCode(Language.Japanese);
			case SystemLanguage.Turkish:
				return Localizer.GetLanguageCode(Language.Turkish);
			default:
				return Localizer.GetLanguageCode(Language.English);
			}
		}

		public static Country GetCountry()
		{
			string country = GetCountryCode();
			string country2 = (Hub.Instance.LoginManager as LoginManagerKabam).Country;
			if (!string.IsNullOrEmpty(country2))
			{
				country = country2;
			}
			return GetCountryFromCountryCode(country);
		}

		public static string GetCountryCode()
		{
			if (!(UnityEngine.Application.platform == UnityEngine.RuntimePlatform.Android))
			{
				return "US";
			}
			AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
			AndroidJavaObject androidJavaObject2 = androidJavaObject.Call<AndroidJavaObject>("getResources", new object[0]);
			AndroidJavaObject androidJavaObject3 = androidJavaObject2.Call<AndroidJavaObject>("getConfiguration", new object[0]);
			AndroidJavaObject androidJavaObject4 = androidJavaObject3.Get<AndroidJavaObject>("locale");
			return androidJavaObject4.Call<string>("getCountry", new object[0]);
		}

		public static int GetChangeList()
		{
			string version = GetVersion();
			try
			{
				string[] array = version.Split('.');
				return int.Parse(array[array.Length - 1]);
			}
			catch
			{
				return 0;
			}
		}

		public static string GetAndroidDeviceID()
		{
			string result = string.Empty;
			if (!(UnityEngine.Application.platform == UnityEngine.RuntimePlatform.Android))
			{
				return UnityEngine.SystemInfo.deviceUniqueIdentifier;
			}
			AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.explodingbarrel.android.UnityAndroidDeviceInfo");
			if (androidJavaClass != null)
			{
				result = androidJavaClass.CallStatic<string>("GetDeviceID", new object[0]);
			}
			return result;
		}

		public static string GetMACAddress()
		{
			string result = string.Empty;
			if (!(UnityEngine.Application.platform == UnityEngine.RuntimePlatform.Android))
			{
				return UnityEngine.SystemInfo.deviceUniqueIdentifier;
			}
			AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.explodingbarrel.android.UnityAndroidDeviceInfo");
			if (androidJavaClass != null)
			{
				result = androidJavaClass.CallStatic<string>("WifiMacAddress", new object[0]);
				result = result.Replace(":", string.Empty);
			}
			return result;
		}
	}
}
