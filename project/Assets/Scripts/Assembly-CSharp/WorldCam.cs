using UnityEngine;

public class WorldCam : MonoBehaviour
{
	private enum DriveMode
	{
		kUser = 0,
		kMotor = 1
	}

	private enum UpdateMode
	{
		kInert = 0,
		kOrbit = 1,
		kPan = 2
	}

	public delegate void UpdateFn();

	public delegate void MovementFinishFn();

	private const float _minrx = -80f;

	private const float _maxrx = 80f;

	public Material _faderMaterial;

	private float _panUpdateRate = 1f;

	private float _faderAlpha;

	private bool _down;

	private Vector3 _downAt;

	private float _drx;

	private float _dry;

	private float _rx;

	private float _ry;

	private Quaternion _q0;

	private Quaternion _q1;

	private float _t;

	private MovementFinishFn _fnMovementFinished;

	private Vector3 _target;

	private Mesh _faderMesh;

	private int _worldLayer;

	private float _radius;

	private float _motorDuration;

	public float Radius
	{
		get
		{
			return _radius;
		}
		set
		{
			_radius = value;
			base.transform.localPosition = base.transform.rotation * (Vector3.back * _radius);
		}
	}

	public float FaderAlpha
	{
		get
		{
			return _faderAlpha;
		}
		set
		{
			_faderAlpha = value;
			Color black = Color.black;
			black.a = _faderAlpha;
			_faderMaterial.color = black;
		}
	}

	private DriveMode DMode { get; set; }

	private UpdateMode UMode { get; set; }

	private void Awake()
	{
		float orthographicSize = base.camera.orthographicSize;
		float num = orthographicSize * ((float)Screen.width / (float)Screen.height);
		_faderMesh = Misc.MakeQuadMesh(0f - num, 0f - orthographicSize, num, orthographicSize);
		FaderAlpha = 1f;
		_worldLayer = LayerMask.NameToLayer("World");
		base.transform.rotation = Quaternion.LookRotation(Vector3.right);
		base.transform.rotation = Quaternion.Euler(0f, -10.5f, 0f) * base.transform.rotation;
	}

	private void Update()
	{
		switch (UMode)
		{
		case UpdateMode.kOrbit:
			Orbit();
			break;
		case UpdateMode.kPan:
			Pan();
			break;
		}
		if (_faderMaterial != null && FaderAlpha > 0f)
		{
			Graphics.DrawMesh(_faderMesh, base.transform.position + base.transform.forward * (base.camera.nearClipPlane + 0.5f), base.transform.rotation, _faderMaterial, _worldLayer);
		}
	}

	private void Orbit()
	{
		switch (DMode)
		{
		case DriveMode.kUser:
			if (SinglePlayerUI.UserInputEnabled())
			{
				if (Input.GetMouseButtonDown(0))
				{
					_down = true;
					_downAt = Input.mousePosition;
				}
				else if (Input.GetMouseButtonUp(0))
				{
					_down = false;
				}
				if (_down)
				{
					Vector3 vector = Input.mousePosition - _downAt;
					_dry = vector.x * Time.deltaTime;
					_drx = (0f - vector.y) * Time.deltaTime;
				}
				else
				{
					_drx *= 0.95f;
					_dry *= 0.95f;
				}
				float num = 1f;
				if (_rx > 0f && _drx > 0f)
				{
					num = (80f - _rx) / 80f;
				}
				else if (_rx < 0f && _drx < 0f)
				{
					num = (-80f - _rx) / -80f;
				}
				_drx *= num;
				_rx += _drx;
				_ry += _dry;
				Quaternion quaternion = Quaternion.AngleAxis(_rx, Vector3.right);
				Quaternion quaternion2 = Quaternion.AngleAxis(_ry, Vector3.up);
				_q1 = quaternion2 * quaternion;
				base.transform.rotation = _q1;
			}
			break;
		case DriveMode.kMotor:
			_down = false;
			base.transform.rotation = Quaternion.Slerp(_q0, _q1, CubicS(_t));
			_t += Time.deltaTime * 1f / _motorDuration;
			if (_t >= 1f)
			{
				_rx = ToRX(base.transform.eulerAngles.x);
				_ry = base.transform.eulerAngles.y;
				DMode = DriveMode.kUser;
				if (_fnMovementFinished != null)
				{
					_fnMovementFinished();
				}
			}
			break;
		}
		base.transform.localPosition = base.transform.rotation * (Vector3.back * Radius);
	}

	private static float ToRX(float ex)
	{
		if (ex > 180f)
		{
			return ex - 360f;
		}
		return ex;
	}

	private static float CubicS(float t)
	{
		float num = t * t;
		float num2 = num * t;
		return 3f * num - 2f * num2;
	}

	public void OrbitTo(Vector3 pos, MovementFinishFn onFinishMotorOrbit)
	{
		UMode = UpdateMode.kOrbit;
		_motorDuration = 1f;
		DMode = DriveMode.kMotor;
		_q1 = Quaternion.LookRotation(-pos, Vector3.up);
		_q0 = base.transform.rotation;
		float value = Quaternion.Angle(_q0, _q1);
		_motorDuration = Mathf.Clamp(value, 10f, 90f) / 90f;
		_t = 0f;
		_fnMovementFinished = onFinishMotorOrbit;
	}

	private void Pan()
	{
		_t += Time.deltaTime * _panUpdateRate;
		if (_t > 1f)
		{
			if (_fnMovementFinished != null)
			{
				_fnMovementFinished();
			}
		}
		else
		{
			base.transform.rotation = Quaternion.Slerp(_q0, _q1, CubicS(_t));
		}
	}

	public void SlowPan(float duration)
	{
		_q0 = base.transform.rotation;
		_q1 = Quaternion.Euler(0f, 10.5f, 0f) * base.transform.rotation;
		_t = 0f;
		_panUpdateRate = 1f / duration;
		UMode = UpdateMode.kPan;
	}

	public void LookAway(MovementFinishFn onFinishLookaway, float duration)
	{
		_q0 = base.transform.rotation;
		_q1 = Quaternion.AngleAxis(45f, base.transform.up);
		_q1 *= _q0;
		_t = 0f;
		_panUpdateRate = 1f / duration;
		UMode = UpdateMode.kPan;
		_fnMovementFinished = () =>
		{
			UMode = UpdateMode.kInert;
			if (onFinishLookaway != null)
			{
				onFinishLookaway();
			}
		};
	}

	public void LookAtWorld(MovementFinishFn onFinishLookat, float duration)
	{
		if (duration == 0f)
		{
			Quaternion quaternion = Quaternion.LookRotation(-base.transform.localPosition);
			base.transform.rotation = quaternion;
			_q0 = (_q1 = quaternion);
			if (onFinishLookat != null)
			{
				onFinishLookat();
			}
			UMode = UpdateMode.kOrbit;
			return;
		}
		_q0 = base.transform.rotation;
		_q1 = Quaternion.LookRotation(-base.transform.localPosition);
		_panUpdateRate = 1f / duration;
		_t = 0f;
		UMode = UpdateMode.kPan;
		_fnMovementFinished = () =>
		{
			UMode = UpdateMode.kOrbit;
			if (onFinishLookat != null)
			{
				onFinishLookat();
			}
		};
	}
}
