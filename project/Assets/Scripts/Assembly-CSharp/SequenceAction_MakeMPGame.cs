using System.Collections;
using EB;
using EB.Sequence;
using EB.Sequence.Runtime;
using EB.Sparx;
using UnityEngine;

[MenuItem(Path = "Actions/MakeMPGame")]
public class SequenceAction_MakeMPGame : EB.Sequence.Runtime.Action
{
	[Property]
	public string spPrefabName;

	[Property]
	public int countdownTime = 10;

	[Trigger]
	public Trigger Finished = new Trigger();

	private Game _game;

	private SinglePlayerPrefab _singlePlayerPrefab;

	private ArrayList _playersToJoin = new ArrayList();

	[Entry]
	public void Process()
	{
		if (!string.IsNullOrEmpty(spPrefabName))
		{
			GameObject gameObject = Assets.Load(string.Format("{0}", spPrefabName), typeof(GameObject)) as GameObject;
			_singlePlayerPrefab = gameObject.GetComponent<SinglePlayerPrefab>();
			GameAttributes.GameModeType gameModeType = GameAttributes.GameModeType.DeathMatch;
			GameManager gameManager = Hub.Instance.GameManager;
			GameManager.LocalCreateGameParams pars = new GameManager.LocalCreateGameParams
			{
				Attributes = GameAttributes.CreateDefault(GameAttributes.ModeType.MP)
			};
			GameHUD._settings._eVisFlags = GameHUD.Settings.Visibility.kBoosts;
			GameAttributes.SetAttribute(GameAttributes.AttributeType.GameMode, gameModeType, pars.Attributes);
			GameAttributes.SetAttribute(GameAttributes.AttributeType.Level, _singlePlayerPrefab.Level, pars.Attributes);
			GameAttributes.SetAttribute(GameAttributes.AttributeType.AuxFlags, GameAttributes.AuxFlagsType.kTutorialOverride, pars.Attributes);
			pars.PlayerAttributes = Misc.CreatePlayerAttributes();
			pars.CampaignAttributes = CreateCampaignAttributes();
			_game = gameManager.CreateLocalGame(pars);
			GameObject gameObject2 = ScreenManager.Instance.OpenPopup("LobbyUI");
			LobbyUI component = gameObject2.GetComponent<LobbyUI>();
			if (component != null)
			{
				component.SetCountdownTimerInterval(countdownTime);
			}
			AISimple.SetLoadout(_singlePlayerPrefab.Inventory);
		}
		else
		{
			EB.Debug.LogError("Could not create the faux multiplayer game");
			Finished.Invoke();
		}
	}

	[Entry]
	public void AddPlayer()
	{
		if (_playersToJoin.Count < _singlePlayerPrefab.Players.Length)
		{
			CharacterPrefab characterPrefab = _singlePlayerPrefab.Players[_playersToJoin.Count];
			string spawnPointName = string.Empty;
			if (_playersToJoin.Count < _singlePlayerPrefab.AiSpawnPoints.Length)
			{
				spawnPointName = _singlePlayerPrefab.AiSpawnPoints[_playersToJoin.Count];
			}
			Hashtable hashtable = new Hashtable();
			hashtable["uid"] = Hub.Instance.LoginManager.LocalUserId;
			hashtable["id"] = (long)_playersToJoin.Count + 10L;
			hashtable["attributes"] = characterPrefab.CreatePlayerAttributes(spawnPointName);
			_playersToJoin.Add(hashtable);
			_game.SendGameCommand(0u, "ai", _playersToJoin);
		}
		if (_playersToJoin.Count >= _singlePlayerPrefab.Players.Length)
		{
			Finished.Invoke();
		}
	}

	private Hashtable CreateCampaignAttributes()
	{
		Hashtable hashtable = new Hashtable();
		hashtable["sp_prefab"] = spPrefabName;
		return hashtable;
	}
}
