using System.Collections;
using EB;
using UnityEngine;

public class Healthbar : MonoBehaviour
{
	private class HealthLabels
	{
		public GameObject[] _labels;

		public int _next;

		public HealthLabels(GameObject template, int count, Transform parent)
		{
			_labels = new GameObject[count];
			for (int i = 0; i < count; i++)
			{
				_labels[i] = (GameObject)Object.Instantiate(template);
				_labels[i].SetActive(false);
				Util.Parent(parent.gameObject, _labels[i]);
			}
		}

		public GameObject Next()
		{
			GameObject gameObject = _labels[_next];
			gameObject.SetActive(true);
			_next = (_next + 1) % _labels.Length;
			return gameObject;
		}

		public void Disable()
		{
			for (int i = 0; i < _labels.Length; i++)
			{
				if (_labels[i] != null)
				{
					_labels[i].SetActive(false);
				}
			}
		}

		public void Destroy()
		{
			GameObject[] labels = _labels;
			foreach (GameObject obj in labels)
			{
				Object.Destroy(obj);
			}
		}
	}

	private Camera uiCamera;

	private CameraController cameraController;

	private GameObject player;

	private Health health;

	private UISlider progressMeter;

	private UISlider shieldMeter;

	private UILabel _nameLabel;

	private UILabel _powerLabel;

	private UILabel _aimLabel;

	private string _playerName;

	private Vector3 _keyScaleMax;

	private GameObject keyObj;

	private GameObject labelsObj;

	private GameObject barObj;

	private GameObject shieldObj;

	private bool _isShowing;

	private bool _gameHUDVisible = true;

	private HealthLabels _damageLabels;

	private HealthLabels _healLabels;

	private int _damageLabelID;

	private int _healLabelID;

	public bool IsShowing
	{
		get
		{
			return _isShowing;
		}
	}

	private void Awake()
	{
		barObj = Util.GetObjectExactMatch(base.gameObject, "Health");
		shieldObj = Util.GetObjectExactMatch(base.gameObject, "Shield");
		progressMeter = barObj.GetComponentInChildren<UISlider>();
		shieldMeter = shieldObj.GetComponentInChildren<UISlider>();
		labelsObj = Util.GetObjectExactMatch(base.gameObject, "LABELS");
	}

	public void RegisterPlayerObject(GameObject go, string playerName, int teamId)
	{
		player = go;
		_playerName = playerName;
		health = player.GetComponent<Health>();
		health.RegisterHealthbar(base.gameObject);
		Color color = Tuning.Instance.TeamColors[teamId];
		keyObj = Util.GetObject(base.gameObject, "Key");
		_keyScaleMax = keyObj.transform.localScale;
		_nameLabel = Util.GetObject(base.gameObject, "Name", true).GetComponent<UILabel>();
		_powerLabel = Util.GetObject(base.gameObject, "Power", true).GetComponent<UILabel>();
		_aimLabel = Util.GetObject(base.gameObject, "Aim", true).GetComponent<UILabel>();
		progressMeter.GetComponentInChildren<UIFilledSprite>().color = color;
		shieldMeter.GetComponentInChildren<UIFilledSprite>().color = Color.blue;
		InitHealthLabels();
		if (GameHUD.Instance != null)
		{
			OnGameHUDVisible(GameHUD.Instance.Visible);
		}
	}

	private void InitHealthLabels()
	{
		GameObject gameObject = Util.GetObject(ScreenManager.Instance.gameObject, "Panel");
		_damageLabelID = HUDLabels.Instance.Register(Assets.Load("UI/DamageTaken") as GameObject, gameObject.transform, 5);
		_healLabelID = HUDLabels.Instance.Register(Assets.Load("UI/HealReceived") as GameObject, gameObject.transform, 5);
	}

	public void OnDamage(Misc.DamageInfo dmgInfo)
	{
		if (!GameAttributes.IsAuxFlagSet(GameAttributes.AuxFlagsType.kDetonationVis))
		{
			HUDLabels.Instance.Next(_damageLabelID, AnimateDamage, "-" + (int)dmgInfo.damage, base.transform, Vector3.zero);
		}
	}

	public void OnHeal(float heal)
	{
		if (!GameAttributes.IsAuxFlagSet(GameAttributes.AuxFlagsType.kDetonationVis))
		{
			HUDLabels.Instance.Next(_healLabelID, AnimateDamage, ((int)heal).ToString(), base.transform, Vector3.zero);
		}
	}

	private IEnumerator AnimateDamage(GameObject go, Transform relativeTo, Vector3 offset)
	{
		float time = 2f;
		Vector3 speed = new Vector3(0f, 50f, 0f);
		Vector3 delta = Vector3.zero;
		while (time > 0f)
		{
			delta += speed * UnityEngine.Time.deltaTime;
			go.transform.localPosition = relativeTo.localPosition + Tuning.Instance.DamageTextOffset + delta;
			time -= UnityEngine.Time.deltaTime;
			yield return 1;
		}
		go.SetActive(false);
	}

	private Vector3 WorldToScreen(Vector3 pos)
	{
		Vector3 position = Camera.main.WorldToViewportPoint(pos);
		return uiCamera.ViewportToWorldPoint(position);
	}

	private void WorldToScreen(Transform t, Vector3 pos)
	{
		t.position = WorldToScreen(pos);
		Vector3 localPosition = new Vector3((int)t.localPosition.x, (int)t.localPosition.y, 50f);
		t.localPosition = localPosition;
	}

	private void Update()
	{
		if (uiCamera == null)
		{
			uiCamera = NGUITools.FindCameraForLayer(base.gameObject.layer);
		}
		if (cameraController == null)
		{
			cameraController = Camera.main.GetComponent<CameraController>();
		}
		if (!(player != null))
		{
			return;
		}
		Vector3 position = player.transform.position;
		WorldToScreen(base.transform, position);
		float maxHP = health.MaxHP;
		float hitPoints = health._hitPoints;
		progressMeter.sliderValue = hitPoints / maxHP;
		Player component = player.GetComponent<Player>();
		if (!component)
		{
			return;
		}
		if (component.HasKey)
		{
			if (!keyObj.active)
			{
				keyObj.SetActive(true);
			}
			float num = component.HalfHeight * 2f * 1.25f;
			WorldToScreen(keyObj.transform, position + new Vector3(0f, num, 0f));
			float num2 = component.HalfHeight * 2f * Tuning.Instance.HUD_KeySizeRelativeToPlayer;
			float num3 = (WorldToScreen(position + new Vector3(0f, num + num2, 0f)).y - keyObj.transform.position.y) * 384f;
			keyObj.transform.localScale = new Vector3(_keyScaleMax.x / _keyScaleMax.y, 1f, 1f) * num3;
		}
		else if (keyObj.active)
		{
			keyObj.SetActive(false);
		}
		Player.Move currentMove = component.CurrentMove;
		if (currentMove._aiming)
		{
			int num4 = (int)(Mathf.Atan2(currentMove._aimDir.y, currentMove._aimDir.x) * 57.29578f);
			if (num4 > 90 && num4 <= 180)
			{
				num4 = 90 - (num4 - 90);
			}
			else if (num4 <= -90 && num4 > -180)
			{
				num4 = -(num4 + 180);
			}
			_nameLabel.text = string.Empty;
			_powerLabel.text = (int)(component.RealFirePower * 100f) + "%";
			_aimLabel.text = num4 + "°";
		}
		else
		{
			_nameLabel.text = _playerName;
			_powerLabel.text = string.Empty;
			_aimLabel.text = string.Empty;
		}
	}

	private void OnDestroy()
	{
		StopAllCoroutines();
	}

	private void OnDisable()
	{
	}

	private void EndGame()
	{
		OnDestroy();
	}

	public void Show(bool show)
	{
		_isShowing = show;
		show &= _gameHUDVisible;
		if (labelsObj != null)
		{
			NGUITools.SetActive(labelsObj, show);
		}
		if (barObj != null)
		{
			NGUITools.SetActive(barObj, show);
		}
		if (shieldObj != null)
		{
			Player component = player.GetComponent<Player>();
			show &= component != null;
			if (show)
			{
				show &= component.ShieldActive;
			}
			NGUITools.SetActive(shieldObj, show);
		}
	}

	public void ShowShield(bool show)
	{
		EB.Debug.Log("ShowShield: show: " + show + ", _isShowing: " + _isShowing + ", _gameHUDVisible: " + _gameHUDVisible);
		show &= _isShowing && _gameHUDVisible;
		if (shieldObj != null)
		{
			EB.Debug.Log("ShowShield, setting active to " + show);
			NGUITools.SetActive(shieldObj, show);
		}
	}

	public UISlider GetShieldProgressBar()
	{
		return shieldMeter;
	}

	public void OnGameHUDVisible(bool bVisible)
	{
		_gameHUDVisible = bVisible;
		Show(_isShowing);
	}
}
