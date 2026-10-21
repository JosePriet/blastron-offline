using System.Collections.Generic;
using UnityEngine;

public class GlobalSettings : MonoBehaviour
{
	private const string s_GameObjectName = "GlobalSettings";

	private const string s_IsFullScreenPlayerPrefsKey = "IsFullScreen";

	private const string s_IsAudioMutedPlayerPrefsKey = "IsAudioMuted";

	private const string s_IsMusicMutedPlayerPrefsKey = "IsMusicMuted";

	private const bool s_DefaultIsFullScreenPlayerPref = false;

	private const bool s_DefaultIsAudioMutedPlayerPref = false;

	private const bool s_DefaultIsMusicMutedPlayerPref = false;

	private static GameObject s_GlobalSettingsGameObject;

	private bool m_AreMessageHandlersRegistered;

	private int m_InitialScreenWidth;

	private int m_InitialScreenHeight;

	private Dictionary<string, bool> m_CachedPlayerPrefs = new Dictionary<string, bool>();

	public static void SetUpGlobalGameSettingsGameObject()
	{
		if (null == s_GlobalSettingsGameObject)
		{
			s_GlobalSettingsGameObject = new GameObject("GlobalSettings");
			s_GlobalSettingsGameObject.AddComponent<Persistent>();
			GlobalSettings globalSettings = s_GlobalSettingsGameObject.AddComponent<GlobalSettings>();
			globalSettings.RegisterMessageHandlers();
			globalSettings.m_InitialScreenWidth = Screen.width;
			globalSettings.m_InitialScreenHeight = Screen.height;
			globalSettings.CachePlayerPrefs();
			globalSettings.SetFullScreen(globalSettings.GetCachedPlayerPref("IsFullScreen"));
			globalSettings.MuteAudio(globalSettings.GetCachedPlayerPref("IsAudioMuted"));
			globalSettings.MuteMusic(globalSettings.GetCachedPlayerPref("IsMusicMuted"));
		}
	}

	public static GameObject GetGlobalSettingsGameObject()
	{
		if (null == s_GlobalSettingsGameObject)
		{
			SetUpGlobalGameSettingsGameObject();
		}
		return s_GlobalSettingsGameObject;
	}

	public void Awake()
	{
	}

	public void OnDestroy()
	{
		UnregisterMessageHandlers();
	}

	private void RegisterMessageHandlers()
	{
		if (!m_AreMessageHandlersRegistered)
		{
			Messenger.AddListener(base.gameObject, "ToggleIsFullScreen", OnToggleIsFullScreen);
			Messenger<bool>.AddListener(base.gameObject, "GetIsFullScreen", OnGetIsFullScreen);
			Messenger<bool>.AddListener(base.gameObject, "SetIsFullScreen", OnSetIsFullScreen);
			Messenger.AddListener(base.gameObject, "ToggleIsAudioMuted", OnToggleIsAudioMuted);
			Messenger<bool>.AddListener(base.gameObject, "GetIsAudioMuted", OnGetIsAudioMuted);
			Messenger.AddListener(base.gameObject, "ToggleIsMusicMuted", OnToggleIsMusicMuted);
			Messenger<bool>.AddListener(base.gameObject, "GetIsMusicMuted", OnGetIsMusicMuted);
			m_AreMessageHandlersRegistered = true;
		}
	}

	private void UnregisterMessageHandlers()
	{
		if (m_AreMessageHandlersRegistered)
		{
			Messenger.RemoveListener(base.gameObject, "ToggleIsFullScreen", OnToggleIsFullScreen);
			Messenger<bool>.RemoveListener(base.gameObject, "GetIsFullScreen", OnGetIsFullScreen);
			Messenger<bool>.RemoveListener(base.gameObject, "SetIsFullScreen", OnSetIsFullScreen);
			Messenger.RemoveListener(base.gameObject, "ToggleIsAudioMuted", OnToggleIsAudioMuted);
			Messenger<bool>.RemoveListener(base.gameObject, "GetIsAudioMuted", OnGetIsAudioMuted);
			Messenger.RemoveListener(base.gameObject, "ToggleIsMusicMuted", OnToggleIsMusicMuted);
			Messenger<bool>.RemoveListener(base.gameObject, "GetIsMusicMuted", OnGetIsMusicMuted);
			m_AreMessageHandlersRegistered = false;
		}
	}

	private void OnToggleIsFullScreen()
	{
		SetFullScreen(!GetCachedPlayerPref("IsFullScreen"));
	}

	private void OnGetIsFullScreen(ref bool IsFullScreen)
	{
		IsFullScreen = GetCachedPlayerPref("IsFullScreen");
	}

	private void OnSetIsFullScreen(ref bool DoIsFullScreen)
	{
		SetFullScreen(DoIsFullScreen);
	}

	private void OnToggleIsAudioMuted()
	{
		MuteAudio(!GetCachedPlayerPref("IsAudioMuted"));
	}

	private void OnGetIsAudioMuted(ref bool IsAudioMuted)
	{
		IsAudioMuted = GetCachedPlayerPref("IsAudioMuted");
	}

	private void OnToggleIsMusicMuted()
	{
		MuteMusic(!GetCachedPlayerPref("IsMusicMuted"));
	}

	private void OnGetIsMusicMuted(ref bool IsMusicMuted)
	{
		IsMusicMuted = GetCachedPlayerPref("IsMusicMuted");
	}

	private void CachePlayerPrefs()
	{
		SetCachedPlayerPref("IsFullScreen", GetPlayerPref("IsFullScreen", false));
		SetCachedPlayerPref("IsAudioMuted", GetPlayerPref("IsAudioMuted", false));
		SetCachedPlayerPref("IsMusicMuted", GetPlayerPref("IsMusicMuted", false));
	}

	private bool GetCachedPlayerPref(string Key)
	{
		return m_CachedPlayerPrefs[Key];
	}

	private void SetCachedPlayerPref(string Key, bool Value)
	{
		m_CachedPlayerPrefs[Key] = Value;
	}

	private bool GetPlayerPref(string Key, bool Default)
	{
		return PlayerPrefs.GetInt(Key, Default ? 1 : 0) == 1;
	}

	private void SetPlayerPref(string Key, bool Pref)
	{
		if (Pref != GetCachedPlayerPref(Key))
		{
			PlayerPrefs.SetInt(Key, Pref ? 1 : 0);
			SetCachedPlayerPref(Key, Pref);
		}
	}

	private void SetFullScreen(bool DoFullScreen)
	{
		if (DoFullScreen != Screen.fullScreen)
		{
			int num = 0;
			int num2 = 0;
			if (DoFullScreen)
			{
				num = Screen.resolutions[Screen.resolutions.Length - 1].width;
				num2 = Screen.resolutions[Screen.resolutions.Length - 1].height;
			}
			else
			{
				num = m_InitialScreenWidth;
				num2 = m_InitialScreenHeight;
			}
			Screen.SetResolution(num, num2, DoFullScreen);
		}
		SetPlayerPref("IsFullScreen", DoFullScreen);
	}

	private void MuteAudio(bool DoMuteAudio)
	{
		if (DoMuteAudio)
		{
			AudioListener.volume = 0.0001f;
		}
		else
		{
			AudioListener.volume = 1f;
		}
		SetPlayerPref("IsAudioMuted", DoMuteAudio);
	}

	private void MuteMusic(bool DoMuteMusic)
	{
		bool Argument = true;
		Messenger<bool, bool>.BroadcastToAllListeners("MuteAudioJukebox", ref DoMuteMusic, ref Argument);
		SetPlayerPref("IsMusicMuted", DoMuteMusic);
	}
}
