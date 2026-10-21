using System;
using System.Collections.Generic;
using EB;
using EB.Sparx;
using UnityEngine;

public class StageItemControl : MonoBehaviour
{
	public GameObject Completed;

	public GameObject Locked;

	public GameObject Current;

	public GameObject StarPanel;

	public GameObject FriendPortrait;

	private int _division;

	private Division.Stage _stage;

	private UITexture _currentStageTextureHolder;

	private UITexture _friendTextureHolder;

	private TextureFade _currentStageFader;

	private TextureFade _friendFader;

	private string _stageTexPath;

	private Vector3 _friendPortraitOriginalPosition;

	private List<UIWidget> _activeWidgets;

	[HideInInspector]
	public string FriendFacebookId;

	private UIPanelAlpha _uiRootPanelAlpha;

	private GameObject _activeContents;

	private float _alpha = 255f;

	public TerritoryControl Territory { get; set; }

	public int Division
	{
		get
		{
			return _division;
		}
		set
		{
			_division = value;
		}
	}

	public Division.Stage Stage
	{
		get
		{
			return _stage;
		}
		set
		{
			_stage = value;
		}
	}

	public bool IsLocked
	{
		get
		{
			return !ChampionshipData.Instance.IsStageUnlocked(_division, _stage);
		}
	}

	public float Alpha
	{
		set
		{
			_alpha = value;
			if ((bool)_uiRootPanelAlpha && _uiRootPanelAlpha.enabled)
			{
				_uiRootPanelAlpha.alpha = _alpha * SinglePlayerUI.GlobalAlpha;
			}
			else if (_activeWidgets != null && _activeWidgets.Count > 0)
			{
				float a = _alpha * SinglePlayerUI.GlobalAlpha;
				for (int i = 0; i < _activeWidgets.Count; i++)
				{
					UIWidget uIWidget = _activeWidgets[i];
					Color color = uIWidget.color;
					color.a = a;
					uIWidget.color = color;
				}
			}
		}
	}

	private void Awake()
	{
		_uiRootPanelAlpha = GetComponent<UIPanelAlpha>();
		_currentStageTextureHolder = Current.GetComponentInChildren<UITexture>();
		_currentStageFader = _currentStageTextureHolder.gameObject.AddComponent<TextureFade>();
		_friendTextureHolder = FriendPortrait.GetComponentInChildren<UITexture>();
		_friendFader = _friendTextureHolder.gameObject.AddComponent<TextureFade>();
		_friendPortraitOriginalPosition = FriendPortrait.transform.localPosition;
	}

	private void OnDestroy()
	{
		TexturePoolManager.Instance.ReleaseTexture(_stageTexPath);
	}

	private void OnClick()
	{
		if (_alpha > 0.1f && ChampionshipData.Instance.IsStageUnlocked(_division, _stage))
		{
			base.transform.parent.SendMessage("OnStageTouched", _stage, SendMessageOptions.RequireReceiver);
		}
	}

	public void Refresh()
	{
		if (IsLocked)
		{
			Locked.SetActive(true);
			Completed.SetActive(false);
			Current.SetActive(false);
			_activeContents = Locked;
			FriendPortrait.transform.localPosition = _friendPortraitOriginalPosition;
		}
		else
		{
			int num = ChampionshipData.StarState(_division, _stage);
			if (num >= 0)
			{
				Completed.SetActive(true);
				Locked.SetActive(false);
				Current.SetActive(false);
				_activeContents = Completed;
				GameObject[] objects = Util.GetObjects(StarPanel, "Sprite");
				Array.Sort(objects, (GameObject g0, GameObject g1) => g0.name.CompareTo(g1.name));
				for (int num2 = 0; num2 < objects.Length; num2++)
				{
					objects[num2].GetComponent<UISprite>().spriteName = ((num < num2) ? "upgrade_star_empty" : "upgrade_star_full");
				}
				FriendPortrait.transform.localPosition = _friendPortraitOriginalPosition;
			}
			else
			{
				Current.SetActive(true);
				Completed.SetActive(false);
				Locked.SetActive(false);
				_activeContents = Current;
				if (!string.IsNullOrEmpty(_stageTexPath))
				{
					_currentStageFader.HideTexture();
					TexturePoolManager.Instance.ReleaseTexture(_stageTexPath);
					_currentStageTextureHolder.enabled = false;
				}
				_stageTexPath = "championship_stageicons/camp_" + _stage._level.ToLower() + ".png";
				TexturePoolManager.Instance.LoadTexture(_stageTexPath, (Texture2D loadedTex) =>
				{
					if (loadedTex != null && _currentStageTextureHolder != null)
					{
						_currentStageTextureHolder.enabled = true;
						_currentStageTextureHolder.mainTexture = loadedTex;
						_currentStageFader.SetTexture(_currentStageTextureHolder);
						_currentStageTextureHolder.shader = Shader.Find("Unlit/Transparent Colored");
					}
					else
					{
						EB.Debug.LogError("failed to load championship level icon " + _stageTexPath);
						_stageTexPath = null;
					}
				});
				FriendPortrait.transform.localPosition = _friendPortraitOriginalPosition + new Vector3(55f, 0f, 0f);
			}
		}
		Division division = ChampionshipData.Instance.GetDivision(_division);
		int num3 = division.FindIndexForStage(_stage);
		Util.GetObjectExactMatch(_activeContents, "Label").GetComponent<UILabel>().text = (num3 + 1).ToString();
		FriendPortrait.SetActive(false);
		_friendFader.HideTexture();
		if (FriendFacebookId != null)
		{
			string imageUrlForUser = FacebookUser.GetImageUrlForUser(FriendFacebookId, FacebookUser.ImageType.square);
			FacebookPortraitManager.Load(imageUrlForUser, (Texture2D loadedTex) =>
			{
				if (loadedTex != null && FriendPortrait != null)
				{
					FriendPortrait.SetActive(true);
					_friendTextureHolder.mainTexture = loadedTex;
					_friendFader.SetTexture(_friendTextureHolder);
					_friendTextureHolder.shader = Shader.Find("Unlit/Transparent Colored");
				}
			});
		}
		UIWidget[] array = Util.FindAllComponents<UIWidget>(_activeContents);
		UIWidget[] array2 = Util.FindAllComponents<UIWidget>(FriendPortrait);
		_activeWidgets = new List<UIWidget>(array.Length + array2.Length);
		UIWidget[] array3 = array;
		foreach (UIWidget item in array3)
		{
			_activeWidgets.Add(item);
		}
		UIWidget[] array4 = array2;
		foreach (UIWidget item2 in array4)
		{
			_activeWidgets.Add(item2);
		}
	}
}
