using System;
using System.Collections.Generic;
using EB;
using EB.Sparx;
using UnityEngine;

public class ChatUI : MonoBehaviour
{
	public GameObject _chatHistory;

	public GameObject _chatHistoryPanel;

	public GameObject _chatMessageHandler;

	public GameObject _phoneInputContainer;

	public Collider[] _colliders;

	public GameObject _chatItemPrefab;

	private List<ChatItem> _chatList = new List<ChatItem>();

	private UIPanel[] _allPanels;

	private UIGrid grid;

	private bool _isShown;

	private UIInput _phoneChatInput;

	private GameObject _chatButton;

	private UIPanel _chatButtonPanel;

	private GameObject _unreadChatCount;

	private int _nChatsUnread;

	private float _widgetSize;

	private GameObject _movable;

	private GameObject _closeCatcher;

	public static ChatUI Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
		_chatHistory = Util.GetObjectExactMatch(base.gameObject, "ChatHistory");
		_chatHistoryPanel = Util.GetObjectExactMatch(_chatHistory, "SwipablePanel");
		_phoneInputContainer = Util.GetObjectExactMatch(base.gameObject, "PhoneChatInput");
		_allPanels = Util.FindAllComponents<UIPanel>(base.gameObject);
		grid = Util.FindComponent<UIGrid>(base.gameObject);
		_phoneChatInput = Util.FindComponent<UIInput>(Util.GetObjectExactMatch(_phoneInputContainer, "PhoneChatInput"));
		_chatButton = Util.GetObjectExactMatch(base.gameObject, "ChatButton");
		ButtonScript component = _chatButton.GetComponent<ButtonScript>();
		component.onButtonClick += OnChatButtonClicked;
		_chatButtonPanel = Util.FindComponent<UIPanel>(_chatButton);
		_unreadChatCount = Util.GetObjectExactMatch(_chatButton, "Unread");
		_movable = Util.GetObjectExactMatch(base.gameObject, "Movable");
		_closeCatcher = Util.GetObjectExactMatch(base.gameObject, "MainScreen");
		UIEventListener uIEventListener = UIEventListener.Get(_closeCatcher);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnChatButtonClicked));
	}

	public void Start()
	{
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "BackgroundFill");
		_widgetSize = objectExactMatch.transform.localScale.x;
		MoveOntoScreen(false, true);
	}

	public void OnChatButtonClicked(GameObject caller)
	{
		if (GameHUD.Instance.Visible)
		{
			if (_isShown)
			{
				TransitionScreen(false);
				return;
			}
			GameHUD.Instance.ToggleSubscreen(HUDSubscreenManager.Subscreen.None);
			TransitionScreen(true);
		}
	}

	public void TransitionScreen(bool bShouldShow)
	{
		if (_isShown != bShouldShow)
		{
			_isShown = bShouldShow;
			MoveOntoScreen(_isShown);
		}
	}

	public void MoveOntoScreen(bool bOnScreen, bool skipTransitions = false)
	{
		Vector3 localPosition = _movable.transform.localPosition;
		if (bOnScreen)
		{
			localPosition.x += _widgetSize;
			Show();
			_movable.transform.localPosition = localPosition;
			if (skipTransitions)
			{
			}
		}
		else
		{
			localPosition.x -= _widgetSize;
			Hide();
			_movable.transform.localPosition = localPosition;
			if (skipTransitions)
			{
			}
		}
	}

	public bool isShown()
	{
		return _isShown;
	}

	private void ToggleChatButton(bool bOnScreen)
	{
		_nChatsUnread = 0;
		RefreshButtonCount();
	}

	private void RefreshButtonCount()
	{
		if (_nChatsUnread > 0 && !_unreadChatCount.activeSelf)
		{
			_unreadChatCount.SetActive(true);
		}
		if (_nChatsUnread == 0 && _unreadChatCount.activeSelf)
		{
			_unreadChatCount.SetActive(false);
		}
		Util.GetObjectExactMatch(_unreadChatCount, "Unread_Count").GetComponent<UILabel>().text = _nChatsUnread.ToString();
	}

	public void AddMessage(string texturePath, string name, string msg, bool isLocalPlayer, Color textColor, Id userId)
	{
		GameObject gameObject = NGUITools.AddChild(grid.gameObject, _chatItemPrefab);
		gameObject.name = string.Format("ChatItem{0:D4}", _chatList.Count);
		ChatItem component = gameObject.GetComponent<ChatItem>();
		component.SetData(texturePath, name, msg, textColor, userId);
		_chatList.Add(component);
		grid.Reposition();
		if (!_isShown && !Profile.Instance.UserMuted(userId.Value))
		{
			_nChatsUnread++;
			RefreshButtonCount();
		}
	}

	public void OnTextChange(string message)
	{
		if (_phoneChatInput != null)
		{
			_phoneChatInput.text = message;
		}
	}

	private void OnSubmit(string inputString)
	{
		_chatMessageHandler.SendMessage("SendChatMessage", inputString, SendMessageOptions.RequireReceiver);
		OnTextChange(string.Empty);
	}

	private void Show()
	{
		GameHUD.Instance.ToggleHudChatVisibility(false);
		UIPanel[] allPanels = _allPanels;
		foreach (UIPanel uIPanel in allPanels)
		{
			uIPanel.enabled = true;
		}
		Collider[] colliders = _colliders;
		foreach (Collider collider in colliders)
		{
			collider.enabled = true;
		}
		ToggleChatItemColliders(true);
		grid.Reposition();
		ToggleChatButton(true);
	}

	private void Hide()
	{
		GameHUD.Instance.ToggleHudChatVisibility(true);
		UIPanel[] allPanels = _allPanels;
		foreach (UIPanel uIPanel in allPanels)
		{
			uIPanel.enabled = false;
		}
		_chatButtonPanel.enabled = true;
		Collider[] colliders = _colliders;
		foreach (Collider collider in colliders)
		{
			collider.enabled = false;
		}
		ToggleChatItemColliders(false);
		ToggleChatButton(false);
	}

	private void ToggleChatItemColliders(bool bEnabled)
	{
		Collider[] array = Util.FindAllComponents<Collider>(grid.gameObject);
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			collider.enabled = bEnabled;
		}
	}

	public void SetChatItemsMuteStatus()
	{
		foreach (ChatItem chat in _chatList)
		{
			chat.Refresh();
		}
	}
}
