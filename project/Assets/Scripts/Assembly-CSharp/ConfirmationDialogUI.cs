using EB;
using EB.Sequence;
using UnityEngine;

public class ConfirmationDialogUI : PopupScript
{
	public enum ButtonIds
	{
		Back = 0,
		Left = 1,
		Right = 2
	}

	private Vector3 originalLeftButtonPos;

	private ButtonIds _pressedButtonId;

	public Action<ButtonIds> callback;

	private bool _enableInput = true;

	protected override void OnAwake()
	{
		base.OnAwake();
		showCurrencyBar = false;
		originalLeftButtonPos = Util.GetObjectExactMatch(base.gameObject, "ButtonLeft").transform.localPosition;
	}

	protected override void HandleOpenPopup()
	{
		base.HandleOpenPopup();
	}

	protected override void OnClose()
	{
		if (_enableInput)
		{
			CloseDialog(ButtonIds.Back);
		}
	}

	private void OnLeft(GameObject caller)
	{
		if (_enableInput)
		{
			CloseDialog(ButtonIds.Left);
		}
	}

	private void OnRight(GameObject caller)
	{
		if (_enableInput)
		{
			CloseDialog(ButtonIds.Right);
		}
	}

	public void SetParameters(bool showCloseButton, string description, string title, string leftButtonText, string rightButtonText)
	{
		base.showCloseButton = showCloseButton;
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "ButtonLeft");
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "ButtonRight");
		if (string.IsNullOrEmpty(rightButtonText))
		{
			objectExactMatch2.SetActive(false);
			Vector3 localPosition = objectExactMatch.transform.localPosition;
			localPosition.x = 0f;
			objectExactMatch.transform.localPosition = localPosition;
		}
		else
		{
			objectExactMatch2.SetActive(true);
			objectExactMatch.transform.localPosition = originalLeftButtonPos;
		}
		UIUtils.SetLabelContents(base.gameObject, "LabelPopupTitle", title);
		UIUtils.SetLabelContents(base.gameObject, "LabelDescription", description);
		UIUtils.SetLabelContents(objectExactMatch, "LabelLeft", leftButtonText);
		UIUtils.SetLabelContents(objectExactMatch2, "LabelRight", rightButtonText);
		Util.FindComponent<ButtonScript>(objectExactMatch).onButtonClick += OnLeft;
		Util.FindComponent<ButtonScript>(objectExactMatch2).onButtonClick += OnRight;
	}

	protected void CloseDialog(ButtonIds buttonID)
	{
		_pressedButtonId = buttonID;
		if (LevelData.Instance != null && LevelData.Instance.PauseGame)
		{
			LevelData.Instance.PauseGame = false;
		}
		base.OnClose();
	}

	protected override void OnTransitionInComplete()
	{
		EB.Sequence.Component.Activate(base.gameObject, "ConfirmationDialogUI", typeof(SequenceEvent_OnScreenOpened));
		if (ScreenManager.Instance.GetTransitionBlockerState())
		{
			ScreenManager.Instance.EnableTransitionBlocker(false);
		}
	}

	protected override void OnTransitionOutComplete()
	{
		base.OnTransitionOutComplete();
		if (callback != null)
		{
			callback(_pressedButtonId);
		}
		else if ((bool)parentScreen)
		{
			parentScreen.SendMessage("OnConfirmationDialogUIClosed", _pressedButtonId, SendMessageOptions.RequireReceiver);
		}
	}

	private void EnableInput(bool enable)
	{
		_enableInput = enable;
	}

	private void OnFrontEndTutorial()
	{
		CloseDialog(ButtonIds.Back);
	}
}
