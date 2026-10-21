using EB;
using UnityEngine;

public class ButtonAim : MonoBehaviour
{
	private const string _aimCircleName = "SlicedSprite (aim_circle)";

	private PlayerControlByGUI _playerControl;

	private Vector3 originalPosition;

	private float lastAngle;

	private bool isInit;

	private bool _enabled = true;

	private bool _down;

	private UISlicedSprite _aimCircle;

	public GameObject EquipWeaponLabel;

	public bool ShouldShowEquipWeaponLabel;

	private UISprite _sprite;

	private bool _active;

	private Vector2 _pressedPos;

	private Vector3 _clampedPosition;

	private bool Enabled
	{
		get
		{
			return _enabled;
		}
		set
		{
			if (value)
			{
				if (!_enabled)
				{
					base.transform.localPosition = Vector3.zero;
				}
			}
			else
			{
				_down = false;
				_playerControl.InputCancelAim();
			}
			_enabled = value;
			if (EquipWeaponLabel != null)
			{
				bool flag = !GameAttributes.IsTutorialGameMode();
				flag &= ShouldShowEquipWeaponLabel;
				EquipWeaponLabel.SetActive(flag);
			}
		}
	}

	private Vector3 AimCircleScale
	{
		get
		{
			if (_aimCircle != null)
			{
				return _aimCircle.transform.localScale;
			}
			return new Vector3(200f, 200f, 200f);
		}
	}

	private bool Active
	{
		get
		{
			return _active;
		}
		set
		{
			if (value != _active)
			{
				if (value)
				{
					_playerControl.InputStartAim();
				}
				else if (_down)
				{
					_playerControl.InputCancelAim();
				}
				_active = value;
			}
		}
	}

	private void Awake()
	{
		isInit = false;
		_aimCircle = Util.GetObjectExactMatch(base.transform.parent.transform.parent.gameObject, "SlicedSprite (aim_circle)").GetComponent<UISlicedSprite>();
		if (_aimCircle == null)
		{
			UnityEngine.Debug.LogError("Aim circle not found - using hardcoded (and potentially incorrect) scale instead");
		}
	}

	private void OnPlayerReady()
	{
		if (!isInit)
		{
			Init();
		}
	}

	private void ResetAim()
	{
		_down = false;
		base.transform.localPosition = originalPosition;
		_playerControl.InputCancelAim();
	}

	private void Update()
	{
		if (_playerControl == null)
		{
			return;
		}
		Enabled = _playerControl.CanAim;
		_sprite.gameObject.SetActive(Enabled);
		if (Enabled && !_sprite.enabled)
		{
			_sprite.enabled = true;
		}
		Vector3 vector = BlastronGamepad.Instance.GetAim();
		if (vector.magnitude > 0f)
		{
			_clampedPosition = vector * AimCircleScale.x * 0.5f;
			_down = true;
		}
		if (!_down)
		{
			if (!_playerControl.PostActivationAiming)
			{
				base.transform.localPosition = originalPosition;
			}
		}
		else
		{
			float control_MinPowerActivationDeflection = Tuning.Instance.Control_MinPowerActivationDeflection;
			base.transform.localPosition = _clampedPosition;
			Vector3 delta = NormalizedPosition(_clampedPosition);
			float magnitude = delta.magnitude;
			if (!Active && magnitude > control_MinPowerActivationDeflection)
			{
				Active = true;
			}
			else if (Active && magnitude < control_MinPowerActivationDeflection * 0.5f)
			{
				Active = false;
			}
			float num = Mathf.Max(magnitude - control_MinPowerActivationDeflection, 0f) / (1f - control_MinPowerActivationDeflection);
			delta *= num / magnitude;
			_playerControl.InputAim(delta);
		}
		if (Active)
		{
			_aimCircle.color = Color.red;
		}
		else
		{
			_aimCircle.color = Color.cyan;
		}
		if (BlastronGamepad.Instance.FirePressed())
		{
			_playerControl.InputEndAim();
		}
	}

	private void Init()
	{
		GameObject playerObject = LevelData.Instance.GetPlayerObject();
		if (playerObject != null)
		{
			_playerControl = playerObject.GetComponent<PlayerControlByGUI>();
		}
		originalPosition = base.transform.localPosition;
		_sprite = GetComponentInChildren<UISprite>();
		isInit = true;
	}

	private void OnPress(bool isDown)
	{
		if (_playerControl == null)
		{
			return;
		}
		_down = isDown;
		if (_down)
		{
			Vector3 localPosition = base.transform.localPosition;
			_pressedPos = localPosition;
		}
		else
		{
			if (Enabled && Active)
			{
				_playerControl.InputEndAim();
			}
			Active = false;
		}
		_clampedPosition = ClampedPosition();
	}

	private Vector3 ClampedPosition()
	{
		Vector3 vector = UICamera.currentTouch.totalDelta + _pressedPos;
		float magnitude = vector.magnitude;
		vector.Normalize();
		magnitude = Mathf.Min(magnitude, AimCircleScale.x * 0.5f);
		return vector * magnitude;
	}

	private Vector3 NormalizedPosition(Vector3 clampedPosition)
	{
		return clampedPosition / (AimCircleScale.x * 0.5f);
	}

	private void OnDrag(Vector2 delta)
	{
		if (!(_playerControl == null))
		{
			_clampedPosition = ClampedPosition();
		}
	}
}
