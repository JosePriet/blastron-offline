using System.Collections.Generic;
using EB;
using EB.Sequence;
using EB.Sparx;
using UnityEngine;

public class CampaignUI : MonoBehaviour
{
	private const int maxWeaponItems = 6;

	public float dragThreshold;

	private int selectedMission;

	private List<SinglePlayerPrefab> _missions;

	private UITexture levelImage;

	private bool _enableInput;

	private string currentLevelTexture = string.Empty;

	private void OnOpenPopup()
	{
		_enableInput = true;
		_missions = CampaignData.Instance.Missions;
		selectedMission = 0;
		if (Profile.Instance.IsTutorialDone("firstboot_shown"))
		{
			for (int i = 0; i < _missions.Count && UnlocksManager.Instance.IsUnlocked(_missions[i].name); i++)
			{
				selectedMission = i;
			}
		}
		ScreenManager.Instance.AddPersistentScreen("StatusBar");
		ScreenManager.Instance.AddPersistentScreen("BackgroundUI");
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "BackButton");
		objectExactMatch.GetComponent<ButtonScript>().onButtonClick += OnBack;
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "PlayButton");
		objectExactMatch2.GetComponent<ButtonScript>().onButtonClick += OnStart;
		GameObject objectExactMatch3 = Util.GetObjectExactMatch(base.gameObject, "ButtonLeft");
		objectExactMatch3.GetComponent<ButtonScript>().onButtonClick += OnLeft;
		GameObject objectExactMatch4 = Util.GetObjectExactMatch(base.gameObject, "ButtonRight");
		objectExactMatch4.GetComponent<ButtonScript>().onButtonClick += OnRight;
		levelImage = Util.GetObjectExactMatch(base.gameObject, "LevelImage").GetComponent<UITexture>();
		if (CampaignData.Instance.IsCampaignPlus())
		{
			GameObject objectExactMatch5 = Util.GetObjectExactMatch(base.gameObject, "GameInfoPanel");
			GameObject objectExactMatch6 = Util.GetObjectExactMatch(objectExactMatch5, "Label");
			objectExactMatch6.GetComponent<UILabel>().text = "ID_UI_CAMPAIGN_TITLE_PLUS";
		}
		SetupMissionInfo();
		EBUI_TransitionManager.Instance.RunIntroTransitions(base.gameObject, () =>
		{
			EB.Sequence.Component.Activate(base.gameObject, "CampaignUI", typeof(SequenceEvent_OnScreenOpened));
		});
		if (Profile.Instance.IsTutorialDone("firstboot_shown") || Hub.Instance.State == HubState.Idle)
		{
			objectExactMatch2.collider.enabled = false;
			return;
		}
		Util.GetObjectExactMatch(base.gameObject, "MickETexture").SetActive(false);
		GameObject objectExactMatch7 = Util.GetObjectExactMatch(objectExactMatch2, "Background");
		objectExactMatch7.collider.enabled = false;
	}

	private void LoadLevelTexture(string newLevelTexture)
	{
		ReleaseCurrentLevelTexture();
		currentLevelTexture = newLevelTexture;
		TexturePoolManager.Instance.LoadTexture(newLevelTexture, this, OnTextureLoaded);
	}

	private void ReleaseCurrentLevelTexture()
	{
		if (currentLevelTexture != null)
		{
			TexturePoolManager.Instance.ReleaseTexture(currentLevelTexture);
			currentLevelTexture = string.Empty;
		}
	}

	private void OnTextureLoaded(Texture2D t)
	{
		if (t.name == currentLevelTexture)
		{
			levelImage.mainTexture = t;
			levelImage.GetComponent<TextureFade>().SetTexture(levelImage);
		}
	}

	private void EnableInput(bool enable)
	{
		_enableInput = enable;
	}

	private void SetupMissionInfo()
	{
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "GameInfoPanel");
		UIUtils.SetLabelContents(objectExactMatch, "MissionName", _missions[selectedMission].missionName);
		UIUtils.SetLabelContents(objectExactMatch, "LevelName", GetLocalizedLevelName(_missions[selectedMission].Level).ToUpper());
		UnityEngine.Debug.Log(string.Format("MISSION NAME {0} LEVEL {1} LOCAL_LEVEL {2}", _missions[selectedMission].missionName, _missions[selectedMission].Level, GetLocalizedLevelName(_missions[selectedMission].Level).ToUpper()));
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "WeaponsInfoPanel");
		UIUtils.SetLabelContents(objectExactMatch2, "Description", _missions[selectedMission].missionDialogDesc);
		UIUtils.SetLabelContents(objectExactMatch2, "Subtitle", _missions[selectedMission].missionSubtitle);
		int num = selectedMission + 1;
		string text = num.ToString();
		if (num < 10)
		{
			text = "0" + text;
		}
		UIUtils.SetLabelContents(objectExactMatch, "LevelNumber", text);
		GameObject objectExactMatch3 = Util.GetObjectExactMatch(base.gameObject, "PlayButton");
		GameObject objectExactMatch4 = Util.GetObjectExactMatch(base.gameObject, "Lock");
		levelImage.GetComponent<TextureFade>().HideTexture();
		string text2 = _missions[selectedMission].LevelTexture;
		if (string.IsNullOrEmpty(text2))
		{
			text2 = _missions[selectedMission].Level;
		}
		LoadLevelTexture("levelthumb_ui/ui_" + text2);
		objectExactMatch3.SetActive(true);
	}

	private string GetLocalizedLevelName(string level)
	{
		string id = string.Format("ID_GAME_ATTRIBUTE_LEVEL_{0}", level).ToUpper();
		string value = string.Empty;
		if (Localizer.GetString(id, out value))
		{
			return value;
		}
		return value;
	}

	private void OnBack(GameObject caller)
	{
		if (_enableInput)
		{
			EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, OnExitToPrevious);
		}
	}

	private void OnExitToPrevious()
	{
		ScreenManager.Instance.ClosePopup();
	}

	private void OnExitToStart()
	{
		ScreenManager.Instance.FadeOut(() =>
		{
			ScreenManager.Instance.ClearPersistentScreens();
			CampaignData.Instance.CurrentMission = selectedMission;
			_missions[selectedMission].Play();
		});
	}

	private void OnStart(GameObject caller)
	{
		if (_enableInput)
		{
			EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, OnExitToStart);
		}
	}

	private void OnFirstBootTutorial()
	{
		EB.Debug.LogWarning("OnFirstBootTutorial");
		if (UnlocksManager.Instance.IsUnlocked(_missions[selectedMission].name))
		{
			EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, OnExitToStart);
		}
	}

	private void OnLeft(GameObject caller)
	{
		if (_enableInput)
		{
			selectedMission--;
			if (selectedMission < 0)
			{
				selectedMission = _missions.Count - 1;
			}
			SetupMissionInfo();
		}
	}

	private void OnRight(GameObject caller)
	{
		if (_enableInput)
		{
			selectedMission++;
			if (selectedMission >= _missions.Count)
			{
				selectedMission = 0;
			}
			SetupMissionInfo();
		}
	}
}
