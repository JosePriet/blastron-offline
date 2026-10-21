using EB.Sparx;

namespace EB.Replication
{
	public class GameListener : EB.Sparx.GameListener
	{
		public void OnJoinedGame(Game game)
		{
			Manager.SetGame(game);
			Util.BroadcastMessage("OnJoinedGame");
		}

		public void OnLeaveGame(Game game, string reason)
		{
			Manager.ClearGame();
			Util.BroadcastMessage("OnLeaveGame", reason);
		}

		public void OnUpdate(Game game)
		{
			Manager.Update();
		}

		public void OnPlayerJoined(Game game, Player player)
		{
			Util.BroadcastMessage("OnPlayerJoined", player);
		}

		public void OnPlayerLeft(Game game, Player player)
		{
			Manager.OnPlayerLeft(game, player);
			Util.BroadcastMessage("OnPlayerLeft", player);
		}

		public void OnAttributesUpdated(Game game)
		{
			Util.BroadcastMessage("OnAttributesUpdated");
		}

		public void OnReceive(Game game, Player player, Packet packet)
		{
			Manager.Receive((player != null) ? player.PlayerId : 0u, packet.Data);
		}

		public void OnGameStarted(Game game)
		{
			Util.BroadcastMessage("OnGameStarted");
		}

		public void OnGameEnded(Game game)
		{
			Util.BroadcastMessage("OnGameEnded");
		}

		public void OnJoinGameFailed(string err)
		{
			Util.BroadcastMessage("OnJoinGameFailed", err);
		}
	}
}
