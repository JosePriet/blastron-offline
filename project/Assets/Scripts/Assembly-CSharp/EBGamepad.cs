using System.Collections.Generic;
using UnityEngine;

public class EBGamepad : MonoBehaviour
{
	public enum GAMEPAD_TYPE
	{
		NONE = 0,
		GAMEPAD = 1,
		EXTENDED_GAMEPAD = 2,
		KEYBOARD = 3
	}

	public enum EB_GAMEPAD_BUTTON
	{
		DPAD_X = 0,
		DPAD_Y = 1,
		BUTTON_A = 2,
		BUTTON_B = 3,
		BUTTON_X = 4,
		BUTTON_Y = 5,
		LEFT_SHOULDER = 6,
		RIGHT_SHOULDER = 7,
		LEFT_THUMBSTICK_X = 8,
		LEFT_THUMBSTICK_Y = 9,
		RIGHT_THUMBSTICK_X = 10,
		RIGHT_THUMBSTICK_Y = 11,
		LEFT_TRIGGER = 12,
		RIGHT_TRIGGER = 13,
		BUTTON_PAUSE = 14
	}

	public struct EBGamepadSnapShotData
	{
		public float dpadX;

		public float dpadY;

		public float buttonA;

		public float buttonB;

		public float buttonX;

		public float buttonY;

		public float leftShoulder;

		public float rightShoulder;

		public float paused;

		public float gamepadType;

		public float leftThumbstickX;

		public float leftThumbstickY;

		public float rightThumbstickX;

		public float rightThumbstickY;

		public float leftTrigger;

		public float rightTrigger;
	}

	private const float XBOX_CONTROLLER_DEAD_ZONE = 0.2f;

	private const int SNAPSHOT_BUFFER_SIZE = 3;

	public readonly Dictionary<EB_GAMEPAD_BUTTON, KeyCode> GamepadKeyboardMap = new Dictionary<EB_GAMEPAD_BUTTON, KeyCode>
	{
		{
			EB_GAMEPAD_BUTTON.DPAD_X,
			KeyCode.None
		},
		{
			EB_GAMEPAD_BUTTON.DPAD_Y,
			KeyCode.None
		},
		{
			EB_GAMEPAD_BUTTON.BUTTON_A,
			KeyCode.A
		},
		{
			EB_GAMEPAD_BUTTON.BUTTON_B,
			KeyCode.B
		},
		{
			EB_GAMEPAD_BUTTON.BUTTON_X,
			KeyCode.X
		},
		{
			EB_GAMEPAD_BUTTON.BUTTON_Y,
			KeyCode.Y
		},
		{
			EB_GAMEPAD_BUTTON.LEFT_SHOULDER,
			KeyCode.K
		},
		{
			EB_GAMEPAD_BUTTON.RIGHT_SHOULDER,
			KeyCode.L
		},
		{
			EB_GAMEPAD_BUTTON.LEFT_THUMBSTICK_X,
			KeyCode.None
		},
		{
			EB_GAMEPAD_BUTTON.LEFT_THUMBSTICK_Y,
			KeyCode.None
		},
		{
			EB_GAMEPAD_BUTTON.RIGHT_THUMBSTICK_X,
			KeyCode.None
		},
		{
			EB_GAMEPAD_BUTTON.RIGHT_THUMBSTICK_Y,
			KeyCode.None
		},
		{
			EB_GAMEPAD_BUTTON.LEFT_TRIGGER,
			KeyCode.I
		},
		{
			EB_GAMEPAD_BUTTON.RIGHT_TRIGGER,
			KeyCode.O
		},
		{
			EB_GAMEPAD_BUTTON.BUTTON_PAUSE,
			KeyCode.P
		}
	};

	protected List<EBGamepadSnapShotData> mEBGamepadData = new List<EBGamepadSnapShotData>(3);

	protected int mCurrentSnapshotIndex;

	protected GAMEPAD_TYPE mGamepadType;

	public bool SimulateExtendedGamepad = true;

	public bool ShowDebugInfo;

	public static EBGamepad Instance { get; private set; }

	public GAMEPAD_TYPE GamepadType
	{
		get
		{
			return mGamepadType;
		}
	}

	protected virtual void Awake()
	{
		Instance = this;
		for (int i = 0; i < 3; i++)
		{
			mEBGamepadData.Add(default(EBGamepadSnapShotData));
		}
	}

	private void Update()
	{
	}

	private void OnGUI()
	{
		if (ShowDebugInfo)
		{
			string empty = string.Empty;
			empty = empty + "buttonA: " + ButtonValue(EB_GAMEPAD_BUTTON.BUTTON_A);
			empty = empty + "\nbuttonB: " + ButtonValue(EB_GAMEPAD_BUTTON.BUTTON_B);
			empty = empty + "\nbuttonX: " + ButtonValue(EB_GAMEPAD_BUTTON.BUTTON_X);
			empty = empty + "\nbuttonY: " + ButtonValue(EB_GAMEPAD_BUTTON.BUTTON_Y);
			empty = empty + "\ndpadX: " + ButtonValue(EB_GAMEPAD_BUTTON.DPAD_X);
			empty = empty + "\ndpadY: " + ButtonValue(EB_GAMEPAD_BUTTON.DPAD_Y);
			empty = empty + "\nleftShoulder: " + ButtonValue(EB_GAMEPAD_BUTTON.LEFT_SHOULDER);
			empty = empty + "\nrightShoulder: " + ButtonValue(EB_GAMEPAD_BUTTON.RIGHT_SHOULDER);
			empty = empty + "\npaused: " + mEBGamepadData[mCurrentSnapshotIndex].paused;
			empty = empty + "\ntype: " + mGamepadType;
			empty += "\n---------------extended gamepad----------------";
			empty = empty + "\nleftThumbstickX: " + ButtonValue(EB_GAMEPAD_BUTTON.LEFT_THUMBSTICK_X);
			empty = empty + "\nleftThumbstickY: " + ButtonValue(EB_GAMEPAD_BUTTON.LEFT_THUMBSTICK_Y);
			empty = empty + "\nrightThumbstickX: " + ButtonValue(EB_GAMEPAD_BUTTON.RIGHT_THUMBSTICK_X);
			empty = empty + "\nrightThumbstickY: " + ButtonValue(EB_GAMEPAD_BUTTON.RIGHT_THUMBSTICK_Y);
			empty = empty + "\nleftTrigger: " + ButtonValue(EB_GAMEPAD_BUTTON.LEFT_TRIGGER);
			empty = empty + "\nrightTrigger: " + ButtonValue(EB_GAMEPAD_BUTTON.RIGHT_TRIGGER);
			GUI.color = Color.green;
			GUI.Label(new Rect(100f, 100f, 2000f, 600f), empty);
			GUI.color = Color.white;
		}
	}

	public void ToggleKeyboardGamepadType(bool enable)
	{
		mGamepadType = (enable ? GAMEPAD_TYPE.KEYBOARD : GAMEPAD_TYPE.NONE);
	}

	public bool ButtonPressed(EB_GAMEPAD_BUTTON button)
	{
		switch (Application.platform)
		{
		case RuntimePlatform.WindowsEditor:
			switch (button)
			{
			case EB_GAMEPAD_BUTTON.DPAD_X:
			case EB_GAMEPAD_BUTTON.DPAD_Y:
			case EB_GAMEPAD_BUTTON.LEFT_THUMBSTICK_X:
			case EB_GAMEPAD_BUTTON.LEFT_THUMBSTICK_Y:
			case EB_GAMEPAD_BUTTON.RIGHT_THUMBSTICK_X:
			case EB_GAMEPAD_BUTTON.RIGHT_THUMBSTICK_Y:
			case EB_GAMEPAD_BUTTON.LEFT_TRIGGER:
			case EB_GAMEPAD_BUTTON.RIGHT_TRIGGER:
				return Mathf.Abs(Input.GetAxis(button.ToString())) > 0.2f;
			default:
				return Input.GetButton(button.ToString());
			}
		case RuntimePlatform.OSXEditor:
		{
			GAMEPAD_TYPE gAMEPAD_TYPE = mGamepadType;
			if (gAMEPAD_TYPE == GAMEPAD_TYPE.KEYBOARD)
			{
				switch (button)
				{
				case EB_GAMEPAD_BUTTON.DPAD_X:
					return Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow);
				case EB_GAMEPAD_BUTTON.DPAD_Y:
					return Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow);
				default:
				{
					KeyCode keyCode = GamepadKeyboardMap[button];
					if (keyCode == KeyCode.None)
					{
						return false;
					}
					return Input.GetKey(keyCode);
				}
				}
			}
			switch (button)
			{
			case EB_GAMEPAD_BUTTON.LEFT_THUMBSTICK_X:
			case EB_GAMEPAD_BUTTON.LEFT_THUMBSTICK_Y:
			case EB_GAMEPAD_BUTTON.RIGHT_THUMBSTICK_X:
			case EB_GAMEPAD_BUTTON.RIGHT_THUMBSTICK_Y:
			case EB_GAMEPAD_BUTTON.LEFT_TRIGGER:
			case EB_GAMEPAD_BUTTON.RIGHT_TRIGGER:
				return Mathf.Abs(Input.GetAxis(button.ToString() + "_OSX")) > 0.2f;
			case EB_GAMEPAD_BUTTON.DPAD_X:
				return Input.GetButton("DPAD_X_LEFT_OSX") || Input.GetButton("DPAD_X_RIGHT_OSX");
			case EB_GAMEPAD_BUTTON.DPAD_Y:
				return Input.GetButton("DPAD_Y_UP_OSX") || Input.GetButton("DPAD_Y_DOWN_OSX");
			default:
				return Input.GetButton(button.ToString() + "_OSX");
			}
		}
		case RuntimePlatform.IPhonePlayer:
			return GetGamepadButtonPressed(button);
		default:
			return false;
		}
	}

	public bool ButtonReleased(EB_GAMEPAD_BUTTON button)
	{
		switch (Application.platform)
		{
		case RuntimePlatform.WindowsEditor:
			switch (button)
			{
			case EB_GAMEPAD_BUTTON.DPAD_X:
			case EB_GAMEPAD_BUTTON.DPAD_Y:
			case EB_GAMEPAD_BUTTON.LEFT_THUMBSTICK_X:
			case EB_GAMEPAD_BUTTON.LEFT_THUMBSTICK_Y:
			case EB_GAMEPAD_BUTTON.RIGHT_THUMBSTICK_X:
			case EB_GAMEPAD_BUTTON.RIGHT_THUMBSTICK_Y:
			case EB_GAMEPAD_BUTTON.LEFT_TRIGGER:
			case EB_GAMEPAD_BUTTON.RIGHT_TRIGGER:
				return false;
			default:
				return Input.GetButtonUp(button.ToString());
			}
		case RuntimePlatform.OSXEditor:
		{
			GAMEPAD_TYPE gAMEPAD_TYPE = mGamepadType;
			if (gAMEPAD_TYPE == GAMEPAD_TYPE.KEYBOARD)
			{
				switch (button)
				{
				case EB_GAMEPAD_BUTTON.DPAD_X:
					return Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow);
				case EB_GAMEPAD_BUTTON.DPAD_Y:
					return Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.DownArrow);
				default:
				{
					KeyCode keyCode = GamepadKeyboardMap[button];
					if (keyCode == KeyCode.None)
					{
						return false;
					}
					return Input.GetKeyUp(keyCode);
				}
				}
			}
			switch (button)
			{
			case EB_GAMEPAD_BUTTON.LEFT_THUMBSTICK_X:
			case EB_GAMEPAD_BUTTON.LEFT_THUMBSTICK_Y:
			case EB_GAMEPAD_BUTTON.RIGHT_THUMBSTICK_X:
			case EB_GAMEPAD_BUTTON.RIGHT_THUMBSTICK_Y:
			case EB_GAMEPAD_BUTTON.LEFT_TRIGGER:
			case EB_GAMEPAD_BUTTON.RIGHT_TRIGGER:
				return false;
			case EB_GAMEPAD_BUTTON.DPAD_X:
				return Input.GetButtonUp("DPAD_X_LEFT_OSX") || Input.GetButtonUp("DPAD_X_RIGHT_OSX");
			case EB_GAMEPAD_BUTTON.DPAD_Y:
				return Input.GetButtonUp("DPAD_Y_UP_OSX") || Input.GetButtonUp("DPAD_Y_DOWN_OSX");
			default:
				return Input.GetButtonUp(button.ToString() + "_OSX");
			}
		}
		case RuntimePlatform.IPhonePlayer:
			return GetGamepadButtonValue(button, 1) != 0f && GetGamepadButtonValue(button) == 0f;
		default:
			return false;
		}
	}

	public bool ButtonDown(EB_GAMEPAD_BUTTON button)
	{
		switch (Application.platform)
		{
		case RuntimePlatform.WindowsEditor:
			switch (button)
			{
			case EB_GAMEPAD_BUTTON.DPAD_X:
			case EB_GAMEPAD_BUTTON.DPAD_Y:
			case EB_GAMEPAD_BUTTON.LEFT_THUMBSTICK_X:
			case EB_GAMEPAD_BUTTON.LEFT_THUMBSTICK_Y:
			case EB_GAMEPAD_BUTTON.RIGHT_THUMBSTICK_X:
			case EB_GAMEPAD_BUTTON.RIGHT_THUMBSTICK_Y:
			case EB_GAMEPAD_BUTTON.LEFT_TRIGGER:
			case EB_GAMEPAD_BUTTON.RIGHT_TRIGGER:
				return false;
			default:
				return Input.GetButtonDown(button.ToString());
			}
		case RuntimePlatform.OSXEditor:
		{
			GAMEPAD_TYPE gAMEPAD_TYPE = mGamepadType;
			if (gAMEPAD_TYPE == GAMEPAD_TYPE.KEYBOARD)
			{
				switch (button)
				{
				case EB_GAMEPAD_BUTTON.DPAD_X:
					return Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow);
				case EB_GAMEPAD_BUTTON.DPAD_Y:
					return Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow);
				default:
				{
					KeyCode keyCode = GamepadKeyboardMap[button];
					if (keyCode == KeyCode.None)
					{
						return false;
					}
					return Input.GetKeyDown(keyCode);
				}
				}
			}
			switch (button)
			{
			case EB_GAMEPAD_BUTTON.LEFT_THUMBSTICK_X:
			case EB_GAMEPAD_BUTTON.LEFT_THUMBSTICK_Y:
			case EB_GAMEPAD_BUTTON.RIGHT_THUMBSTICK_X:
			case EB_GAMEPAD_BUTTON.RIGHT_THUMBSTICK_Y:
			case EB_GAMEPAD_BUTTON.LEFT_TRIGGER:
			case EB_GAMEPAD_BUTTON.RIGHT_TRIGGER:
				return false;
			case EB_GAMEPAD_BUTTON.DPAD_X:
				return Input.GetButtonDown("DPAD_X_LEFT_OSX") || Input.GetButtonDown("DPAD_X_RIGHT_OSX");
			case EB_GAMEPAD_BUTTON.DPAD_Y:
				return Input.GetButtonDown("DPAD_Y_UP_OSX") || Input.GetButtonDown("DPAD_Y_DOWN_OSX");
			default:
				return Input.GetButtonDown(button.ToString() + "_OSX");
			}
		}
		case RuntimePlatform.IPhonePlayer:
			return GetGamepadButtonValue(button, 2) == 0f && GetGamepadButtonValue(button, 1) == 0f && Mathf.Abs(GetGamepadButtonValue(button)) != 0f;
		default:
			return false;
		}
	}

	public float ButtonValue(EB_GAMEPAD_BUTTON button)
	{
		switch (Application.platform)
		{
		case RuntimePlatform.WindowsEditor:
			switch (button)
			{
			case EB_GAMEPAD_BUTTON.DPAD_X:
			case EB_GAMEPAD_BUTTON.DPAD_Y:
			case EB_GAMEPAD_BUTTON.LEFT_THUMBSTICK_X:
			case EB_GAMEPAD_BUTTON.LEFT_THUMBSTICK_Y:
			case EB_GAMEPAD_BUTTON.RIGHT_THUMBSTICK_X:
			case EB_GAMEPAD_BUTTON.RIGHT_THUMBSTICK_Y:
			case EB_GAMEPAD_BUTTON.LEFT_TRIGGER:
			case EB_GAMEPAD_BUTTON.RIGHT_TRIGGER:
				return Input.GetAxis(button.ToString());
			default:
				return (!ButtonPressed(button)) ? 0f : 1f;
			}
		case RuntimePlatform.OSXEditor:
		{
			GAMEPAD_TYPE gAMEPAD_TYPE = mGamepadType;
			if (gAMEPAD_TYPE == GAMEPAD_TYPE.KEYBOARD)
			{
				switch (button)
				{
				case EB_GAMEPAD_BUTTON.DPAD_X:
					return ((!Input.GetKey(KeyCode.LeftArrow)) ? 0f : (-1f)) + ((!Input.GetKey(KeyCode.RightArrow)) ? 0f : 1f);
				case EB_GAMEPAD_BUTTON.DPAD_Y:
					return ((!Input.GetKey(KeyCode.DownArrow)) ? 0f : (-1f)) + ((!Input.GetKey(KeyCode.UpArrow)) ? 0f : 1f);
				default:
					return (!ButtonPressed(button)) ? 0f : 1f;
				}
			}
			switch (button)
			{
			case EB_GAMEPAD_BUTTON.LEFT_THUMBSTICK_X:
			case EB_GAMEPAD_BUTTON.LEFT_THUMBSTICK_Y:
			case EB_GAMEPAD_BUTTON.RIGHT_THUMBSTICK_X:
			case EB_GAMEPAD_BUTTON.RIGHT_THUMBSTICK_Y:
			case EB_GAMEPAD_BUTTON.LEFT_TRIGGER:
			case EB_GAMEPAD_BUTTON.RIGHT_TRIGGER:
				return Input.GetAxis(button.ToString() + "_OSX");
			case EB_GAMEPAD_BUTTON.DPAD_X:
			{
				float num = 0f;
				if (Input.GetButton("DPAD_X_LEFT_OSX"))
				{
					num += -1f;
				}
				if (Input.GetButton("DPAD_X_RIGHT_OSX"))
				{
					num++;
				}
				return num;
			}
			case EB_GAMEPAD_BUTTON.DPAD_Y:
			{
				float num2 = 0f;
				if (Input.GetButton("DPAD_Y_DOWN_OSX"))
				{
					num2 += -1f;
				}
				if (Input.GetButton("DPAD_Y_UP_OSX"))
				{
					num2++;
				}
				return num2;
			}
			default:
				return (!ButtonPressed(button)) ? 0f : 1f;
			}
		}
		case RuntimePlatform.IPhonePlayer:
			return GetGamepadButtonValue(button);
		default:
			return 0f;
		}
	}

	public Vector2 LeftStick()
	{
		GAMEPAD_TYPE gamepadType = GamepadType;
		if (gamepadType == GAMEPAD_TYPE.EXTENDED_GAMEPAD)
		{
			return new Vector2(ButtonValue(EB_GAMEPAD_BUTTON.LEFT_THUMBSTICK_X), ButtonValue(EB_GAMEPAD_BUTTON.LEFT_THUMBSTICK_Y));
		}
		return Vector2.zero;
	}

	public Vector2 RightStick()
	{
		GAMEPAD_TYPE gamepadType = GamepadType;
		if (gamepadType == GAMEPAD_TYPE.EXTENDED_GAMEPAD)
		{
			return new Vector2(ButtonValue(EB_GAMEPAD_BUTTON.RIGHT_THUMBSTICK_X), ButtonValue(EB_GAMEPAD_BUTTON.RIGHT_THUMBSTICK_Y));
		}
		return Vector2.zero;
	}

	public Vector2 DPad()
	{
		GAMEPAD_TYPE gamepadType = GamepadType;
		if (gamepadType == GAMEPAD_TYPE.GAMEPAD || gamepadType == GAMEPAD_TYPE.EXTENDED_GAMEPAD)
		{
			return new Vector2(ButtonValue(EB_GAMEPAD_BUTTON.DPAD_X), ButtonValue(EB_GAMEPAD_BUTTON.DPAD_Y));
		}
		return Vector2.zero;
	}

	public float GetGamepadButtonValue(EB_GAMEPAD_BUTTON button, int framePast = 0)
	{
		int index = mCurrentSnapshotIndex;
		if (framePast > 0)
		{
			index = ((mCurrentSnapshotIndex - framePast < 0) ? (3 + mCurrentSnapshotIndex - framePast % 3) : (mCurrentSnapshotIndex - framePast));
		}
		switch (button)
		{
		case EB_GAMEPAD_BUTTON.BUTTON_A:
			return mEBGamepadData[index].buttonA;
		case EB_GAMEPAD_BUTTON.BUTTON_B:
			return mEBGamepadData[index].buttonB;
		case EB_GAMEPAD_BUTTON.BUTTON_X:
			return mEBGamepadData[index].buttonX;
		case EB_GAMEPAD_BUTTON.BUTTON_Y:
			return mEBGamepadData[index].buttonY;
		case EB_GAMEPAD_BUTTON.DPAD_X:
			return mEBGamepadData[index].dpadX;
		case EB_GAMEPAD_BUTTON.DPAD_Y:
			return mEBGamepadData[index].dpadY;
		case EB_GAMEPAD_BUTTON.LEFT_SHOULDER:
			return mEBGamepadData[index].leftShoulder;
		case EB_GAMEPAD_BUTTON.RIGHT_SHOULDER:
			return mEBGamepadData[index].rightShoulder;
		case EB_GAMEPAD_BUTTON.LEFT_THUMBSTICK_X:
			return mEBGamepadData[index].leftThumbstickX;
		case EB_GAMEPAD_BUTTON.LEFT_THUMBSTICK_Y:
			return mEBGamepadData[index].leftThumbstickY;
		case EB_GAMEPAD_BUTTON.RIGHT_THUMBSTICK_X:
			return mEBGamepadData[index].rightThumbstickX;
		case EB_GAMEPAD_BUTTON.RIGHT_THUMBSTICK_Y:
			return mEBGamepadData[index].rightThumbstickY;
		case EB_GAMEPAD_BUTTON.LEFT_TRIGGER:
			return mEBGamepadData[index].leftTrigger;
		case EB_GAMEPAD_BUTTON.RIGHT_TRIGGER:
			return mEBGamepadData[index].rightTrigger;
		case EB_GAMEPAD_BUTTON.BUTTON_PAUSE:
			return mEBGamepadData[index].paused;
		default:
			return 0f;
		}
	}

	public bool GetGamepadButtonPressed(EB_GAMEPAD_BUTTON button)
	{
		return Mathf.Abs(GetGamepadButtonValue(button)) > 0f;
	}
}
