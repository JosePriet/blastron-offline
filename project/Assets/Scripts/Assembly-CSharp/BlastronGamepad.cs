using UnityEngine;

public class BlastronGamepad
{
	private static BlastronGamepad _instance;

	public static BlastronGamepad Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new BlastronGamepad();
			}
			return _instance;
		}
	}

	public Vector2 GetMovement()
	{
		switch (EBGamepad.Instance.GamepadType)
		{
		case EBGamepad.GAMEPAD_TYPE.GAMEPAD:
			return new Vector2(EBGamepad.Instance.ButtonValue(EBGamepad.EB_GAMEPAD_BUTTON.DPAD_X), EBGamepad.Instance.ButtonValue(EBGamepad.EB_GAMEPAD_BUTTON.DPAD_Y));
		case EBGamepad.GAMEPAD_TYPE.EXTENDED_GAMEPAD:
			return new Vector2(EBGamepad.Instance.ButtonValue(EBGamepad.EB_GAMEPAD_BUTTON.LEFT_THUMBSTICK_X), EBGamepad.Instance.ButtonValue(EBGamepad.EB_GAMEPAD_BUTTON.LEFT_THUMBSTICK_Y));
		default:
			return Vector2.zero;
		}
	}

	public Vector2 GetAim()
	{
		EBGamepad.GAMEPAD_TYPE gamepadType = EBGamepad.Instance.GamepadType;
		if (gamepadType == EBGamepad.GAMEPAD_TYPE.EXTENDED_GAMEPAD)
		{
			return new Vector2(EBGamepad.Instance.ButtonValue(EBGamepad.EB_GAMEPAD_BUTTON.RIGHT_THUMBSTICK_X), EBGamepad.Instance.ButtonValue(EBGamepad.EB_GAMEPAD_BUTTON.RIGHT_THUMBSTICK_Y));
		}
		return Vector2.zero;
	}

	public Vector2 GetCameraPan()
	{
		EBGamepad.GAMEPAD_TYPE gamepadType = EBGamepad.Instance.GamepadType;
		if (gamepadType == EBGamepad.GAMEPAD_TYPE.EXTENDED_GAMEPAD)
		{
			return new Vector2(EBGamepad.Instance.ButtonValue(EBGamepad.EB_GAMEPAD_BUTTON.DPAD_X), EBGamepad.Instance.ButtonValue(EBGamepad.EB_GAMEPAD_BUTTON.DPAD_Y));
		}
		return Vector2.zero;
	}

	public bool JumpPressed()
	{
		EBGamepad.GAMEPAD_TYPE gamepadType = EBGamepad.Instance.GamepadType;
		if (gamepadType == EBGamepad.GAMEPAD_TYPE.GAMEPAD || gamepadType == EBGamepad.GAMEPAD_TYPE.EXTENDED_GAMEPAD)
		{
			return EBGamepad.Instance.ButtonPressed(EBGamepad.EB_GAMEPAD_BUTTON.BUTTON_A);
		}
		return false;
	}

	public bool OkPressed()
	{
		EBGamepad.GAMEPAD_TYPE gamepadType = EBGamepad.Instance.GamepadType;
		if (gamepadType == EBGamepad.GAMEPAD_TYPE.GAMEPAD || gamepadType == EBGamepad.GAMEPAD_TYPE.EXTENDED_GAMEPAD)
		{
			return EBGamepad.Instance.ButtonPressed(EBGamepad.EB_GAMEPAD_BUTTON.BUTTON_A);
		}
		return false;
	}

	public bool OkReleased()
	{
		EBGamepad.GAMEPAD_TYPE gamepadType = EBGamepad.Instance.GamepadType;
		if (gamepadType == EBGamepad.GAMEPAD_TYPE.GAMEPAD || gamepadType == EBGamepad.GAMEPAD_TYPE.EXTENDED_GAMEPAD)
		{
			return EBGamepad.Instance.ButtonReleased(EBGamepad.EB_GAMEPAD_BUTTON.BUTTON_A);
		}
		return false;
	}

	public bool CancelPressed()
	{
		EBGamepad.GAMEPAD_TYPE gamepadType = EBGamepad.Instance.GamepadType;
		if (gamepadType == EBGamepad.GAMEPAD_TYPE.GAMEPAD || gamepadType == EBGamepad.GAMEPAD_TYPE.EXTENDED_GAMEPAD)
		{
			return EBGamepad.Instance.ButtonPressed(EBGamepad.EB_GAMEPAD_BUTTON.BUTTON_B);
		}
		return false;
	}

	public bool FirePressed()
	{
		EBGamepad.GAMEPAD_TYPE gamepadType = EBGamepad.Instance.GamepadType;
		if (gamepadType == EBGamepad.GAMEPAD_TYPE.GAMEPAD || gamepadType == EBGamepad.GAMEPAD_TYPE.EXTENDED_GAMEPAD)
		{
			return EBGamepad.Instance.ButtonPressed(EBGamepad.EB_GAMEPAD_BUTTON.RIGHT_SHOULDER);
		}
		return false;
	}

	public bool SwitchWeaponPressed()
	{
		EBGamepad.GAMEPAD_TYPE gamepadType = EBGamepad.Instance.GamepadType;
		if (gamepadType == EBGamepad.GAMEPAD_TYPE.GAMEPAD || gamepadType == EBGamepad.GAMEPAD_TYPE.EXTENDED_GAMEPAD)
		{
			return EBGamepad.Instance.ButtonPressed(EBGamepad.EB_GAMEPAD_BUTTON.BUTTON_X);
		}
		return false;
	}

	public bool SwitchBoostPressed()
	{
		EBGamepad.GAMEPAD_TYPE gamepadType = EBGamepad.Instance.GamepadType;
		if (gamepadType == EBGamepad.GAMEPAD_TYPE.GAMEPAD || gamepadType == EBGamepad.GAMEPAD_TYPE.EXTENDED_GAMEPAD)
		{
			return EBGamepad.Instance.ButtonPressed(EBGamepad.EB_GAMEPAD_BUTTON.BUTTON_Y);
		}
		return false;
	}
}
