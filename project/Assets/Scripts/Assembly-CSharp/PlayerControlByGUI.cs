using EB.Sparx;
using UnityEngine;

public class PlayerControlByGUI : MonoBehaviour
{
	public enum JumpDir
	{
		kUp = 0,
		kLeft = 1,
		kRight = 2
	}

	private enum TouchDeflectionPlatform
	{
		kPad = 0,
		kPhone = 1
	}

	private class TouchDeflections
	{
		public float _cMoveDeflection;

		public float _cFaceDeflection;
	}

	private const float cGUIWidth = 200f;

	private Player _player;

	private bool _moving;

	private bool _aiming;

	private Vector3 _moveDelta;

	private Vector3 _aimDelta;

	private Vector3 _aimDir;

	private bool _jumpEnable;

	private bool _dropEnable;

	private bool _fireEnable;

	private float _power;

	private bool _faceRight;

	private bool _faceLeft;

	private bool _goingLeft;

	private bool _goingRight;

	private TouchDeflections _touchDeflections;

	private bool _inputEnabled;

	private bool _didAnything;

	private int _idleCount;

	private TouchDeflections[] _touchDeflectionsTable = new TouchDeflections[2]
	{
		new TouchDeflections
		{
			_cMoveDeflection = 5f
		},
		new TouchDeflections
		{
			_cMoveDeflection = 1f
		}
	};

	public bool PostActivationAiming
	{
		get
		{
			if (_player != null && _player.CurrentWeapon != null)
			{
				return _player.CurrentWeapon._postActivationAim;
			}
			return false;
		}
	}

	public bool CanAim
	{
		get
		{
			if (_player != null)
			{
				return _player.IsInputEnabled && _player.CanAim() && !_player.CurrentWeapon.name.Equals("Empty");
			}
			return false;
		}
	}

	public bool CanMove
	{
		get
		{
			if (_player != null)
			{
				return _player.IsInputEnabled;
			}
			return false;
		}
	}

	public bool IsAiming
	{
		get
		{
			return _aiming;
		}
	}

	public static bool DebugOn { get; set; }

	private void EnableInput(bool enable)
	{
		if (_inputEnabled && !enable && base.enabled)
		{
			if (_didAnything)
			{
				_idleCount = 0;
			}
			else
			{
				_idleCount++;
				if (_idleCount >= Tuning.Instance.IdleTurnKickCount)
				{
					Game game = Hub.Instance.GameManager.Game;
					if (game != null && !game.Network.IsLocal)
					{
						game.LeaveGame("ID_GAME_KICKED_IDLE");
						LevelData.Instance.TerminateGame();
					}
				}
			}
		}
		_inputEnabled = enable;
		if (!_inputEnabled)
		{
			_fireEnable = false;
		}
		_didAnything = false;
	}

	private void Start()
	{
		_player = GetComponent<Player>();
		TouchDeflectionPlatform touchDeflectionPlatform = (Misc.IsPhone() ? TouchDeflectionPlatform.kPhone : TouchDeflectionPlatform.kPad);
		_touchDeflections = _touchDeflectionsTable[(int)touchDeflectionPlatform];
	}

	public void InputStartMove()
	{
		_moving = true;
		_moveDelta = Vector3.zero;
		_didAnything = true;
	}

	public void InputEndMove()
	{
		_moving = false;
		_moveDelta = Vector3.zero;
		_goingLeft = (_goingRight = false);
	}

	public void InputMoveDir(Vector3 delta)
	{
		_moveDelta = delta;
	}

	public void InputMove(bool right)
	{
		_didAnything = true;
		_goingRight = right;
		_goingLeft = !_goingRight;
	}

	public void InputStartAim()
	{
		if (CanAim)
		{
			_didAnything = true;
			_power = 0f;
			_aiming = true;
			_aimDelta = Vector3.zero;
			Camera.main.SendMessage("PlayerStartAim");
		}
	}

	public void InputCancelAim()
	{
		_aiming = false;
	}

	public void InputEndAim()
	{
		if (IsAiming)
		{
			_aiming = false;
			if (DebugSettings.FireOnEndAim)
			{
				InputEndFire();
			}
		}
	}

	public void InputAim(Vector3 delta)
	{
		if (IsAiming)
		{
			_didAnything = true;
			_aimDelta = delta;
		}
	}

	public void InputStartFire()
	{
		_power = 0f;
		_didAnything = true;
	}

	public void InputEndFire()
	{
		_fireEnable = true;
	}

	public void InputJump()
	{
		if (_player.Grounded || _player.IsHoverpackActive() || _player.IsJetpackActive())
		{
			_jumpEnable = true;
			_dropEnable = false;
			_didAnything = true;
		}
	}

	public void InputDrop(bool right)
	{
		if (!_player.Grounded && _player.IsHoverpackActive())
		{
			_jumpEnable = false;
			_dropEnable = true;
			_didAnything = true;
		}
		InputMove(right);
	}

	private void Update()
	{
		if (!_inputEnabled)
		{
			return;
		}
		Player.PlayerInstructions move = new Player.PlayerInstructions
		{
			airborne = !_player.Grounded,
			faceRight = _faceRight,
			faceLeft = _faceLeft
		};
		if (DebugSettings.SlideJumpLeft)
		{
			move.goRight = _goingRight;
			move.goLeft = _goingLeft;
		}
		if (_moving)
		{
			if (!_goingRight || _goingLeft)
			{
				_goingRight = _moveDelta.x > _touchDeflections._cMoveDeflection;
			}
			if (!_goingLeft || _goingRight)
			{
				_goingLeft = _moveDelta.x < 0f - _touchDeflections._cMoveDeflection;
			}
			move.goRight = _goingRight;
			move.goLeft = _goingLeft;
		}
		if (_jumpEnable)
		{
			move.jump = true;
			_jumpEnable = false;
			move.goRight = _goingRight;
			move.goLeft = _goingLeft;
			_goingRight = (_goingLeft = false);
		}
		if (_dropEnable)
		{
			move.drop = true;
			_dropEnable = false;
			move.goRight = _goingRight;
			move.goLeft = _goingLeft;
			_goingRight = (_goingLeft = false);
		}
		if (_aiming)
		{
			float magnitude = _aimDelta.magnitude;
			Vector3 normalized = _aimDelta.normalized;
			_power = magnitude * magnitude;
			_aimDir = Vector3.Slerp(_aimDir, normalized, Mathf.Clamp01(Tuning.Instance.Control_PowerMeterRotationCatchup));
		}
		move.fire = _fireEnable;
		move.fireMag = _power;
		move.fireDir = _aimDir;
		move.aiming = _aiming;
		move.moveDir = _moveDelta;
		move.moveDir.Normalize();
		_player.Move2(move);
		_fireEnable = false;
		_faceLeft = (_faceRight = false);
	}
}
