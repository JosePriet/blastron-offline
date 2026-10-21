using System.Collections.Generic;
using EB;
using EB.Replication;
using UnityEngine;

public class EmoteManager : MonoBehaviour
{
	private static EmoteManager _instance;

	private string emoteBase = "ui_emote_base";

	private GameObject emotePrefab;

	private UIAtlas emoteAtlas;

	private List<string> _spriteNames;

	private List<GameObject> _emoteInstances;

	public static EmoteManager Instance
	{
		get
		{
			return _instance;
		}
	}

	public string[] Emotes
	{
		get
		{
			if (_spriteNames != null)
			{
				return _spriteNames.ToArray();
			}
			return null;
		}
	}

	private void Awake()
	{
		_instance = this;
	}

	private void Start()
	{
	}

	public void SpawnEmote(string name)
	{
		uint playerId = Manager.LocalPlayer.PlayerId;
		Manager.RPC("CreateEmote", RPCMode.AllBuffered, playerId, name);
	}

	[RPC]
	private void CreateEmote(uint playerId, string name)
	{
		Player player = LevelData.Instance.GetPlayer(playerId);
		GameObject playerObject = player.gameObject;
		GameObject gameObject = Object.Instantiate(emotePrefab) as GameObject;
		GameObject gameObject2 = Util.GetObject(ScreenManager.Instance.gameObject, "Panel");
		gameObject.transform.parent = gameObject2.transform;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localScale = Vector3.one;
		Emote component = gameObject.GetComponent<Emote>();
		component.Init(playerObject, name);
		if (_emoteInstances.Count > 0)
		{
			foreach (GameObject emoteInstance in _emoteInstances)
			{
				Object.Destroy(emoteInstance);
			}
		}
		_emoteInstances.Add(gameObject);
	}

	public void OnEmoteDestroyed(GameObject emoteObj)
	{
		_emoteInstances.Remove(emoteObj);
	}
}
