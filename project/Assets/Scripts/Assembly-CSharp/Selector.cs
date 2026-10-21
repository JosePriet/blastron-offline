using System;
using System.Collections.Generic;
using EB;
using EB.Sparx;
using UnityEngine;

public class Selector : MonoBehaviour
{
	private int _language;

	private List<string> languages = new List<string>();

	private List<Language> languageValues = new List<Language>();

	private string[] localIps = new string[6] { "127.0.0.1", "10.10.1.106", "10.10.1.111", "10.210.3.13", "10.210.0.37", "10.210.3.11" };

	private void Awake()
	{
		foreach (int value in Enum.GetValues(typeof(Language)))
		{
			if (value != 0)
			{
				languages.Add(((Language)value).ToString());
				languageValues.Add((Language)value);
			}
			else
			{
				languages.Add("Auto");
				languageValues.Add((Language)value);
			}
		}
		Setup.NoStats = true;
	}

	private void Start()
	{
	}

	private void OnGUI()
	{
		int num = Screen.width / 2;
		int num2 = Screen.height / 7;
		GUILayout.BeginArea(new Rect(0f, 0f, Screen.width, Screen.height));
		GUILayout.BeginVertical();
		GUILayout.FlexibleSpace();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Production", GUILayout.Width(num / 2), GUILayout.Height(num2)))
		{
			Setup.ApiEndPointOverride = "https://api.rivets.sparx.io";
			Application.LoadLevel("2_boot");
		}
		if (GUILayout.Button("Development", GUILayout.Width(num / 2), GUILayout.Height(num2)))
		{
			Setup.ApiEndPointOverride = "https://api.rivets-dev.sparx.io";
			Application.LoadLevel("2_boot");
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		string[] array = localIps;
		foreach (string text in array)
		{
			if (GUILayout.Button(text, GUILayout.Width(num / localIps.Length), GUILayout.Height(num2)))
			{
				Setup.ApiEndPointOverride = "https://" + text;
				Application.LoadLevel("2_boot");
			}
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		int num3 = 8;
		if (GUILayout.Button("New UDID", GUILayout.Width(num / num3), GUILayout.Height(num2)))
		{
			PlayerPrefs.DeleteAll();
			string text2 = Device.NewUniqueIdentifier();
			PlayerPrefs.Save();
			EB.Debug.Log("udid: " + text2);
		}
		if (GUILayout.Button("Clear UDID", GUILayout.Width(num / num3), GUILayout.Height(num2)))
		{
			PlayerPrefs.DeleteAll();
			PlayerPrefs.Save();
			EB.Debug.Log("udid: " + Device.UniqueIdentifier);
		}
		if (GUILayout.Button("Clear Login Data", GUILayout.Width(num / num3), GUILayout.Height(num2)))
		{
			SecurePrefs.DeleteKey("credentials", "guestlogin", "facebooklogin", "credentials", "access_token");
		}
		if (GUILayout.Button("No tutorials", GUILayout.Width(num / num3), GUILayout.Height(num2)))
		{
			Profile.TutorialSkip = true;
		}
		if (GUILayout.Button("Rainbow", GUILayout.Width(num / num3), GUILayout.Height(num2)))
		{
			Localizer.ShowStatuses = true;
		}
		if (GUILayout.Button("String IDs", GUILayout.Width(num / num3), GUILayout.Height(num2)))
		{
			Localizer.ShowStringIds = true;
		}
		if (GUILayout.Button("Protect", GUILayout.Width(num / num3), GUILayout.Height(num2)))
		{
			Coroutines.Run(Protect.CalculateSha1((string sha1) =>
			{
				UnityEngine.Debug.Log("sha1 = " + sha1);
			}));
		}
		if (GUILayout.Button("WebView", GUILayout.Width(num / num3), GUILayout.Height(num2)))
		{
			Vector2 pt = new Vector2(0.2070313f, 0.1614583f);
			Vector2 size = new Vector2(75f / 128f, 25f / 32f);
			WebViewDialogUI.TestOpenUrl("Testing", "http://www.example.com", pt, size);
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Show Longest String in All Languages (CPU/MEM!!)", GUILayout.Width(num), GUILayout.Height(num2)))
		{
			Localizer.ShowLongestStrings = true;
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		_language = GUILayout.SelectionGrid(_language, languages.ToArray(), 6, GUILayout.Height(num2 * 2), GUILayout.Width(num));
		Setup.Locale = languageValues[_language];
		Setup.ForceSelectLanguage = true;
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.EndVertical();
		GUILayout.EndArea();
	}
}
