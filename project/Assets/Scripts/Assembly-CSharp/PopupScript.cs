using EB;
using UnityEngine;

public class PopupScript : MonoBehaviour
{
	public delegate void onScreenClose(PopupScript ps);

	public bool showCurrencyBar;

	public bool showCloseButton = true;

	public GameObject parentScreen;

	public event onScreenClose onClosed;

	public void SetParentScreen(GameObject parentScreen)
	{
		this.parentScreen = parentScreen;
	}

	public void ClosePopup()
	{
		OnClose();
	}

	protected virtual void OnAwake()
	{
	}

	protected virtual void OnStart()
	{
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "CurrencyBar");
		if (objectExactMatch != null)
		{
			objectExactMatch.SetActive(showCurrencyBar);
		}
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "CloseButton");
		if (objectExactMatch2 != null)
		{
			objectExactMatch2.SetActive(showCloseButton);
			ButtonScript buttonScript = Util.FindComponent<ButtonScript>(objectExactMatch2);
			buttonScript.onButtonClick += Close;
		}
		GameObject objectExactMatch3 = Util.GetObjectExactMatch(base.gameObject, "LabelPopupTitle");
		UILabel l = Util.FindComponent<UILabel>(objectExactMatch3);
		UIUtils.ResizeLabelForText(l);
	}

	protected virtual void OnUpdate()
	{
	}

	protected virtual void HandleOpenPopup()
	{
		EBUI_TransitionInfo componentInChildren = base.gameObject.GetComponentInChildren<EBUI_TransitionInfo>();
		if (componentInChildren != null)
		{
			EBUI_TransitionManager.Instance.RunIntroTransitions(base.gameObject, SafeAction.Wrap(this, OnTransitionInComplete));
		}
		else
		{
			OnTransitionInComplete();
		}
	}

	protected virtual void OnClose()
	{
		EBUI_TransitionInfo componentInChildren = base.gameObject.GetComponentInChildren<EBUI_TransitionInfo>();
		if (componentInChildren != null)
		{
			EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, SafeAction.Wrap(this, OnTransitionOutComplete));
		}
		else
		{
			OnTransitionOutComplete();
		}
	}

	private void OnBack(GameObject caller)
	{
		OnClose();
	}

	protected virtual void OnTransitionInComplete()
	{
	}

	protected virtual void OnTransitionOutComplete()
	{
		string text = GetType().ToString();
		if (text != "PopupScript")
		{
			ScreenManager.Instance.ClosePopup(text);
		}
		else
		{
			ScreenManager.Instance.ClosePopup();
		}
		if (onClosed != null)
		{
			onClosed(this);
		}
	}

	private void Start()
	{
		OnStart();
	}

	private void Awake()
	{
		OnAwake();
	}

	private void Update()
	{
		OnUpdate();
	}

	private void OnOpenPopup()
	{
		HandleOpenPopup();
	}

	protected virtual void Close(GameObject caller)
	{
		OnClose();
	}

	private void UpdateLabelContents()
	{
	}
}
