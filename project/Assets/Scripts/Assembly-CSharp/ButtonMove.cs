using UnityEngine;

public class ButtonMove : MonoBehaviour
{
	private PlayerControlByGUI _playerControl;

	private Vector3 totalDelta;

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
		if (!(_playerControl == null))
		{
			if (isDown)
			{
				_playerControl.InputStartMove();
				totalDelta = Vector3.zero;
			}
			else
			{
				_playerControl.InputEndMove();
			}
		}
	}

	private void OnDrag(Vector2 delta)
	{
		if (!(_playerControl == null))
		{
			Vector3 vector = new Vector3(delta.x, delta.y);
			totalDelta += vector;
			_playerControl.InputMoveDir(totalDelta);
		}
	}
}
