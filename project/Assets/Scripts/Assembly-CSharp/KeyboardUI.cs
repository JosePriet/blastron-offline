using UnityEngine;

public class KeyboardUI : MonoBehaviour
{
	public struct CGRect
	{
		public float OriginX;

		public float OriginY;

		public float Width;

		public float Height;
	}

	public enum IOSReturnKeyType
	{
		UIReturnKeyDefault = 0,
		UIReturnKeyGo = 1,
		UIReturnKeyGoogle = 2,
		UIReturnKeyJoin = 3,
		UIReturnKeyNext = 4,
		UIReturnKeyRoute = 5,
		UIReturnKeySearch = 6,
		UIReturnKeySend = 7,
		UIReturnKeyYahoo = 8,
		UIReturnKeyDone = 9,
		UIReturnKeyEmergencyCall = 10
	}

	public enum AndroidReturnKeyType
	{
		Unspecified = 0,
		None = 1,
		Go = 2,
		Search = 3,
		Send = 4,
		Next = 5,
		Done = 6,
		Previous = 7
	}

	public enum KeyboardType
	{
		UIKeyboardTypeDefault = 0,
		UIKeyboardTypeASCIICapable = 1,
		UIKeyboardTypeNumbersAndPunctuation = 2,
		UIKeyboardTypeURL = 3,
		UIKeyboardTypeNumberPad = 4,
		UIKeyboardTypePhonePad = 5,
		UIKeyboardTypeNamePhonePad = 6,
		UIKeyboardTypeEmailAddress = 7
	}

	public GameObject _callbackGameObject;

	public string _callbackSendMessageFunctionName;

	public string _callbackTextChangeMethodName;

	public string _callbackKeyboardHiddenFunctionName;

	public KeyboardType _keyboardType;

	public IOSReturnKeyType _iosReturnKeyType = IOSReturnKeyType.UIReturnKeySend;

	public AndroidReturnKeyType _androidReturnKeyType = AndroidReturnKeyType.Send;

	public int _maxStringLength = 20;

	private AndroidJavaClass _androidClass;

	private bool _pcOpen;

	private string _pcText = string.Empty;

	private void Awake()
	{
		if (!(UnityEngine.Application.platform == UnityEngine.RuntimePlatform.Android))
		{
			return;
		}
		_androidClass = new AndroidJavaClass("com.explodingbarrel.keyboard.KeyboardActivity");
		_androidClass.CallStatic("initializeEBKeyboard", base.transform.name, "OnKeyboardSendClicked", "OnKeyboardHidden", (int)_keyboardType, (int)_androidReturnKeyType);
	}

	public void DisplayKeyboard()
	{
		if (_androidClass == null)
		{
			_pcOpen = true;
			return;
		}
		_androidClass.CallStatic("displayEBKeyboard");
	}

	private void OnGUI()
	{
		if (!_pcOpen)
		{
			return;
		}
		float w = Mathf.Min(600f, Screen.width - 40f);
		Rect r = new Rect((Screen.width - w) / 2f, Screen.height * 0.25f, w, 40f);
		GUI.SetNextControlName("pc_keyboard");
		_pcText = GUI.TextField(r, _pcText, _maxStringLength);
		GUI.FocusControl("pc_keyboard");
		OnKeyboardTextChangeClicked(_pcText);
		bool enter = Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter);
		if (enter || GUI.Button(new Rect(r.x + w - 120f, r.y + 48f, 120f, 36f), "OK"))
		{
			_pcOpen = false;
			OnKeyboardSendClicked(_pcText);
			OnKeyboardHidden(_pcText);
		}
	}

	public void HideKeyboard()
	{
		_pcOpen = false;
	}

	public void OnKeyboardTextChangeClicked(string message)
	{
		if (!(_callbackGameObject == null))
		{
			_callbackGameObject.SendMessage(_callbackTextChangeMethodName, message, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void OnKeyboardSendClicked(string message)
	{
		if (!(_callbackGameObject == null))
		{
			_callbackGameObject.SendMessage(_callbackSendMessageFunctionName, message, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void OnKeyboardHidden(string message)
	{
		if (!(_callbackGameObject == null))
		{
			_callbackGameObject.SendMessage(_callbackKeyboardHiddenFunctionName, message, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void GetKeyboardArea(out CGRect area)
	{
		area = default(CGRect);
	}
}
