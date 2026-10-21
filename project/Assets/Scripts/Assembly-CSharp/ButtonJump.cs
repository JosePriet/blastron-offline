using UnityEngine;

public class ButtonJump : MonoBehaviour
{
	private PlayerControlByGUI _playerControl;

	private void OnPlayerReady()
	{
		GameObject playerObject = LevelData.Instance.GetPlayerObject();
		if (playerObject != null)
		{
			_playerControl = playerObject.GetComponent<PlayerControlByGUI>();
		}
	}

	private void OnPress(bool isDown)
	{
		if (!(_playerControl == null) && isDown)
		{
			_playerControl.InputJump();
		}
	}

	private void OnClick()
	{
		if (_playerControl != null)
		{
			_playerControl.InputJump();
		}
	}
}
