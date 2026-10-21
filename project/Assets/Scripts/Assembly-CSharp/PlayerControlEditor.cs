using UnityEngine;

public class PlayerControlEditor : MonoBehaviour
{
	private Player _player;

	private void Start()
	{
		_player = GetComponent<Player>();
	}

	private void Update()
	{
		if (!base.networkView || base.networkView.isMine)
		{
			Player.PlayerInstructions? playerInstructions = UpdateController();
			if (!playerInstructions.HasValue)
			{
				playerInstructions = UpdateKeyboard();
			}
			if (playerInstructions.HasValue)
			{
				_player.Move2Ancillary(playerInstructions.Value);
			}
		}
	}

	private Player.PlayerInstructions? UpdateKeyboard()
	{
		return null;
	}

	private Player.PlayerInstructions? UpdateController()
	{
		return null;
	}
}
