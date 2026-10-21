using System;
using System.Collections.Generic;
using EB;
using EB.Sparx;
using UnityEngine;

public class CampaignHighScoreUI : PopupScript
{
	private const float _popupBackgroundDiff = 172f;

	private const int MAX_FRIENDS_SHOWN = 3;

	public float FBPortraitSpace = 10f;

	private UILabel _popupTitleLabel;

	private UILabel _stageLabel;

	private GameObject _popupBackground;

	private GameObject _topNoFB;

	private GameObject _topFB;

	private UITexture _playerPortrait;

	private GameObject _newRank;

	private UILabel _newRankLabel;

	private UISprite _newRankDeltaIcon;

	private UILabel _newRankDeltaLabel;

	private GameObject _loading;

	private GameObject _bottom;

	private GameObject _bottomFB;

	private GameObject _friendPortraitReference;

	private UILabel _activeScoreLabel;

	private Vector3 _popupBackgroundFullSize;

	internal int _highScore;

	internal Scoring.ScoringMode _scoreMode;

	private void Awake()
	{
		base.OnAwake();
		_popupTitleLabel = Util.GetObjectExactMatch(base.gameObject, "LabelPopupTitle").GetComponent<UILabel>();
		_stageLabel = Util.GetObjectExactMatch(base.gameObject, "StageLabel").GetComponent<UILabel>();
		_popupBackground = Util.GetObjectExactMatch(base.gameObject, "Background");
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "Top");
		_topNoFB = Util.GetObjectExactMatch(objectExactMatch, "NoFB");
		_topFB = Util.GetObjectExactMatch(objectExactMatch, "FB");
		_loading = Util.GetObjectExactMatch(_topFB, "Loading");
		_loading.SetActive(false);
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(_topFB, "PortraitContainer");
		_playerPortrait = Util.FindComponent<UITexture>(objectExactMatch2);
		_newRank = Util.GetObjectExactMatch(_topFB, "NewRank");
		_newRankLabel = Util.GetObjectExactMatch(_newRank, "NewRankNumberLabel").GetComponent<UILabel>();
		_newRankDeltaIcon = Util.GetObjectExactMatch(_newRank, "RankDeltaIcon").GetComponent<UISprite>();
		_newRankDeltaLabel = Util.GetObjectExactMatch(_newRank, "RankDeltaLabel").GetComponent<UILabel>();
		_bottom = Util.GetObjectExactMatch(base.gameObject, "Bottom");
		_bottomFB = Util.GetObjectExactMatch(_bottom, "FB");
		_friendPortraitReference = Util.GetObjectExactMatch(_bottomFB, "PortraitReference");
		_friendPortraitReference.SetActive(false);
	}

	private void Start()
	{
		base.OnStart();
		_popupBackgroundFullSize = _popupBackground.transform.localScale;
		Refresh();
	}

	public void Refresh()
	{
		bool flag = Hub.Instance.FacebookManager.HasAccessToken && Hub.Instance.FacebookManager.AppFriends.Length > 0;
		_topFB.SetActive(flag);
		_topNoFB.SetActive(!flag);
		_bottomFB.SetActive(flag);
		_popupBackground.transform.localScale = _popupBackgroundFullSize - new Vector3(0f, 172f, 0f);
		_bottom.SetActive(false);
		_newRank.SetActive(false);
		int div = ChampionshipData.Instance.GetSelectedDivision();
		Division division = ChampionshipData.Instance.GetDivision(div);
		Division.Stage selectedStage = ChampionshipData.Instance.GetSelectedStage();
		int stageIndex = division.FindIndexForStage(selectedStage);
		if (stageIndex == -1)
		{
			EB.Debug.LogError("invalid stage for division!");
			return;
		}
		_stageLabel.text = Localizer.GetString("ID_GAME_ATTRIBUTE_LEVEL_" + selectedStage._level.ToUpper());
		if (flag)
		{
			_popupTitleLabel.text = "ID_UI_HIGHSCORE_TITLE_FB";
			_activeScoreLabel = Util.GetObjectExactMatch(_topFB, "NumScore").GetComponent<UILabel>();
			_loading.SetActive(true);
			ChampionshipLeaderboards.Instance.GetUserPositionAmongFriends(div, stageIndex, (int userPos, int size, string error) =>
			{
				if (!string.IsNullOrEmpty(error))
				{
					EB.Debug.LogError("error reading user position: " + error);
					_loading.SetActive(false);
				}
				else
				{
					int cachedRank = ChampionshipLeaderboards.Instance.CachedRank;
					EB.Debug.Log("First Time: " + RewardsManager.Instance.ChampionshipFirstTimeAttempt + ", previous rank: " + cachedRank + ", new rank: " + userPos);
					if (RewardsManager.Instance.ChampionshipFirstTimeAttempt)
					{
						_newRank.SetActive(true);
						_newRankLabel.text = "#" + (userPos + 1);
						_newRankDeltaIcon.enabled = false;
						_newRankDeltaLabel.enabled = false;
					}
					else if (userPos != cachedRank)
					{
						_newRankLabel.text = "#" + (userPos + 1);
						_newRankDeltaIcon.enabled = cachedRank != -1;
						_newRankDeltaLabel.enabled = cachedRank != -1;
						if (cachedRank != -1)
						{
							if (userPos < cachedRank)
							{
								_newRankDeltaIcon.color = Color.green;
								_newRankDeltaIcon.transform.rotation = Quaternion.Euler(Vector3.zero);
							}
							else if (userPos > cachedRank)
							{
								_newRankDeltaIcon.color = Color.red;
								_newRankDeltaIcon.transform.rotation = Quaternion.Euler(new Vector3(180f, 0f, 0f));
							}
							_newRankDeltaLabel.text = Mathf.Abs(userPos - cachedRank).ToString();
						}
						_newRank.SetActive(true);
						UIWidget[] componentsInChildren = _newRank.GetComponentsInChildren<UIWidget>();
						UIWidget[] array = componentsInChildren;
						foreach (UIWidget uIWidget in array)
						{
							uIWidget.alpha = 0f;
							TweenColor.Begin(uIWidget.gameObject, 0.2f, new Color(uIWidget.color.r, uIWidget.color.g, uIWidget.color.b, 1f));
						}
					}
					else
					{
						_newRank.SetActive(false);
					}
					ChampionshipLeaderboards.Instance.GetFriendScoresBelowPlayer(div, stageIndex, 10, OnLeaderboardRead);
				}
			});
			string imageUrlForUser = FacebookUser.GetImageUrlForUser(Hub.Instance.FacebookManager.LocalUser.FacebookId, 100, 100);
			FacebookPortraitManager.Load(imageUrlForUser, (Texture2D loadedTex) =>
			{
				if (loadedTex != null)
				{
					_playerPortrait.mainTexture = loadedTex;
				}
			});
		}
		else
		{
			_popupTitleLabel.text = "ID_UI_HIGHSCORE_TITLE";
			_activeScoreLabel = Util.GetObjectExactMatch(_topNoFB, "NumScore").GetComponent<UILabel>();
		}
		if (_scoreMode == Scoring.ScoringMode.Kill)
		{
			_activeScoreLabel.text = Localizer.Format("ID_UI_STAGEINFO_FORMAT_KILLS", _highScore);
		}
		else
		{
			_activeScoreLabel.text = Localizer.Format("ID_UI_STAGEINFO_FORMAT_POINTS", _highScore);
		}
	}

	private void OnLeaderboardRead(ChampionshipLeaderboards.PlayerData[] belowPlayers, string error)
	{
		if (!string.IsNullOrEmpty(error))
		{
			EB.Debug.LogError("error reading leaderboards - " + error);
			_loading.SetActive(false);
			return;
		}
		List<ChampionshipLeaderboards.PlayerData> list = new List<ChampionshipLeaderboards.PlayerData>();
		if (belowPlayers != null && belowPlayers.Length > 0)
		{
			ChampionshipLeaderboards.PlayerData p;
			foreach (ChampionshipLeaderboards.PlayerData cachedLeaderboardResult in ChampionshipLeaderboards.Instance.CachedLeaderboardResults)
			{
				p = cachedLeaderboardResult;
				ChampionshipLeaderboards.PlayerData playerData = Array.Find(belowPlayers, (ChampionshipLeaderboards.PlayerData bp) => p != null && bp != null && bp.fbId != null && p.fbId != null && bp.fbId.Equals(p.fbId));
				if (playerData != null && playerData.fbId != Hub.Instance.FacebookManager.LocalUser.FacebookId)
				{
					list.Add(playerData);
				}
			}
		}
		if (list.Count > 0)
		{
			_bottom.SetActive(true);
			SetupFBFriendPortraits(list.ToArray());
			UIWidget[] widgets = _bottom.GetComponentsInChildren<UIWidget>();
			UIWidget[] array = widgets;
			foreach (UIWidget uIWidget in array)
			{
				uIWidget.alpha = 0f;
			}
			AnimateScale.Do(_popupBackground, EZAnimation.ANIM_MODE.To, _popupBackgroundFullSize, EZAnimation.linear, 0.2f, 0f, null, (EZAnimation anim) =>
			{
				UIWidget[] array2 = widgets;
				foreach (UIWidget uIWidget2 in array2)
				{
					TweenColor.Begin(uIWidget2.gameObject, 0.2f, new Color(uIWidget2.color.r, uIWidget2.color.g, uIWidget2.color.b, 1f));
				}
			});
		}
		else
		{
			ClearFBFriendPortraits();
		}
		_loading.SetActive(false);
	}

	private void SetupFBFriendPortraits(ChampionshipLeaderboards.PlayerData[] surpassedUsers)
	{
		ClearFBFriendPortraits();
		int num = Mathf.Min(3, surpassedUsers.Length);
		if (num >= 1)
		{
			GameObject[] array = new GameObject[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = LoadPortrait(i, surpassedUsers[i].fbId);
			}
			float x = Util.GetObjectExactMatch(array[0], "Border").transform.localScale.x;
			float num2 = x * (float)num + FBPortraitSpace * (float)(num - 1);
			float num3 = x + FBPortraitSpace;
			float num4 = (x - num2) / 2f;
			for (int j = 0; j < num; j++)
			{
				Vector3 localPosition = array[j].transform.localPosition;
				localPosition.x = num4;
				array[j].transform.localPosition = localPosition;
				num4 += num3;
			}
		}
	}

	private void ClearFBFriendPortraits()
	{
		GameObject[] objects = Util.GetObjects(_bottomFB, "FBFriendPortrait");
		GameObject[] array = objects;
		foreach (GameObject obj in array)
		{
			UnityEngine.Object.Destroy(obj);
		}
	}

	private GameObject LoadPortrait(int index, string fbId)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(_friendPortraitReference) as GameObject;
		gameObject.transform.parent = _friendPortraitReference.transform.parent;
		gameObject.transform.localPosition = _friendPortraitReference.transform.localPosition;
		gameObject.transform.localScale = _friendPortraitReference.transform.localScale;
		gameObject.name = "FBFriendPortrait" + index;
		gameObject.SetActive(true);
		string imageUrlForUser = FacebookUser.GetImageUrlForUser(fbId, 75, 75);
		if (!string.IsNullOrEmpty(imageUrlForUser))
		{
			UITexture uiTexture = Util.FindComponent<UITexture>(gameObject);
			TextureFade textureFade = Util.FindComponent<TextureFade>(gameObject);
			FacebookPortraitManager.Load(imageUrlForUser, (Texture2D tex) =>
			{
				if (tex != null)
				{
					uiTexture.mainTexture = tex;
					textureFade.SetTexture(uiTexture);
				}
			});
		}
		return gameObject;
	}
}
