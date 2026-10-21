using System;
using UnityEngine;

public class ButtonMoveJump : MonoBehaviour
{
	private PlayerControlByGUI _playerControl;

	private Vector2 _pressAt;

	private Vector2 _nowAt;

	private Vector2 _delta;

	private float _width;

	private float _height;

	private bool _down;

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

	private void Start()
	{
		BoxCollider component = GetComponent<BoxCollider>();
		_width = component.size.x;
		_height = component.size.y;
		_stopMovingMinAngle = Mathf.Sin(Tuning.Instance.Control_StopMovingMinAngle * ((float)Math.PI / 180f));
		_stopMovingMinDeflection = Tuning.Instance.Control_StopMovingMinDeflection;
		_startMovingMaxAngle = Mathf.Sin(Tuning.Instance.Control_StartMovingMaxAngle * ((float)Math.PI / 180f));
		_startMovingMinDeflection = Tuning.Instance.Control_StartMovingMinDeflection;
		_stopJumpingMaxAngle = Mathf.Sin(Tuning.Instance.Control_StopJumpingMaxAngle * ((float)Math.PI / 180f));
		_stopJumpingMaxDeflection = Tuning.Instance.Control_StopJumpingMaxDeflection;
		_startJumpingMinAngle = Mathf.Sin(Tuning.Instance.Control_StartJumpingMinAngle * ((float)Math.PI / 180f));
		_startJumpingMinDeflection = Tuning.Instance.Control_StartJumpingMinDeflection;
	}

	private void Update()
	{
		if (_down)
		{
			ProcessTouch(_nowAt);
			if (_moving)
			{
				_playerControl.InputMove(_nowAt.x > 0f);
			}
			if (_jumping)
			{
				_playerControl.InputJump();
			}
		}
	}

	private void OnPlayerReady()
	{
		GameObject playerObject = LevelData.Instance.GetPlayerObject();
		if (playerObject != null)
		{
			_playerControl = playerObject.GetComponent<PlayerControlByGUI>();
		}
	}

	private void OnPress(bool isDown)
	{
		if (_playerControl == null)
		{
			return;
		}
		if (isDown)
		{
			_pressAt = ToControl(UICamera.currentTouch.pos);
			_nowAt = _pressAt;
			ProcessTouch(_pressAt);
		}
		else
		{
			if (_delta.magnitude < 1f)
			{
			}
			_delta = Vector2.zero;
			_playerControl.InputEndMove();
			_moving = (_jumping = false);
		}
		_down = isDown;
	}

	private void OnDrag(Vector2 delta)
	{
		if (!(_playerControl == null))
		{
			_nowAt = ToControl(UICamera.currentTouch.pos);
			_delta = _nowAt - _pressAt;
		}
	}

	private Vector2 ToControl(Vector2 v)
	{
		Vector3 vector = v;
		Vector2 result = vector - UICamera.currentCamera.WorldToScreenPoint(base.transform.position);
		result.y /= _height * 0.5f;
		result.x *= 2f / _width;
		return result;
	}

	private void ProcessTouch(Vector2 p)
	{
		Vector2 normalized = p.normalized;
		float magnitude = p.magnitude;
		if (_moving && normalized.y > _stopMovingMinAngle && magnitude > _stopMovingMinDeflection)
		{
			_moving = false;
		}
		if (!_moving && normalized.y < _startMovingMaxAngle && magnitude > _startMovingMinDeflection)
		{
			_moving = true;
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
}
