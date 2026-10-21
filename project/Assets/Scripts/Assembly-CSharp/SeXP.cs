using System.Collections;
using EB;
using UnityEngine;

public class SeXP : MonoBehaviour
{
	private enum Mode
	{
		kTwinkleSpin = 0,
		kStopNSpurt = 1,
		kSpurt = 2,
		kWait = 3
	}

	private const float _spinPeriod = 0.2f;

	private const float _stayPeriod = 1.5f;

	private const float _departPeriod = 0.5f;

	public float _twinkleInterval = 0.05f;

	public Texture2D _twinkleTex;

	public AudioEvent _spinSound;

	public AudioEvent _spurtSound;

	public AudioEvent _leaveSound;

	private Mode _eMode;

	private UILabel _widget;

	private Quaternion _rotStep;

	private Color[] _twinkleCols;

	private AudioEmitter _audioEmitter;

	private int _rewardXP;

	private float _minCosine;

	private static GameObject _seXPObjectTemplate;

	public string text
	{
		set
		{
			_widget.text = value;
		}
	}

	private static GameObject seXPObject
	{
		get
		{
			if (_seXPObjectTemplate == null)
			{
				_seXPObjectTemplate = Assets.Load("UI/SeXP") as GameObject;
			}
			return _seXPObjectTemplate;
		}
	}

	private int RewardXP
	{
		set
		{
			_rewardXP = value;
			string text = _rewardXP.ToString();
			if (_rewardXP <= 0)
			{
				text = "zero?";
			}
			_widget.text = Localizer.Format("ID_UI_ATTRIBUTE_PLUS_XP", text);
		}
	}

	public static void CreateAtVP(Vector3 pos, int seXPToAward)
	{
		if (seXPToAward > 0)
		{
			GameObject gameObject = Object.Instantiate(seXPObject) as GameObject;
			gameObject.transform.localPosition = Vector3.zero;
			SeXP component = gameObject.GetComponent<SeXP>();
			component.RewardXP = seXPToAward;
			GameObject gameObject2 = Util.GetObject(ScreenManager.Instance.gameObject, "Panel");
			if (gameObject2 != null)
			{
				Util.Parent(gameObject2, gameObject);
			}
			pos = UICamera.currentCamera.ViewportToScreenPoint(pos);
			gameObject.transform.localPosition = ScreenToLocal(pos);
			gameObject.transform.localScale = Vector3.one * 25f;
		}
	}

	private static Vector3 ScreenToLocal(Vector3 pos)
	{
		return new Vector3(pos.x - (float)Screen.width * 0.5f, pos.y - (float)Screen.height * 0.5f, -300f);
	}

	private void Awake()
	{
		_widget = GetComponent<UILabel>();
		_twinkleCols = _twinkleTex.GetPixels();
		_audioEmitter = GetComponent<AudioEmitter>();
		base.transform.localRotation = Quaternion.AngleAxis(Random.Range(-45f, 45f), Vector3.forward);
		float angle = Random.Range(15f, 20f) * (float)(1 + (UnityEngine.Time.frameCount & 1) * -2);
		_rotStep = Quaternion.AngleAxis(angle, Vector3.forward);
		_minCosine = Random.Range(0.866f, 0.95f);
	}

	private void Start()
	{
		StartCoroutine(Sex());
	}

	private void Update()
	{
		switch (_eMode)
		{
		case Mode.kTwinkleSpin:
			break;
		case Mode.kStopNSpurt:
			_eMode = Mode.kSpurt;
			break;
		case Mode.kSpurt:
			_audioEmitter.Play(_spurtSound);
			_eMode = Mode.kWait;
			break;
		}
	}

	private IEnumerator Sex()
	{
		AwardXP();
		_audioEmitter.Play(_spinSound);
		_eMode = Mode.kTwinkleSpin;
		TweenScale.Begin(base.gameObject, 0.2f, base.transform.localScale * Random.Range(2f, 3f));
		yield return StartCoroutine(Twinkle(0.2f));
		_eMode = Mode.kStopNSpurt;
		if (base.particleSystem != null)
		{
			base.particleSystem.Play(true);
		}
		yield return new WaitForSeconds(1.5f);
		_audioEmitter.Play(_leaveSound);
		TweenPosition.Begin(base.gameObject, 0.5f, base.transform.localPosition + Vector3.up * Screen.height);
		TweenColor.Begin(base.gameObject, 0.5f, Color.clear);
		yield return new WaitForSeconds(0.5f);
		Object.Destroy(base.gameObject);
	}

	private void AwardXP()
	{
		int level = Profile.Instance.Level;
		Profile.Instance.AddXP(_rewardXP);
		Profile.Instance.Save();
		int xP = Profile.Instance.XP;
		if (Profile.Instance.Level != level)
		{
			RewardsManager.Instance.GrantLevelUpRewards();
			LevelUpUI component = ScreenManager.Instance.OpenPopup("LevelUpUI", true, true).GetComponent<LevelUpUI>();
			if (component != null)
			{
				component.BackToLobbyTimer = 6f;
			}
		}
	}

	private IEnumerator Twinkle(float duration)
	{
		while (duration > 0f)
		{
			_widget.color = _twinkleCols[Random.Range(0, _twinkleCols.Length)];
			yield return new WaitForSeconds(_twinkleInterval);
			duration -= _twinkleInterval;
		}
	}
}
