using System.Collections.Generic;
using EB;
using SmoothMoves;
using UnityEngine;

public class ResultsPlayerUiItem : MonoBehaviour
{
	private BoneAnimation _smCharacter;

	private float _timer;

	private List<string> _anims;

	public float minIdleTime = 4f;

	public float maxIdleTime = 8f;

	public float HeightPointsOnly = 70f;

	public float HeightPointsAndKills = 132f;

	public string playerName
	{
		set
		{
			UIUtils.SetLabelContents(base.gameObject, "PlayerName", value);
		}
	}

	public string playerScore
	{
		set
		{
			UIUtils.SetLabelContents(base.gameObject, "PlayerScore", value);
		}
	}

	public string playerKills
	{
		set
		{
			UIUtils.SetLabelContents(base.gameObject, "PlayerKills", value);
		}
	}

	public uint playerId { get; set; }

	public int teamId
	{
		set
		{
		}
	}

	public bool showScoring
	{
		set
		{
			Util.GetObjectExactMatch(base.gameObject, "frame").SetActive(value);
			Util.GetObjectExactMatch(base.gameObject, "PlayerKills").SetActive(value);
			Util.GetObjectExactMatch(base.gameObject, "PlayerKillsTitle").SetActive(value);
			Util.GetObjectExactMatch(base.gameObject, "PlayerScore").SetActive(value);
			Util.GetObjectExactMatch(base.gameObject, "PlayerScoreTitle").SetActive(value);
		}
	}

	public bool isPresent
	{
		set
		{
			base.gameObject.SetActive(value);
		}
	}

	public bool showBothKillsAndPoints
	{
		set
		{
			GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "frame");
			float y = (value ? HeightPointsAndKills : HeightPointsOnly);
			objectExactMatch.transform.localScale = new Vector3(objectExactMatch.transform.localScale.x, y, objectExactMatch.transform.localScale.z);
			Util.GetObjectExactMatch(base.gameObject, "PlayerKills").SetActive(value);
			Util.GetObjectExactMatch(base.gameObject, "PlayerKillsTitle").SetActive(value);
		}
	}

	private void Awake()
	{
		_anims = new List<string>();
		showBothKillsAndPoints = false;
	}

	public void OnPlayerReady(string[] anims)
	{
		_smCharacter = base.gameObject.GetComponentInChildren<BoneAnimation>();
		_anims.AddRange(anims);
		if (_smCharacter != null && _anims.Count > 0)
		{
			_smCharacter.Play(_anims[0], PlayMode.StopAll);
		}
	}

	private void Update()
	{
		_smCharacter = base.gameObject.GetComponentInChildren<BoneAnimation>();
		if (!(_smCharacter == null) && _anims.Count > 0)
		{
			_smCharacter.Play(_anims[0], PlayMode.StopSameLayer);
		}
	}
}
