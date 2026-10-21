using System.Collections;
using EB;
using EB.Sparx;
using UnityEngine;

public class Automation : AIBase
{
	public static EventEmitter Events = new EventEmitter();

	public static Automation Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
		Object.DontDestroyOnLoad(base.gameObject);
		CreateStates();
	}

	private new void SetState(string name)
	{
		Events.Clear();
		base.SetState(name);
	}

	private void Start()
	{
		SetState("idle");
	}

	private void CreateStates()
	{
		AddState("idle", "_Idle");
		AddState("login", "_Login");
		AddState("search", "_Search");
		AddState("lobby", "_Lobby");
		AddState("attach", "_Attach");
		AddState("ingame", "_InGame");
		AddState("cooldown", "_CoolDown");
	}

	private IEnumerator _Idle()
	{
		while (Setup.Instance == null)
		{
			yield return new WaitForSeconds(2f);
		}
		if (Setup.Instance.IsLoggedIn)
		{
			SetState("search");
		}
		else
		{
			SetState("login");
		}
	}

	private IEnumerator _Login()
	{
		Setup.Instance.ReturnToFrontendScreen();
		yield return 1;
		Setup.Instance.Login(false);
		while (!Setup.Instance.IsLoggedIn)
		{
			yield return 1;
		}
		SetState("idle");
	}

	private IEnumerator _Search()
	{
		bool didSearch = false;
		while (Setup.Instance.IsLoggedIn)
		{
			if (!didSearch)
			{
				didSearch = true;
				UnityEngine.Debug.Log("Searching for games...");
				GameManager.FindGameParams pars = default(GameManager.FindGameParams);
				pars.PlayerAttributes = Misc.CreatePlayerAttributes();
				pars.PlayerAttributes["level"] = 2;
				pars.SearchAttributes = new Hashtable();
				Hub.Instance.GameManager.FindGame(pars);
				Events.Clear();
				Events.On("joinfailed", (string err) =>
				{
					UnityEngine.Debug.Log("join failed:" + err);
					Coroutines.SetTimeout(() =>
					{
						didSearch = false;
					}, 5000);
				});
				Events.On("joingame", () =>
				{
					SetState("lobby");
				});
			}
			yield return 1;
		}
		SetState("idle");
	}

	private IEnumerator _Lobby()
	{
		UnityEngine.Debug.LogError("In Lobby");
		Events.On("gamestarted", () =>
		{
			SetState("attach");
		});
		while (Hub.Instance.GameManager.Game != null && Hub.Instance.GameManager.Game.Players.Count > 1)
		{
			yield return 1;
		}
		SetState("cooldown");
	}

	private IEnumerator _Attach()
	{
		UnityEngine.Debug.LogError("Attaching");
		while (Hub.Instance.GameManager.Game != null)
		{
			if (LevelData.Instance != null)
			{
				GameObject player = LevelData.Instance.GetPlayerObject();
				if (player != null)
				{
					AISimple tempAI = player.AddComponent<AISimple>();
					tempAI.IsBot = true;
					tempAI.OrderBotArsenal();
					player.GetComponent<PlayerControlByGUI>().enabled = false;
					SetState("ingame");
					yield break;
				}
			}
			yield return 1;
		}
		SetState("idle");
	}

	private IEnumerator _InGame()
	{
		UnityEngine.Debug.LogError("InGame");
		Events.On("gameended", () =>
		{
			SetState("cooldown");
		});
		while (Hub.Instance.GameManager.Game != null)
		{
			yield return 1;
		}
		SetState("cooldown");
	}

	private IEnumerator _CoolDown()
	{
		Hub.Instance.GameManager.LeaveGame();
		yield return new WaitForSeconds(30f);
		SetState("idle");
	}
}
