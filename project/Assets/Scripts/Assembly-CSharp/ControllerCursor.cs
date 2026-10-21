using UnityEngine;

public class ControllerCursor : MonoBehaviour
{
	public Camera _uiCamera;

	private Vector2 _cursorPos;

	private Vector2 _delta;

	private Texture2D _cursorTex;

	private UISprite _sprite;

	private bool _bEnabled;

	public static ControllerCursor Instance { get; private set; }

	public bool Enabled
	{
		get
		{
			return _bEnabled;
		}
		set
		{
			if ((bool)EBGamepad.Instance && EBGamepad.Instance.GamepadType != EBGamepad.GAMEPAD_TYPE.NONE && _bEnabled != value)
			{
				if (_sprite != null)
				{
					_sprite.enabled = value;
				}
				_bEnabled = value;
			}
		}
	}

	private void Start()
	{
		Instance = this;
		_cursorPos = new Vector2((float)Screen.width / 2f, (float)Screen.height / 2f);
		_sprite = GetComponentInChildren<UISprite>();
		if (_uiCamera == null)
		{
			_uiCamera = NGUITools.FindCameraForLayer(base.gameObject.layer);
		}
		Enabled = false;
	}

	private void Update()
	{
		if (!Enabled || !EBGamepad.Instance)
		{
			return;
		}
		_delta = EBGamepad.Instance.LeftStick();
		_cursorPos += _delta * 10f;
		_cursorPos.x = Mathf.Clamp(_cursorPos.x, 0f, Screen.width);
		_cursorPos.y = Mathf.Clamp(_cursorPos.y, 0f, Screen.height);
		RaycastHit hit = default(RaycastHit);
		if (UICamera.Raycast(_cursorPos, out hit))
		{
			GameObject gameObject = hit.collider.gameObject;
			if (gameObject.GetComponent<UIInput>() == null)
			{
				UICamera.selectedObject = gameObject;
				if (EBGamepad.Instance.ButtonReleased(EBGamepad.EB_GAMEPAD_BUTTON.BUTTON_A))
				{
					UICamera.Notify(gameObject, "OnPress", true);
					UICamera.Notify(gameObject, "OnClick", null);
				}
			}
			else if (EBGamepad.Instance.ButtonPressed(EBGamepad.EB_GAMEPAD_BUTTON.BUTTON_A))
			{
				UICamera.selectedObject = gameObject;
			}
		}
		else
		{
			UICamera.selectedObject = null;
		}
		if (_sprite != null && _sprite.atlas != null)
		{
			Vector3 zero = Vector3.zero;
			if (_uiCamera != null)
			{
				zero.x = Mathf.Clamp01(_cursorPos.x / (float)Screen.width);
				zero.y = Mathf.Clamp01(_cursorPos.y / (float)Screen.height);
				base.transform.position = _uiCamera.ViewportToWorldPoint(zero);
				Vector3 vector = new Vector3(0f, 0f, -70f);
				base.transform.localPosition += vector;
			}
		}
	}
}
