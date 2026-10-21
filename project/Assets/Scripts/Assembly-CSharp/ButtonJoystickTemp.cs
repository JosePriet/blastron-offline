using System;
using EB;
using UnityEngine;

public class ButtonJoystickTemp : MonoBehaviour
{
	private const string _moveCircleName = "SlicedSprite (aim_circle)";

	private PlayerControlByGUI _playerControl;

	private Vector2 _pressAt;

	private Vector2 _delta;

	private Vector2 _deltaJoystick;

	private bool _down;

	private bool _usingJoystick;

	private bool _moving;

	private bool _jumping;

	private float _stopMovingMinAngle = 0.866f;

	private float _stopMovingMinDeflection = 0.5f;

	private float _startMovingMaxAngle = 0.707f;

	private float _startMovingMinDeflection = 0.25f;

	private float _stopJumpingMaxAngle = 0.5f;

	private float _stopJumpingMaxDeflection = 0.25f;

	private float _startJumpingMinAngle = 0.707f;

	private float _startJumpingMinDeflection = 0.5f;

	public float maxDeflection = 100f;

	public float minDeflection = 30f;

	private UISlicedSprite _moveCircle;

	private Vector3 originalPosition;

	private bool isInit;

	private Transform joystickButton;

	private bool _enabled = true;

	private UISprite _sprite;

	private GameObject[] _diskSprites;

	private Vector3 _clampedPosition;

	private Vector3 MoveCircleScale
	{
		get
		{
			if (_moveCircle != null)
			{
				return _moveCircle.transform.localScale;
			}
			return new Vector3(200f, 200f, 200f);
		}
	}

	private void Awake()
	{
		isInit = false;
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "ButtonJoystick");
		joystickButton = objectExactMatch.transform;
		_sprite = joystickButton.GetComponentInChildren<UISprite>();
		_diskSprites = Util.GetObjects(base.gameObject, "hud_joystick_disk");
		GameObject gameObject = base.gameObject.transform.parent.transform.parent.gameObject;
		EB.Debug.Log("BUTTON ROOT = " + gameObject.name);
		_moveCircle = Util.GetObjectExactMatch(gameObject, "SlicedSprite (aim_circle)").GetComponent<UISlicedSprite>();
		if (_moveCircle == null)
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

	private void Init()
	{
		GameObject playerObject = LevelData.Instance.GetPlayerObject();
		if (playerObject != null)
		{
			_playerControl = playerObject.GetComponent<PlayerControlByGUI>();
		}
		originalPosition = joystickButton.localPosition;
		isInit = true;
	}

	private void Start()
	{
		BoxCollider component = GetComponent<BoxCollider>();
		_stopMovingMinAngle = Mathf.Sin(Tuning.Instance.Control_StopMovingMinAngle * ((float)Math.PI / 180f));
		_stopMovingMinDeflection = Tuning.Instance.Control_StopMovingMinDeflection;
		_startMovingMaxAngle = Mathf.Sin(Tuning.Instance.Control_StartMovingMaxAngle * ((float)Math.PI / 180f));
		_startMovingMinDeflection = Tuning.Instance.Control_StartMovingMinDeflection;
		_stopJumpingMaxAngle = Mathf.Sin(Tuning.Instance.Control_StopJumpingMaxAngle * ((float)Math.PI / 180f));
		_stopJumpingMaxDeflection = Tuning.Instance.Control_StopJumpingMaxDeflection;
		_startJumpingMinAngle = Mathf.Sin(Tuning.Instance.Control_StartJumpingMinAngle * ((float)Math.PI / 180f));
		_startJumpingMinDeflection = Tuning.Instance.Control_StartJumpingMinDeflection;
	}

	private void ShowDisk(bool show)
	{
		GameObject[] diskSprites = _diskSprites;
		foreach (GameObject go in diskSprites)
		{
			NGUITools.SetActive(go, show);
		}
	}

	private void Update()
	{
		if (_playerControl == null)
		{
			return;
		}
		if (DebugSettings.JoystickLeft && _enabled != _playerControl.CanMove)
		{
			_enabled = _playerControl.CanMove;
			_sprite.gameObject.SetActive(_enabled);
			if (_enabled && !_sprite.enabled)
			{
				_sprite.enabled = true;
			}
		}
		if (!_enabled)
		{
			return;
		}
		Vector3 vector = BlastronGamepad.Instance.GetMovement();
		if (vector.magnitude > 0.35f)
		{
			_usingJoystick = true;
			_clampedPosition = vector * MoveCircleScale.x * 0.5f;
			_delta = vector;
			_down = true;
			joystickButton.localPosition = _clampedPosition;
		}
		else if (_usingJoystick)
		{
			_delta = Vector2.zero;
			_playerControl.InputEndMove();
			_moving = (_jumping = false);
			joystickButton.localPosition = originalPosition;
			_usingJoystick = false;
		}
		ShowDisk(_playerControl.CanMove);
		if (_down)
		{
			ProcessTouch(NormalizedPosition(_clampedPosition));
			if (_moving)
			{
				_playerControl.InputMove(_delta.x > 0f);
			}
			if (_jumping)
			{
				_playerControl.InputJump();
			}
			_playerControl.InputMoveDir(_delta);
		}
		else
		{
			_playerControl.InputMoveDir(Vector2.zero);
		}
		if (_usingJoystick)
		{
			_down = false;
		}
	}

	private void OnPress(bool isDown)
	{
		if (!(_playerControl == null))
		{
			if (isDown)
			{
				_pressAt = UICamera.currentTouch.pos;
			}
			else
			{
				_delta = Vector2.zero;
				_playerControl.InputEndMove();
				_moving = (_jumping = false);
				joystickButton.localPosition = originalPosition;
			}
			_down = isDown;
			_clampedPosition = ClampedPosition();
		}
	}

	private void OnDrag(Vector2 delta)
	{
		if (!(_playerControl == null))
		{
			_delta = UICamera.currentTouch.pos - _pressAt;
			if (DebugSettings.JoystickLeft)
			{
				joystickButton.localPosition = ClampedPosition();
			}
			_clampedPosition = ClampedPosition();
		}
	}

	private void ProcessTouch(Vector2 p)
	{
		Vector2 normalized = p.normalized;
		float magnitude = p.magnitude;
		if (_moving && normalized.y > _stopMovingMinAngle && magnitude > _stopMovingMinDeflection)
		{
			_moving = false;
		}
		if (!_moving)
		{
			if (normalized.y > 0f)
			{
				if (normalized.y < _startMovingMaxAngle && magnitude > _startMovingMinDeflection)
				{
					_moving = true;
				}
			}
			else if (Mathf.Abs(p.x) > Tuning.Instance.Control_LateralDeadZone)
			{
				_moving = true;
			}
		}
		if (_jumping && (normalized.y < _stopJumpingMaxAngle || magnitude < _stopJumpingMaxDeflection))
		{
			_jumping = false;
		}
		if (!_jumping && normalized.y > _startJumpingMinAngle && magnitude > _startJumpingMinDeflection)
		{
			_jumping = true;
		}
	}

	private Vector3 ClampedPosition()
	{
		Vector3 vector = _delta;
		float magnitude = vector.magnitude;
		vector.Normalize();
		magnitude = Mathf.Min(magnitude, MoveCircleScale.x * 0.5f);
		return vector * magnitude;
	}

	private Vector3 NormalizedPosition(Vector3 clampedPosition)
	{
		return clampedPosition / (MoveCircleScale.x * 0.5f);
	}
}
