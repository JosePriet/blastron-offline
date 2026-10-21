using System.Collections;
using EB;
using EB.Replication;
using EB.Sequence;
using EB.Sparx;
using UnityEngine;

public class GameListener : MonoBehaviour, EB.Sparx.GameListener
{
	public void Awake()
	{
		base.useGUILayout = false;
		EB.Replication.Manager.RegisterSerializable(typeof(Misc.HitInfo));
	}

	public void Update()
	{
		EB.Replication.Manager.Update();
	}

	public void OnJoinedGame(Game game)
	{
		EB.Debug.Log("OnJoinedGame");
		EB.Replication.Manager.SetGame(game);
		ScreenManager.Instance.CloseAllPopups();
		if (!game.Network.IsLocal)
		{
			ScreenManager.Instance.ClearPersistentScreens();
			ScreenManager.Instance.OpenPopup("LobbyUI");
			ScreenManager.Instance.AddPersistentScreen("BackgroundUI");
			Hub.Instance.InventoryManager.Sync();
		}
		Automation.Events.Emit("joingame");
	}

	public void OnGameEnded(Game game)
	{
		EB.Debug.Log("OnGameEnded");
		uint num = (uint)Random.Range(int.MinValue, int.MaxValue);
		Hashtable attributes = new Hashtable();
		GameAttributes.SetAttribute(GameAttributes.AttributeType.Order, num.ToString(), attributes);
		if (!game.IsPrivate)
		{
			string[] mP_Levels = Setup.Instance.MP_Levels;
			string value = mP_Levels[Random.Range(0, mP_Levels.Length)];
			GameAttributes.SetAttribute(GameAttributes.AttributeType.Level, value, attributes);
		}
		game.SetAttributes(attributes);
		StartCoroutine(ReturnToLobby());
		Automation.Events.Emit("gameended");
	}

	private IEnumerator ReturnToLobby()
	{
		EB.Debug.Log("ReturnToLobby!!!!");
		yield return Setup.Instance.OnLeaveGame();
		ScreenManager.Instance.AddPersistentScreen("BackgroundUI");
		ScreenManager.Instance.OpenPopup("LobbyUI");
	}

	public void OnLeaveGame(Game game, string reason)
	{
		EB.Debug.Log("OnLeaveGame");
		EB.Replication.Manager.ClearGame();
		StopAllCoroutines();
		Input.multiTouchEnabled = false;
		if (!BugReport.DidCrash)
		{
			StartCoroutine(_HandleLeaveGame(reason));
		}
	}

	private IEnumerator _HandleLeaveGame(string reason)
	{
		EB.Debug.Log("HANDLE LEAVE GAME REASON " + reason);
		if (!string.IsNullOrEmpty(reason) && !ScreenManager.Instance.IsLoadingScreenUp)
		{
			bool done = false;
			ScreenManager.Instance.ShowError(reason, () =>
			{
				done = true;
			});
			while (!done)
			{
				yield return 1;
			}
		}
		EB.Debug.Log("HANDLE LEAVE GAME 2");
		ScreenManager.Instance.EnableTransitionBlocker(true);
		ScreenManager.Instance.SetCameraClearFlags(CameraClearFlags.Color, Color.black);
		while (EB.Replication.Manager.IsLoadingLevel)
		{
			EB.Debug.Log("waiting for the level to be loaded");
			yield return 1;
		}
		yield return new WaitForFixedUpdate();
		Hub.Instance.InventoryManager.ClearOverride();
		Hub.Instance.InventoryManager.SinglePlayerClearUsedInventory();
		if (CrateManager.Instance != null)
		{
			CrateManager.Instance.UseDefaultSpawnData();
		}
		Setup.Instance.OnLeaveGame();
	}

	public void OnPlayerJoined(Game game, EB.Sparx.Player player)
	{
		EB.Debug.Log("OnPlayerJoined " + player.UserId);
		ScreenManager.Instance.BroadcastMessage("OnPlayerJoined", player, SendMessageOptions.DontRequireReceiver);
	}

	public void OnPlayerLeft(Game game, EB.Sparx.Player player)
	{
		EB.Debug.LogWarning(string.Format("OnPlayerLeft {0} game started : {1}", player.UserId, game.Started));
		EB.Replication.Manager.OnPlayerLeft(game, player);
		ScreenManager.Instance.BroadcastMessage("OnPlayerLeft", player, SendMessageOptions.DontRequireReceiver);
		EB.Sequence.Component.Activate(null, player.PlayerId, typeof(SequenceEvent_PlayerLeft));
		StartCoroutine(_DoTeamCheck());
	}

	private IEnumerator _DoTeamCheck()
	{
		while (LevelData.Instance == null || !LevelData.Instance.IsReady)
		{
			yield return 1;
		}
		if ((bool)Scoring.Instance && !Scoring.Instance.CheckTeams())
		{
			EB.Debug.LogWarning("Min number of teams failed");
			EB.Sequence.Component.Activate(null, null, typeof(SequenceEvent_TeamCheckFailed));
			StopAllCoroutines();
		}
	}

	public void OnAttributesUpdated(Game game)
	{
		EB.Debug.Log("OnAttributesUpdated ");
		ScreenManager.Instance.BroadcastMessage("OnAttributesUpdated", SendMessageOptions.DontRequireReceiver);
	}

	public void OnReceive(Game game, EB.Sparx.Player player, Packet packet)
	{
		EB.Replication.Manager.Receive((player != null) ? player.PlayerId : 0u, packet.Data);
	}

	public void OnGameStarted(Game game)
	{
		ScreenManager.Instance.FadeIn(() =>
		{
			Input.multiTouchEnabled = true;
			EB.Debug.Log("OnGameStarted");
			if (EB.Replication.Manager.IsHost)
			{
				string value = GameAttributes.GetValue(GameAttributes.AttributeType.Level, game.Attributes);
				EB.Replication.Manager.LoadLevel(value);
			}
			Setup.Instance.OnStartGame();
			ScreenManager.Instance.CloseAllPopups();
			ScreenManager.Instance.ClearPersistentScreens();
			ScreenManager.Instance.ShowLoadingScreen("LoadingScreenTipsUI");
			ScreenManager.Instance.EnableMultiTouch(true);
			StatManager.Instance.IncrementStat(StatManager.Stat.GamesPlayed);
			GameAttributes.ModeType mode = GameAttributes.GetMode(GameAttributes.GetValue(GameAttributes.AttributeType.Mode));
			if (mode == GameAttributes.ModeType.SP)
			{
				Telemetry.Event("sp_start/" + GameAttributes.GetValue(GameAttributes.AttributeType.CampaignData));
			}
			else
			{
				Telemetry.Event("mp_start/" + GameAttributes.GetValue(GameAttributes.AttributeType.GameMode));
				Telemetry.Event("mp_level/" + GameAttributes.GetValue(GameAttributes.AttributeType.Level));
			}
			Telemetry.Flush();
			Automation.Events.Emit("gamestarted");
		});
	}

	public void OnJoinGameFailed(string err)
	{
		EB.Debug.Log("ON JOIN GAME FAILED!");
		Automation.Events.Emit("joinfailed", err);
		ScreenManager.Instance.ShowError(err, () =>
		{
			Setup.Instance.ReturnToFrontendScreen();
		});
	}
}
