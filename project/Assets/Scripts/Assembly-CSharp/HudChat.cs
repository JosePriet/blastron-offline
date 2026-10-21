using System.Collections.Generic;
using EB;
using UnityEngine;

public class HudChat : MonoBehaviour
{
	private class ChatMessageInfo
	{
		public string name;

		public string message;

		public string shortenedMessage;

		public float timestamp;

		public Color textColor;

		public ChatMessageInfo(string name, string message, float timestamp, Color textColor)
		{
			this.name = name;
			this.message = message;
			this.timestamp = timestamp;
			this.textColor = textColor;
		}
	}

	private struct HudChatMessage
	{
		public UILabel Lbl;

		public HudChatMessage(UILabel lbl)
		{
			Lbl = lbl;
		}
	}

	public GameObject[] HudChatMessageObjects;

	public float messageOffset = -30f;

	public float maxChatTextWidthInPixels = 220f;

	private HudChatMessage[] _hudChatMessages;

	private List<ChatMessageInfo> _chatMessageInfo;

	public void AddMessage(string playerName, string message, Color textColor)
	{
		ChatMessageInfo item = new ChatMessageInfo(playerName, message, UnityEngine.Time.time, textColor);
		_chatMessageInfo.Add(item);
		while (_chatMessageInfo.Count > HudChatMessageObjects.Length)
		{
			_chatMessageInfo.RemoveAt(0);
		}
		RefreshChatTextContents();
	}

	private void RefreshChatTextContents()
	{
		for (int i = 0; i < HudChatMessageObjects.Length; i++)
		{
			int index = HudChatMessageObjects.Length - i - 1;
			int num = _chatMessageInfo.Count - i - 1;
			if (num >= 0 && num < _chatMessageInfo.Count)
			{
				ChatMessageInfo info = _chatMessageInfo[num];
				SetText(index, info);
				ToggleEnableText(index, true);
			}
			else
			{
				ToggleEnableText(index, false);
			}
		}
	}

	private void Start()
	{
		_chatMessageInfo = new List<ChatMessageInfo>();
		_hudChatMessages = new HudChatMessage[HudChatMessageObjects.Length];
		for (int i = 0; i < HudChatMessageObjects.Length; i++)
		{
			Vector3 localPosition = HudChatMessageObjects[i].transform.localPosition;
			HudChatMessageObjects[i].transform.localPosition = new Vector3(localPosition.x, localPosition.y + (float)i * messageOffset, localPosition.z);
			_hudChatMessages[i] = new HudChatMessage(GetChatMessageLbl(HudChatMessageObjects[i]));
			ToggleEnableText(i, false);
		}
	}

	private void Update()
	{
		bool flag = false;
		while (_chatMessageInfo.Count > 0 && UnityEngine.Time.time - _chatMessageInfo[0].timestamp >= Tuning.Instance.Chat_MessageDuration)
		{
			_chatMessageInfo.RemoveAt(0);
			flag = true;
		}
		if (flag)
		{
			RefreshChatTextContents();
		}
		float num = UnityEngine.Time.time - Tuning.Instance.Chat_MessageDuration + Tuning.Instance.Chat_MessageFadeTime;
		for (int i = 0; i < _chatMessageInfo.Count; i++)
		{
			int index = HudChatMessageObjects.Length - i - 1;
			int num2 = _chatMessageInfo.Count - i - 1;
			if (num2 >= 0 && num2 < _chatMessageInfo.Count)
			{
				ChatMessageInfo chatMessageInfo = _chatMessageInfo[num2];
				if (chatMessageInfo.timestamp < num)
				{
					float fadeRatio = 1f - (num - chatMessageInfo.timestamp) / Tuning.Instance.Chat_MessageFadeTime;
					SetChatColor(index, chatMessageInfo.textColor, fadeRatio);
				}
				else
				{
					SetChatColor(index, chatMessageInfo.textColor, 1f);
				}
			}
		}
	}

	private void SetChatColor(int index, Color textColor, float fadeRatio)
	{
		if (index >= 0 && index < _hudChatMessages.Length)
		{
			Color color = new Color(1f, 1f, 1f, fadeRatio);
			_hudChatMessages[index].Lbl.color = color;
		}
	}

	private UILabel GetChatMessageLbl(GameObject gameObject)
	{
		return Util.GetObject(gameObject, "ChatMessage", true).GetComponent<UILabel>();
	}

	private UILabel GetChatNameLbl(GameObject gameObject)
	{
		return Util.GetObject(gameObject, "Name", true).GetComponent<UILabel>();
	}

	private void SetText(int index, ChatMessageInfo info)
	{
		if (index < 0 || index >= _hudChatMessages.Length)
		{
			return;
		}
		if (info.shortenedMessage == null)
		{
			info.shortenedMessage = info.message;
			_hudChatMessages[index].Lbl.text = info.shortenedMessage;
			float x = UIUtils.GetLabelTextSize(_hudChatMessages[index].Lbl).x;
			if (x > maxChatTextWidthInPixels)
			{
				float num = x / (float)info.shortenedMessage.Length;
				int length = Mathf.FloorToInt(maxChatTextWidthInPixels / num) - 3;
				info.shortenedMessage = info.shortenedMessage.Substring(0, length) + "...";
			}
		}
		_hudChatMessages[index].Lbl.text = string.Format("[{0}]{1}: [{2}]{3}", NGUITools.EncodeColor(info.textColor), info.name, NGUITools.EncodeColor(Color.white), info.shortenedMessage);
	}

	private void ToggleEnableText(int index, bool enabled)
	{
		if (index >= 0 && index < _hudChatMessages.Length)
		{
			_hudChatMessages[index].Lbl.enabled = enabled;
		}
	}

	public void ToggleVisibility(bool bVisible)
	{
		GameObject[] hudChatMessageObjects = HudChatMessageObjects;
		foreach (GameObject gameObject in hudChatMessageObjects)
		{
			gameObject.SetActive(bVisible);
		}
	}
}
