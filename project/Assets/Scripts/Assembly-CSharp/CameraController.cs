using System.Collections;
using System.Collections.Generic;
using EB.Replication;
using EB.Sparx;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	public enum UpdateMode
	{
		kAuto = 0,
		kUser = 1,
		kSequence = 2
	}

	public class SequenceCameraSettings
	{
		public Vector3 focusPoint;

		public float orthoFracCatchup;

		public float zoom;
	}

	private const float _range = 70f;

	private GameObject _player;

	private Vector2 _viewLimitMax;

	private Vector2 _viewLimitMin;

	public float _topBuffer = 128f;

	public float _bottomBuffer = 128f;

	public float _leftBuffer = 128f;

	public float _rightBuffer = 128f;

	public float OrthoFracMin = 0.3f;

	private float OrthoFracMax = 0.975f;

	public float OrthoFracDefault = 0.65f;

	public float _orthoMax;

	public float _orthoFrac0;

	public float _orthoFrac1;

	private Vector3 _p0;

	private Vector3 _p1;

	private float _savedZoom = 1f;

	private bool _disableInput;

	private Vector2[] _fingers = new Vector2[2];

	private SequenceCameraSettings sequenceCameraSettings;

	private UpdateMode _eMoveMode;

	private Material _debugMaterial;

	private GameObject _overlayCamObj;

	public float OrthoMax
	{
		get
		{
			return _orthoMax;
		}
		set
		{
			_orthoMax = value;
		}
	}

	public float Zoom01
	{
		get
		{
			return (_orthoFrac1 - OrthoFracMin) / (OrthoFracMax - OrthoFracMin);
		}
		set
		{
			_orthoFrac1 = OrthoFracMin + Mathf.Clamp01(value) * (OrthoFracMax - OrthoFracMin);
		}
	}

	public Vector2 ViewLimitMin
	{
		get
		{
			return _viewLimitMin;
		}
	}

	public Vector2 ViewLimitMax
	{
		get
		{
			return _viewLimitMax;
		}
	}

	private int Touches { get; set; }

	private Material DebugMaterial
	{
		get
		{
			if (_debugMaterial == null)
			{
				_debugMaterial = new Material(Shader.Find("Custom/Color"));
			}
			return _debugMaterial;
		}
	}

	public bool IntroRunning { get; private set; }

	public bool OutroRunning { get; private set; }

	private void Awake()
	{
		EB.Replication.Manager.RegisterRPCs(this);
	}

	private IEnumerator Start()
	{
		while (LevelData.Instance == null)
		{
			yield return new WaitForFixedUpdate();
		}
		_disableInput = false;
		_viewLimitMin = LevelData.Instance.GameplayMin;
		_viewLimitMax = LevelData.Instance.GameplayMax;
		_viewLimitMax.y = Mathf.Max(Mathf.Min(Screen.height, 768f) * 0.1f, _viewLimitMax.y);
		_viewLimitMin.x -= _leftBuffer * 0.1f;
		_viewLimitMin.y -= _bottomBuffer * 0.1f;
		_viewLimitMax.x += _rightBuffer * 0.1f;
		_viewLimitMax.y += _topBuffer * 0.1f;
		_p1 = Clamp(_p1);
		_p1.z = 0f;
		_p1 += Vector3.back * 70f;
		base.transform.position = (_p0 = _p1);
		UICamera.fallThrough = base.gameObject;
		OrthoFracMin = ((!Misc.IsPhone()) ? Tuning.Instance.Camera_OrthoFracMin : Tuning.Instance.Camera_OrthoFracMinPhone);
		OrthoFracDefault = ((!Misc.IsPhone()) ? Tuning.Instance.Camera_OrthoFracDefault : Tuning.Instance.Camera_OrthoFracDefaultPhone);
		OrthoFracMax = ((!Misc.IsPhone()) ? Tuning.Instance.Camera_OrthoFracMax : Tuning.Instance.Camera_OrthoFracMaxPhone);
		_orthoFrac0 = OrthoFracMax;
		_orthoFrac1 = Mathf.Clamp(OrthoFracDefault, OrthoFracMin, OrthoFracMax);
		_savedZoom = _orthoFrac1;
		_eMoveMode = UpdateMode.kAuto;
		base.gameObject.AddComponent<CameraShake>();
		base.camera.nearClipPlane = 5f;
		base.camera.farClipPlane = 200f;
		base.camera.clearFlags = CameraClearFlags.Color;
		base.camera.backgroundColor = Color.black;
		EnableCameraRender(true);
	}

	public void EnableCameraRender(bool enable)
	{
		if (enable)
		{
			base.camera.cullingMask = (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("TransparentFX")) | (1 << LayerMask.NameToLayer("Ignore Raycast")) | (1 << LayerMask.NameToLayer("Water")) | (1 << LayerMask.NameToLayer("OMIT"));
			if (Misc.IsSlow())
			{
				base.camera.cullingMask &= ~(1 << LayerMask.NameToLayer("OMIT"));
			}
		}
		else
		{
			base.camera.cullingMask = 0;
		}
	}

	private void PlayerMoved(GameObject playerObject)
	{
		if (!Misc.isSP || !(LevelData.Instance.GetPlayer().gameObject != playerObject))
		{
			if (Tuning.Instance.Camera_CenterPlayerOnMove)
			{
				_eMoveMode = UpdateMode.kAuto;
			}
			else
			{
				KeepPlayerOnScreen();
			}
		}
	}

	private void PlayerFired()
	{
		if (Tuning.Instance.Camera_ZoomOutOnFire)
		{
			_orthoFrac1 = 1f;
		}
	}

	private void PlayerStartAim()
	{
		if (Tuning.Instance.Camera_KeepPlayerOnScreenDuringAim)
		{
			KeepPlayerOnScreen();
		}
	}

	private void KeepPlayerOnScreen()
	{
		if (_player != null && _eMoveMode == UpdateMode.kUser)
		{
			Vector3 vector = _player.transform.position + Vector3.back * 70f;
			Vector3 vector2 = vector - _p0;
			float num = base.camera.orthographicSize * Tuning.Instance.Camera_KeepOnScreenZone.y;
			float num2 = base.camera.orthographicSize * base.camera.aspect * Tuning.Instance.Camera_KeepOnScreenZone.x;
			if (vector2.x < 0f - num2)
			{
				_p1.x += vector2.x + num2;
			}
			else if (vector2.x > num2)
			{
				_p1.x += vector2.x - num2;
			}
			if (vector2.y < 0f - num)
			{
				_p1.y += vector2.y + num;
			}
			else if (vector2.y > num)
			{
				_p1.y += vector2.y - num;
			}
		}
	}

	public void FollowPlayer(GameObject player)
	{
		if (!OutroRunning)
		{
			_player = player;
			_eMoveMode = UpdateMode.kAuto;
		}
	}

	public void ZoomPlayer(GameObject playerObject)
	{
		if ((!Misc.isSP || !(LevelData.Instance.GetPlayer().gameObject != playerObject)) && !OutroRunning)
		{
			_player = playerObject;
			_eMoveMode = UpdateMode.kAuto;
			if (Tuning.Instance.Camera_ZoomOnGap)
			{
				_savedZoom = _orthoFrac1;
				_orthoFrac1 = Mathf.Clamp(Tuning.Instance.Camera_OrthoZoomGap, OrthoFracMin, OrthoFracMax);
			}
		}
	}

	public void UnzoomPlayer()
	{
		if (!Misc.isSP && !OutroRunning)
		{
			if (Tuning.Instance.Camera_ReturnToLocalOnGap)
			{
				_player = LevelData.Instance.GetPlayer().gameObject;
				_eMoveMode = UpdateMode.kAuto;
			}
			if (Tuning.Instance.Camera_ZoomOnGap)
			{
				_orthoFrac1 = _savedZoom;
			}
		}
	}

	private void Update()
	{
		float num = Mathf.Clamp(Time.smoothDeltaTime * Tuning.Instance.Camera_OrthoFracCatchup, 0f, 1f);
		float t = Mathf.Clamp(Time.smoothDeltaTime * Tuning.Instance.Camera_OrthoFracCatchup, 0f, 1f);
		if (_eMoveMode == UpdateMode.kSequence && sequenceCameraSettings != null)
		{
			num = Mathf.Clamp(Time.smoothDeltaTime * sequenceCameraSettings.orthoFracCatchup, 0f, 1f);
			t = Mathf.Clamp(Time.smoothDeltaTime * sequenceCameraSettings.orthoFracCatchup, 0f, 1f);
		}
		if (OutroRunning)
		{
			num = Mathf.Clamp(Time.smoothDeltaTime * Tuning.Instance.Outro_Camera_OrthoFracCatchup, 0f, 1f);
			t = Mathf.Clamp(Time.smoothDeltaTime * Tuning.Instance.Outro_Camera_OrthoFracCatchup, 0f, 1f);
		}
		_orthoFrac0 += (_orthoFrac1 - _orthoFrac0) * num;
		base.camera.orthographicSize = _orthoMax * _orthoFrac0;
		if (_player != null && _eMoveMode == UpdateMode.kAuto)
		{
			_p1 = _player.transform.position;
			_p1.z = 0f;
			_p1 += Vector3.back * 70f;
		}
		if (_eMoveMode != UpdateMode.kSequence)
		{
			_p1 = Clamp(_p1);
		}
		_p0 = Vector3.Lerp(_p0, _p1, t);
		_p0 = Clamp(_p0);
		base.transform.position = _p0;
	}

	private Vector3 Clamp(Vector3 p)
	{
		float num = Mathf.Min(base.camera.orthographicSize * base.camera.aspect, (_viewLimitMax.x - _viewLimitMin.x) * 0.5f);
		float num2 = Mathf.Min(base.camera.orthographicSize, (_viewLimitMax.y - _viewLimitMin.y) * 0.5f);
		p.x = Mathf.Clamp(p.x, _viewLimitMin.x + num, _viewLimitMax.x - num);
		p.y = Mathf.Clamp(p.y, _viewLimitMin.y + num2, _viewLimitMax.y - num2);
		return p;
	}

	private void OnPlayerReady()
	{
		if (!OutroRunning)
		{
			_player = LevelData.Instance.GetPlayer().gameObject;
		}
	}

	private void OnPress(bool press)
	{
		if (!_disableInput && UICamera.currentTouchID >= 0 && UICamera.currentTouchID < 2)
		{
			_fingers[UICamera.currentTouchID] = UICamera.currentTouch.pos;
			Touches += (press ? 1 : (-1));
		}
	}

	private void OnDrag(Vector2 delta)
	{
		if (!_disableInput && UICamera.currentTouchID >= 0 && UICamera.currentTouchID < 2)
		{
			if (Touches == 2)
			{
				float magnitude = (_fingers[1] - _fingers[0]).magnitude;
				_fingers[UICamera.currentTouchID] = UICamera.currentTouch.pos;
				float magnitude2 = (_fingers[1] - _fingers[0]).magnitude;
				Zoom(magnitude / magnitude2);
			}
			else if (Touches == 1)
			{
				Truck(-delta * Tuning.Instance.Camera_TruckSensitivity * _orthoFrac1);
			}
		}
	}

	private void Zoom(float z)
	{
		if (!IntroRunning && !OutroRunning)
		{
			_orthoFrac1 = Mathf.Clamp(_orthoFrac1 * z, OrthoFracMin, OrthoFracMax);
			ScreenManager.Instance.BroadcastMessage("OnZoomChanged", this, SendMessageOptions.DontRequireReceiver);
		}
	}

	private void Truck(Vector3 t)
	{
		if (!IntroRunning && !OutroRunning)
		{
			_eMoveMode = UpdateMode.kUser;
			_p1 += t;
		}
	}

	private void DisableInput(bool disable)
	{
		_disableInput = disable;
		Touches = 0;
	}

	private void Sequence_CameraOverride(SequenceCameraSettings settings)
	{
		if (settings != null)
		{
			_eMoveMode = UpdateMode.kSequence;
			sequenceCameraSettings = settings;
			_p1 = settings.focusPoint + Vector3.back * 70f;
			if (settings.zoom != -1f)
			{
				Zoom01 = settings.zoom;
			}
		}
	}

	public bool IsCameraMoving()
	{
		return Mathf.Approximately(_p0.x - _p1.x, 0.1f) && Mathf.Approximately(_p0.y - _p1.y, 0.1f) && Mathf.Approximately(_orthoFrac0 - _orthoFrac1, 0.001f);
	}

	public Vector3 NormalizedCentredLevelPosition()
	{
		float num = _viewLimitMax.x - _viewLimitMin.x;
		float num2 = _viewLimitMax.y - _viewLimitMin.y;
		float x = (base.transform.position.x - num * 0.5f) / num;
		float y = (base.transform.position.y - num2 * 0.5f) / num2;
		return new Vector3(x, y, 0f);
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.magenta;
		Gizmos.DrawWireCube((_viewLimitMax - _viewLimitMin) / 2f + _viewLimitMin, _viewLimitMax - _viewLimitMin);
	}

	[RPC]
	public void IntroSequence()
	{
		IntroRunning = true;
		StartCoroutine(_Intro());
	}

	public void OutroSequence(int numWinners)
	{
		OutroRunning = true;
		StartCoroutine(_Outro(numWinners));
	}

	private IEnumerator _Intro()
	{
		while (_player == null)
		{
			yield return 1;
		}
		_orthoFrac1 = OrthoFracMin;
		yield return new WaitForSeconds(Tuning.Instance.Camera_Intro_X_Time);
		float delta = (OrthoFracMax - _orthoFrac1) / Tuning.Instance.Camera_Intro_Y_Time;
		while (_orthoFrac0 < OrthoFracMax)
		{
			_orthoFrac1 += delta * Time.deltaTime;
			yield return 1;
		}
		yield return new WaitForSeconds(Tuning.Instance.Camera_Intro_Z_Time);
		_orthoFrac1 = OrthoFracDefault;
		ScreenManager.Instance.BroadcastMessage("OnZoomChanged", this, SendMessageOptions.DontRequireReceiver);
		IntroRunning = false;
	}

	private IEnumerator _Outro(int numWinners)
	{
		List<EB.Sparx.Player> playerList = Scoring.Instance.GetEndGamePlayers();
		if (playerList != null && playerList.Count > 0)
		{
			Player player = LevelData.Instance.GetPlayer(playerList[0].PlayerId);
			if (player != null)
			{
				_player = player.gameObject;
				_eMoveMode = UpdateMode.kAuto;
				_savedZoom = _orthoFrac1;
				_orthoFrac1 = Mathf.Clamp(Tuning.Instance.Outro_Camera_PlayerZoom, OrthoFracMin, OrthoFracMax);
			}
		}
		if (numWinners == 1)
		{
			yield return new WaitForSeconds(Tuning.Instance.Outro_Camera_HoldTime);
		}
		else
		{
			yield return new WaitForSeconds(Tuning.Instance.Outro_Camera_HoldTime1_Team);
			if (playerList != null && playerList.Count > 1)
			{
				Player player2 = LevelData.Instance.GetPlayer(playerList[1].PlayerId);
				if (player2 != null)
				{
					_player = player2.gameObject;
					_eMoveMode = UpdateMode.kAuto;
					_savedZoom = _orthoFrac1;
					_orthoFrac1 = Mathf.Clamp(Tuning.Instance.Outro_Camera_PlayerZoom, OrthoFracMin, OrthoFracMax);
				}
			}
			yield return new WaitForSeconds(Tuning.Instance.Outro_Camera_HoldTime2_Team);
		}
		OutroRunning = false;
	}
}
