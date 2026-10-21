using System.Collections.Generic;
using EB;
using EB.Sparx;
using UnityEngine;

public class StoryManager : MonoBehaviour
{
	public bool isStoryPlaying;

	public Division.Stage _CurrentStageData;

	internal bool _isWorldIntro;

	internal bool _isStageIntro;

	internal bool _isWinOutro;

	internal bool _isLoseOutro;

	private int _stageKills;

	private int _stagePoints;

	internal int _division;

	private string prefabPath;

	internal ChampionshipPrefab _currentChampionShipPrefab;

	public static StoryManager Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
	}

	public void StartStory(Division.Stage stageData, int division)
	{
		_CurrentStageData = stageData;
		ChampionshipData.Instance.GetHighScore(division, _CurrentStageData, out _stagePoints, out _stageKills);
		_currentChampionShipPrefab = currentChampionShipPrefab(division, stageData);
		_division = division;
		if (_stageKills == 0 && _stagePoints == 0 && _CurrentStageData.worldStory.storycharacters.Count > 0)
		{
			isStoryPlaying = true;
			_isWorldIntro = true;
			ScreenManager.Instance.OpenPopup("StoryPopupUI", true);
		}
		else
		{
			RunCinematic(_currentChampionShipPrefab);
		}
	}

	public void RunCinematic(ChampionshipPrefab prefab)
	{
		if (_currentChampionShipPrefab.Cinematic.Length > 0 && _stageKills == 0 && _stagePoints == 0)
		{
			isStoryPlaying = true;
			CampaignCinematicItem._items = new List<CampaignCinematic>(prefab.Cinematic);
			ScreenManager.Instance.OpenPopup("CampaignCinematicUI");
		}
		else
		{
			isStoryPlaying = false;
		}
	}

	public void IntroStory()
	{
		if (GameAttributes.GetChampionshipMode(GameAttributes.GetValue(GameAttributes.AttributeType.Championship)) != GameAttributes.ChampionshipType.kOff && _CurrentStageData._level != null && _stageKills == 0 && _stagePoints == 0 && _CurrentStageData.introStory.storycharacters.Count > 0)
		{
			_isStageIntro = true;
			isStoryPlaying = true;
			ScreenManager.Instance.OpenPopup("StoryPopupUI", true);
		}
	}

	public void OutroStory()
	{
		if (GameAttributes.GetChampionshipMode(GameAttributes.GetValue(GameAttributes.AttributeType.Championship)) == GameAttributes.ChampionshipType.kOff)
		{
			return;
		}
		bool flag = false;
		if (_CurrentStageData._level != null && _stageKills == 0 && _stagePoints == 0)
		{
			GameAttributes.GameModeType gameMode = GameAttributes.GetGameMode(GameAttributes.GetValue(GameAttributes.AttributeType.GameMode));
			if (gameMode != GameAttributes.GameModeType.TeamDeathMatch)
			{
				if (Scoring.Instance.GetRankByPlayer(Hub.Instance.GameManager.Game.LocalPlayer.PlayerId) == 1 && _CurrentStageData.outroStory.win.storycharacters.Count > 0)
				{
					_isWinOutro = true;
					isStoryPlaying = true;
					flag = true;
				}
				else if (_CurrentStageData.outroStory.lose.storycharacters.Count > 0)
				{
					flag = true;
					isStoryPlaying = true;
					_isLoseOutro = true;
				}
			}
			else if (Scoring.Instance.GetRankByTeam(Scoring.Instance.GetTeamId(Hub.Instance.GameManager.Game.LocalPlayer.PlayerId)) == 1 && _CurrentStageData.outroStory.win.storycharacters.Count > 0)
			{
				_isWinOutro = true;
				isStoryPlaying = true;
				flag = true;
			}
			else if (_CurrentStageData.outroStory.lose.storycharacters.Count > 0)
			{
				isStoryPlaying = true;
				_isLoseOutro = true;
				flag = true;
			}
		}
		if (flag)
		{
			ScreenManager.Instance.OpenPopup("StoryPopupUI", true);
		}
	}

	private ChampionshipPrefab currentChampionShipPrefab(int div, Division.Stage stage)
	{
		switch (div)
		{
		case 0:
			prefabPath = "prefabs/Div_01_Bronze/";
			break;
		case 1:
			prefabPath = "prefabs/Div_02_Silver/";
			break;
		case 2:
			prefabPath = "prefabs/Div_03_Gold/";
			break;
		default:
			EB.Debug.LogError("invalid division specified: " + div + ", cannot load prefab '" + stage._championshipPrefab + "' for stage " + stage._level);
			break;
		}
		GameObject gameObject = Assets.Load(prefabPath + stage._championshipPrefab) as GameObject;
		if (null != gameObject)
		{
			return gameObject.GetComponent<ChampionshipPrefab>();
		}
		return null;
	}
}
