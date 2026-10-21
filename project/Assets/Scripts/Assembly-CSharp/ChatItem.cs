using EB;
using EB.Sparx;
using UnityEngine;

public class ChatItem : MonoBehaviour
{
	private UILabel _chatTextLabel;

	private UISprite _muteIcon;

	private GameObject _muteButton;

	private Id _userId;

	private bool _muted;

	private void Awake()
	{
		_muteButton = Util.GetObjectExactMatch(base.gameObject, "MuteButton");
		_chatTextLabel = Util.GetObjectExactMatch(base.gameObject, "ChatText").GetComponent<UILabel>();
		_muteIcon = Util.GetObjectExactMatch(_muteButton, "MuteIcon").GetComponent<UISprite>();
		ButtonScript component = _muteButton.GetComponent<ButtonScript>();
		component.onButtonClick += OnMuteClicked;
	}

	public void SetData(string texturePath, string playerName, string message, Color textColor, Id userId)
	{
		_userId = userId;
		_muted = Profile.Instance.UserMuted(_userId.Value);
		Refresh();
		UILabel component = Util.GetObjectExactMatch(base.gameObject, "NameLabel").GetComponent<UILabel>();
		component.text = playerName;
		component.color = textColor;
		_chatTextLabel.text = message;
		if (!string.IsNullOrEmpty(texturePath))
		{
			TexturePoolManager.Instance.LoadTexture(texturePath, SafeAction.Wrap(this, (Texture2D texture) =>
			{
				UITexture component2 = Util.GetObjectExactMatch(base.gameObject, "Portrait").GetComponent<UITexture>();
				component2.mainTexture = texture;
			}));
		}
		_muteButton.SetActive(_userId != Hub.Instance.GameManager.Game.LocalPlayer.UserId);
	}

	private void OnMuteClicked(GameObject caller)
	{
		if (!(_userId == Hub.Instance.GameManager.Game.LocalPlayer.UserId))
		{
			Profile.Instance.MuteUser(_userId, !_muted);
			Refresh();
			ChatUI.Instance.SetChatItemsMuteStatus();
		}
	}

	public void Refresh()
	{
		_muted = Profile.Instance.UserMuted(_userId.Value);
		_chatTextLabel.password = _muted;
		_muteIcon.spriteName = ((!_muted) ? "chat_block_icon" : "chat_cancel_icon");
	}
}
