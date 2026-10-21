using System;
using System.Collections.Generic;
using EB;
using UnityEngine;

[EB.Serializable(Tag = "div")]
public class Division : MonoBehaviour
{
	[System.Serializable]
	[EB.Serializable(Tag = "stg")]
	public class Stage
	{
		[EB.Serializable(Tag = "lvl")]
		public string _level;

		[EB.Serializable(Tag = "gm")]
		public GameAttributes.GameModeType _mode;

		[EB.Serializable(Tag = "gt")]
		public int gameTime;

		[EB.Serializable(Tag = "pf")]
		public string _championshipPrefab;

		[EB.Serializable(Tag = "ftr")]
		public ChampionshipPrefab.Reward[] _firstTimeRewards = new ChampionshipPrefab.Reward[0];

		[EB.Serializable(Tag = "rgr")]
		public ChampionshipPrefab.Reward[] _regularRewards = new ChampionshipPrefab.Reward[0];

		[EB.Serializable(Tag = "ftro")]
		public ChampionshipPrefab.Reward _firstTimeRewardUiOverride;

		[EB.Serializable(Tag = "rgro")]
		public ChampionshipPrefab.Reward _regularRewardUiOverride;

		[EB.Serializable(Tag = "sst")]
		public int _silverScoreThreshold;

		[EB.Serializable(Tag = "gst")]
		public int _goldScoreThreshold;

		[EB.Serializable(Tag = "a0")]
		public ChampionshipPrefab.CompletionAward _bronzeAward;

		[EB.Serializable(Tag = "a1")]
		public ChampionshipPrefab.CompletionAward _silverAward;

		[EB.Serializable(Tag = "a2")]
		public ChampionshipPrefab.CompletionAward _goldAward;

		[NoSerialize]
		public StoryBuilder worldStory;

		[NoSerialize]
		public StoryBuilder introStory;

		[NoSerialize]
		public OutroStoryCharacters outroStory;

		public void GetAwards(int points, int kills, out int rivets, out int xp)
		{
			int num = ((_mode != GameAttributes.GameModeType.KillCount) ? points : kills);
			if (num >= _goldScoreThreshold)
			{
				xp = _goldAward.XP;
				rivets = _goldAward.Rivets;
			}
			else if (num >= _silverScoreThreshold)
			{
				xp = _silverAward.XP;
				rivets = _silverAward.Rivets;
			}
			else
			{
				xp = _bronzeAward.XP;
				rivets = _bronzeAward.Rivets;
			}
		}

		public int GetStarsFor(int points, int kills)
		{
			int num = ((_mode != GameAttributes.GameModeType.KillCount) ? points : kills);
			if (num >= _goldScoreThreshold)
			{
				return 2;
			}
			if (num >= _silverScoreThreshold)
			{
				return 1;
			}
			return 0;
		}

		public bool HasFirstTimeRewardUiOverride()
		{
			return _firstTimeRewardUiOverride != null && !string.IsNullOrEmpty(_firstTimeRewardUiOverride.assetName) && _firstTimeRewardUiOverride.quantity > 0;
		}

		public bool HasRegularRewardUiOverride()
		{
			return _regularRewardUiOverride != null && !string.IsNullOrEmpty(_regularRewardUiOverride.assetName) && _regularRewardUiOverride.quantity > 0;
		}
	}

	[System.Serializable]
	public class StoryBuilder
	{
		public List<StoryCharacters> storycharacters = new List<StoryCharacters>();
	}

	[System.Serializable]
	public class StoryCharacters
	{
		public enum speakerSide
		{
			Left = 0,
			Right = 1
		}

		public speakerSide side;

		public GameObject _Speaker;

		public string[] animationForBots = new string[0];
	}

	[System.Serializable]
	public class OutroStoryCharacters
	{
		[System.Serializable]
		public class state
		{
			public List<StoryCharacters> storycharacters = new List<StoryCharacters>();
		}

		public state win;

		public state lose;
	}

	[EB.Serializable(Tag = "st")]
	public Stage[] _stages;

	public int FindIndexForStage(Stage stage)
	{
		return Array.FindIndex(_stages, (Stage obj) => obj.Equals(stage));
	}
}
