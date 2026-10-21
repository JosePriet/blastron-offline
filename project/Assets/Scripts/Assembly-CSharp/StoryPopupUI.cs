using System.Collections;
using System.Collections.Generic;
using EB;
using EB.Sparx;
using SmoothMoves;
using UnityEngine;

public class StoryPopupUI : MonoBehaviour
{
	private class CharInfo
	{
		public BoneAnimation _smAnim;

		public float _scale;

		private Color _c0;

		private Color _c1;

		private Color _c;

		private float _ct;

		public Color Color
		{
			get
			{
				return _c;
			}
			set
			{
				if (value != _c1)
				{
					_c0 = _c;
					_c1 = value;
					_ct = 0f;
				}
			}
		}

		public CharInfo()
		{
			_c0 = (_c1 = (_c = Color.white));
			_ct = 100f;
		}

		public void Update()
		{
			if (_ct < 1f && _smAnim != null)
			{
				_c = Color.Lerp(_c0, _c1, _ct);
				_ct += 0.05f;
				_smAnim.SetMeshColor(_c);
			}
		}
	}

	private int _currentStoryPopup;

	public List<CacDisplay> _cacDisplayList = new List<CacDisplay>();

	public UILabel _rightSpeakerLabel;

	public UILabel _leftSpeakerLabel;

	public Color playerTextColor;

	public Color otherTextColor;

	public UILabel _popupLabel;

	public AudioEvent _audioPopupOpen;

	public AudioEvent _audioWhooshIn;

	public AudioEvent _audioWhooshOut;

	internal List<string> _popupListDialog = new List<string>();

	private TypewriterEffect _typewriter;

	private Division.Stage _stage;

	private List<Division.StoryCharacters> _charList = new List<Division.StoryCharacters>();

	private CharacterDB.Recipe _LastLeftRecipe;

	private CharacterDB.Recipe _LastRightRecipe;

	private UILabel _nextButtonLabel;

	private GameObject _Nextbtn;

	private GameObject _rightLabelContainer;

	private GameObject _leftLabelContainer;

	private AudioEmitter _audioEmitter;

	internal int _division;

	private bool isWorldStory;

	private GameObject _transitionBlocker;

	private int _currentSpeaker = -1;

	private CharInfo[] _chars = new CharInfo[2];

	private int CurrentSpeaker
	{
		get
		{
			return _currentSpeaker;
		}
		set
		{
			if (value != _currentSpeaker)
			{
			}
			_currentSpeaker = value;
		}
	}

	private void Awake()
	{
		_transitionBlocker = GameObject.Find("transitionBlocker");
		_chars[0] = new CharInfo();
		_chars[1] = new CharInfo();
		_audioEmitter = GetComponent<AudioEmitter>();
	}

	private List<string> GetStrings()
	{
		List<string> list = new List<string>();
		string[] array = new string[8] { "A", "B", "C", "D", "E", "F", "G", "H" };
		string empty = string.Empty;
		string empty2 = string.Empty;
		if (_division != 0)
		{
			empty2 = ((_division != 1) ? "GOLD" : "SILVER");
		}
		else
		{
			empty2 = "BRONZE";
		}
		if (!StoryManager.Instance._isWorldIntro)
		{
			if (StoryManager.Instance._isStageIntro)
			{
				empty = "IN";
			}
			else
			{
				empty = ((!StoryManager.Instance._isWinOutro) ? "LOSE" : "WIN");
			}
		}
		else
		{
			empty = "W";
			isWorldStory = true;
		}
		int num = ChampionshipData.Instance.GetDivision(_division).FindIndexForStage(StoryManager.Instance._CurrentStageData) + 1;
		string[] array2 = array;
		foreach (string text in array2)
		{
			string id = "ID_" + empty + "_L_STORY_" + empty2 + "_" + num + text;
			if (Localizer.HasString(id))
			{
				list.Add("ID_" + empty + "_L_STORY_" + empty2 + "_" + num + text);
			}
			id = "ID_" + empty + "_R_STORY_" + empty2 + "_" + num + text;
			if (Localizer.HasString(id))
			{
				list.Add("ID_" + empty + "_R_STORY_" + empty2 + "_" + num + text);
			}
		}
		return list;
	}

	private void OnOpenPopup()
	{
		_stage = StoryManager.Instance._CurrentStageData;
		_division = StoryManager.Instance._division;
		_popupListDialog = GetStrings();
		CharactersSetup();
		if (_popupListDialog.Count == 0)
		{
			UnityEngine.Debug.LogWarning("There are no string associated with the current level, however characters are defined");
			NextItem(base.gameObject);
			return;
		}
		_Nextbtn = Util.GetObjectExactMatch(base.gameObject, "NextBtn");
		if (_Nextbtn != null)
		{
			ButtonScript buttonScript = Util.FindComponent<ButtonScript>(_Nextbtn);
			buttonScript.onButtonClick += NextItem;
			_nextButtonLabel = Util.FindComponent<UILabel>(Util.GetObjectExactMatch(_Nextbtn, "LabelNext"));
		}
		_leftLabelContainer = _leftSpeakerLabel.transform.parent.gameObject;
		_rightLabelContainer = _rightSpeakerLabel.transform.parent.gameObject;
		_leftLabelContainer.SetActive(false);
		_rightLabelContainer.SetActive(false);
		_audioEmitter.Play(_audioPopupOpen);
		StartCoroutine(SetStory());
	}

	private void CharactersSetup()
	{
		if (StoryManager.Instance._isWorldIntro)
		{
			StoryManager.Instance._isWorldIntro = false;
			for (int i = 0; i < _stage.worldStory.storycharacters.Count; i++)
			{
				_charList.Add(_stage.worldStory.storycharacters[i]);
			}
			SinglePlayerUI.FadeControls(false, true, true, 0.5f);
			SinglePlayerUI.UserInputToggle();
		}
		else if (StoryManager.Instance._isStageIntro)
		{
			StoryManager.Instance._isStageIntro = false;
			for (int j = 0; j < _stage.introStory.storycharacters.Count; j++)
			{
				_charList.Add(_stage.introStory.storycharacters[j]);
			}
		}
		else if (StoryManager.Instance._isWinOutro)
		{
			StoryManager.Instance._isWinOutro = false;
			for (int k = 0; k < _stage.outroStory.win.storycharacters.Count; k++)
			{
				_charList.Add(_stage.outroStory.win.storycharacters[k]);
			}
		}
		else
		{
			StoryManager.Instance._isLoseOutro = false;
			for (int l = 0; l < _stage.outroStory.lose.storycharacters.Count; l++)
			{
				_charList.Add(_stage.outroStory.lose.storycharacters[l]);
			}
		}
	}

	private IEnumerator SetStory()
	{
		_popupLabel.text = string.Empty;
		_Nextbtn.SetActive(false);
		if (_currentStoryPopup == _popupListDialog.Count - 1)
		{
			_nextButtonLabel.text = Localizer.GetString("ID_UI_COMMON_DONE");
		}
		CurrentSpeaker = ((!_popupListDialog[_currentStoryPopup].Contains("_L_")) ? 1 : 0);
		_popupListDialog[_currentStoryPopup] = Localizer.Format(_popupListDialog[_currentStoryPopup], Hub.Instance.LoginManager.LocalUser.Name);
		if (CurrentSpeaker == 0)
		{
			_popupLabel.color = playerTextColor;
			_leftSpeakerLabel.color = playerTextColor;
			_rightSpeakerLabel.color = Color.white;
		}
		else
		{
			_popupLabel.color = otherTextColor;
			_rightSpeakerLabel.color = otherTextColor;
			_leftSpeakerLabel.color = Color.white;
		}
		for (int i = 0; i < _charList.Count; i++)
		{
			yield return new WaitForEndOfFrame();
			if (_currentStoryPopup >= _charList[i].animationForBots.Length || !(_charList[i].animationForBots[_currentStoryPopup] != string.Empty))
			{
				continue;
			}
			if (_charList[i].side == Division.StoryCharacters.speakerSide.Left)
			{
				int e = i;
				if (_charList[i]._Speaker != null)
				{
					_cacDisplayList[0].Recipe = _charList[i]._Speaker.GetComponent<CharacterPrefab>().ToRecipe();
				}
				else
				{
					_cacDisplayList[0].Recipe = CharacterManager.Instance.LocalPlayer.recipe;
				}
				if (!_cacDisplayList[0].Recipe.Equals(_LastLeftRecipe))
				{
					bool isRunningTransition = true;
					_audioEmitter.Play(_audioWhooshOut);
					EBUI_TransitionManager.Instance.RunOutroTransitions(_cacDisplayList[0].gameObject, SafeAction.Wrap(this, () =>
					{
						isRunningTransition = false;
						_leftLabelContainer.SetActive(false);
						StartCoroutine(SpawnBot(0, _charList[i].animationForBots[_currentStoryPopup]));
					}));
					while (isRunningTransition)
					{
						yield return new WaitForEndOfFrame();
					}
				}
				else
				{
					StartCoroutine(PlayAnim(_charList[e].animationForBots[_currentStoryPopup], 0));
				}
				_LastLeftRecipe = _cacDisplayList[0].Recipe;
				_leftLabelContainer.SetActive(true);
				if (_charList[e]._Speaker != null)
				{
					_leftSpeakerLabel.text = _charList[e]._Speaker.GetComponent<CharacterPrefab>().Name;
					continue;
				}
				_leftLabelContainer.SetActive(true);
				_leftSpeakerLabel.text = Hub.Instance.LoginManager.LocalUser.Name;
			}
			else
			{
				if (_charList[i].side != Division.StoryCharacters.speakerSide.Right)
				{
					continue;
				}
				int e2 = i;
				if (_charList[i]._Speaker != null)
				{
					_cacDisplayList[1].Recipe = _charList[i]._Speaker.GetComponent<CharacterPrefab>().ToRecipe();
				}
				else
				{
					_cacDisplayList[1].Recipe = CharacterManager.Instance.LocalPlayer.recipe;
				}
				if (!_cacDisplayList[1].Recipe.Equals(_LastRightRecipe))
				{
					bool isRunningTransition2 = true;
					_audioEmitter.Play(_audioWhooshOut);
					EBUI_TransitionManager.Instance.RunOutroTransitions(_cacDisplayList[1].gameObject, SafeAction.Wrap(this, () =>
					{
						isRunningTransition2 = false;
						_rightLabelContainer.SetActive(false);
						StartCoroutine(SpawnBot(1, _charList[i].animationForBots[_currentStoryPopup]));
					}));
					while (isRunningTransition2)
					{
						yield return new WaitForEndOfFrame();
					}
				}
				else
				{
					StartCoroutine(PlayAnim(_charList[e2].animationForBots[_currentStoryPopup], 1));
				}
				_LastRightRecipe = _cacDisplayList[1].Recipe;
				if (_charList[e2]._Speaker != null)
				{
					_rightLabelContainer.SetActive(true);
					_rightSpeakerLabel.text = _charList[e2]._Speaker.GetComponent<CharacterPrefab>().Name;
				}
				else
				{
					_rightLabelContainer.SetActive(true);
					_rightSpeakerLabel.text = Hub.Instance.LoginManager.LocalUser.Name;
				}
			}
		}
		if (CurrentSpeaker >= 0 && _chars[0]._smAnim != null && _chars[1]._smAnim != null)
		{
			_chars[CurrentSpeaker].Color = Color.white;
			_chars[1 - CurrentSpeaker].Color = Color.grey;
		}
		if (_typewriter != null)
		{
			Object.Destroy(_typewriter);
		}
		_popupLabel.text = _popupListDialog[_currentStoryPopup];
		_typewriter = _popupLabel.gameObject.AddComponent<TypewriterEffect>();
		_typewriter.charsPerSecond = 150;
		yield return new WaitForEndOfFrame();
		StartCoroutine(WaitForText());
	}

	private void OnFinishCurrent(GameObject caller)
	{
		if (_typewriter != null && !_transitionBlocker.collider.enabled)
		{
			Object.Destroy(_typewriter);
			_popupLabel.text = _popupListDialog[_currentStoryPopup];
		}
	}

	private IEnumerator WaitForText()
	{
		while (_typewriter != null || (_chars[0]._smAnim == null && _chars[1]._smAnim == null))
		{
			yield return new WaitForEndOfFrame();
		}
		_Nextbtn.SetActive(true);
	}

	private IEnumerator SpawnBot(int index, string anim)
	{
		yield return new WaitForEndOfFrame();
		int index2 = default(int);
		string anim2 = default(string);
		_cacDisplayList[index].SpawnPlayer(() =>
		{
			_chars[index2]._smAnim = _cacDisplayList[index2].gameObject.GetComponentInChildren<BoneAnimation>();
			_chars[index2].Color = ((index2 != CurrentSpeaker) ? Color.grey : Color.white);
			_audioEmitter.Play(_audioWhooshIn);
			EBUI_TransitionManager.Instance.RunIntroTransitions(_cacDisplayList[index2].gameObject, SafeAction.Wrap(this, () =>
			{
				StartCoroutine(PlayAnim(anim2, index2));
			}));
		});
	}

	private IEnumerator PlayAnim(string anim, int index)
	{
		while (_chars[index]._smAnim == null)
		{
			yield return new WaitForEndOfFrame();
		}
		yield return new WaitForEndOfFrame();
		_chars[index]._smAnim.Play(anim, PlayMode.StopAll);
		_chars[index]._smAnim.updateColors = true;
		if (_chars[index]._smAnim[anim].wrapMode != WrapMode.Loop)
		{
			yield return new WaitForSeconds(_chars[index]._smAnim[anim].length);
			_chars[index]._smAnim.Play("bot_idle", PlayMode.StopAll);
		}
	}

	private void NextItem(GameObject caller)
	{
		if (_currentStoryPopup == _popupListDialog.Count - 1 || _popupListDialog.Count == 0)
		{
			ScreenManager.Instance.ClosePopup(base.gameObject.name);
			if (isWorldStory)
			{
				StoryManager.Instance.RunCinematic(StoryManager.Instance._currentChampionShipPrefab);
			}
			else
			{
				StoryManager.Instance.isStoryPlaying = false;
			}
		}
		else
		{
			_currentStoryPopup++;
			StartCoroutine(SetStory());
		}
	}

	private void OnClosePopup()
	{
		if (_chars[0]._smAnim != null && _chars[1]._smAnim != null)
		{
			_chars[0]._smAnim.SetMeshColor(Color.white);
			_chars[1]._smAnim.SetMeshColor(Color.white);
		}
	}

	private void Update()
	{
		_chars[0].Update();
		_chars[1].Update();
		if (Input.GetKeyDown(KeyCode.Escape) && !_transitionBlocker.collider.enabled)
		{
			if (_Nextbtn.activeSelf)
			{
				NextItem(null);
			}
			else
			{
				OnFinishCurrent(null);
			}
		}
	}
}
