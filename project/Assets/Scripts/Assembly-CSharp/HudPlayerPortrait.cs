using System;
using System.Collections;
using EB;
using UnityEngine;

public class HudPlayerPortrait : MonoBehaviour
{
	private string _turnCircleTexName = string.Empty;

	private string _crownTexName = string.Empty;

	private UILabel _name;

	private UILabel _score;

	private UILabel _rank;

	private UISprite _active;

	private UISprite _activeBlock;

	private UISprite _rankBackground;

	private UISprite _deadIcon;

	private GameObject _portraitBackground;

	private UISprite[] _boostSprites;

	private UITexture _portraitTexture;

	private string _portraitTexturePath = string.Empty;

	private bool _current;

	private float _throbDuration = 2f;

	private float _amplitude = 0.05f;

	private Vector3 _restScale;

	private float _oscillations = 3f;

	private bool _throbbing;

	private float _startThrobTime;

	public string PortraitTexturePath
	{
		get
		{
			return _portraitTexturePath;
		}
		set
		{
			if (!(_portraitTexture == null))
			{
				TexturePoolManager.Instance.LoadTexture(value, SafeAction.Wrap<Texture2D>(this, OnTextureLoaded));
				_portraitTexturePath = value;
			}
		}
	}

	public uint PlayerId { get; set; }

	public string Name
	{
		set
		{
			if (!(_name == null))
			{
				UIUtils.TruncateWithEllipsis(_name, value);
			}
		}
	}

	public int TeamId
	{
		set
		{
			Color color = Tuning.Instance.TeamColors[value];
			_active.color = color;
			_activeBlock.color = color;
		}
	}

	public int Score
	{
		set
		{
			if (!(_score == null))
			{
				_score.text = value.ToString();
			}
		}
	}

	public int Rank
	{
		set
		{
			if (!(_rank == null) && !(_rankBackground == null))
			{
				if (value == 1)
				{
					_rank.text = string.Empty;
					_rankBackground.gameObject.SetActive(true);
					_rankBackground.spriteName = _crownTexName;
					_rankBackground.MakePixelPerfect();
				}
				else
				{
					_rank.text = value.ToString();
					_rankBackground.gameObject.SetActive(false);
				}
			}
		}
	}

	public void Initialize(string turnCircle, string crown)
	{
		_turnCircleTexName = turnCircle;
		_crownTexName = crown;
		_name = Util.GetObject(base.gameObject, "Portrait_player_name", true).GetComponent<UILabel>();
		_score = Util.GetObject(base.gameObject, "Portrait_score", true).GetComponent<UILabel>();
		_rank = Util.GetObject(base.gameObject, "Portrait_rank", true).GetComponent<UILabel>();
		_portraitTexture = Util.GetObject(base.gameObject, "Portrait", true).GetComponent<UITexture>();
		_active = Util.GetObject(base.gameObject, "Portrait_Active", true).GetComponent<UISprite>();
		_activeBlock = Util.GetObjectExactMatch(base.gameObject, "Portrait_Active_Block").GetComponent<UISprite>();
		_rankBackground = Util.GetObject(base.gameObject, "Portrait_circle", true).GetComponent<UISprite>();
		_deadIcon = Util.GetObject(base.gameObject, "Portrait_X", true).GetComponent<UISprite>();
		_deadIcon.gameObject.SetActive(false);
		_portraitBackground = Util.GetObjectExactMatch(base.gameObject, "Portrait_Background");
		_portraitBackground.SetActive(false);
		GameObject[] objects = Util.GetObjects(base.gameObject, "boost");
		Array.Sort(objects, (GameObject g1, GameObject g2) => g1.name.CompareTo(g2.name));
		_boostSprites = new UISprite[objects.Length];
		for (int num = 0; num < objects.Length; num++)
		{
			_boostSprites[num] = objects[num].GetComponent<UISprite>();
		}
		_restScale = base.transform.localScale;
	}

	public void HideRank()
	{
		GameObject gameObject = Util.GetObject(base.gameObject, "Portrait_rank", true);
		if (gameObject != null)
		{
			gameObject.SetActive(false);
		}
		GameObject gameObject2 = Util.GetObject(base.gameObject, "Portrait_circle", true);
		if (gameObject2 != null)
		{
			gameObject2.SetActive(false);
		}
	}

	public void HideScore()
	{
		GameObject gameObject = Util.GetObject(base.gameObject, "Portrait_score", true);
		if (gameObject != null)
		{
			gameObject.SetActive(false);
		}
	}

	public void MarkDead()
	{
		_deadIcon.gameObject.SetActive(true);
	}

	public void MarkLeft()
	{
		UISprite[] componentsInChildren = base.gameObject.GetComponentsInChildren<UISprite>();
		UISprite[] array = componentsInChildren;
		foreach (UISprite uISprite in array)
		{
			uISprite.alpha = 0.3f;
		}
		UITexture[] componentsInChildren2 = base.gameObject.GetComponentsInChildren<UITexture>();
		UITexture[] array2 = componentsInChildren2;
		foreach (UITexture uITexture in array2)
		{
			uITexture.alpha = 0.3f;
		}
	}

	private void OnTextureLoaded(Texture2D text)
	{
		_portraitTexture.mainTexture = text;
	}

	private void OnClick()
	{
		Player player = LevelData.Instance.GetPlayer(PlayerId);
		if (player != null)
		{
			CameraController component = Camera.main.GetComponent<CameraController>();
			if (component != null)
			{
				component.FollowPlayer(player.gameObject);
			}
		}
	}

	public void ToggleCurrentPlayer(bool isCurrentPlayer)
	{
		_current = isCurrentPlayer;
		_portraitBackground.gameObject.SetActive(_current);
	}

	public void OnScoringUpdated()
	{
		Rank = Scoring.Instance.GetRankByPlayer(PlayerId);
		Score = Scoring.Instance.GetScoreByPlayer(PlayerId);
		if (Scoring.Instance.GetIsDead(PlayerId))
		{
			MarkDead();
		}
	}

	public void OnActiveBoostsUpdated(Hashtable boostData)
	{
		if (!boostData.ContainsKey("playerId"))
		{
			return;
		}
		uint num = (uint)boostData["playerId"];
		if (num == PlayerId)
		{
			int num2 = (int)boostData["slot"];
			BoostItem.Type type = (BoostItem.Type)(int)boostData["type"];
			string text = BoostItem.GetHudIcon(type, true);
			float alpha = 1f;
			if (text == null)
			{
				text = "white_square";
				alpha = 0.5f;
			}
			_boostSprites[num2].spriteName = text;
			_boostSprites[num2].alpha = alpha;
			_boostSprites[num2].MakePixelPerfect();
		}
	}

	public void SetAsPlaceholder()
	{
		ToggleCurrentPlayer(false);
		Vector3 localPosition = new Vector3(_name.transform.localPosition.x, _name.transform.localPosition.y + 1000f, _name.transform.localPosition.z);
		_name.transform.localPosition = localPosition;
		_score.transform.localPosition = localPosition;
		_rank.transform.localPosition = localPosition;
		_name.text = string.Empty;
		_score.text = string.Empty;
		_rank.text = string.Empty;
	}

	private void StartThrob()
	{
		_throbbing = true;
		_startThrobTime = UnityEngine.Time.time;
	}

	private void Update()
	{
		if (_throbbing)
		{
			Throb();
		}
	}

	private void Throb()
	{
		float num = UnityEngine.Time.time - _startThrobTime;
		float num2 = 1f + _amplitude * (Mathf.Sin(_oscillations * num * (float)Math.PI * 2f) * Misc.H0(num / _throbDuration));
		base.transform.localScale = _restScale * num2;
		if (num > _throbDuration)
		{
			_throbbing = false;
			base.transform.localScale = _restScale;
		}
	}
}
