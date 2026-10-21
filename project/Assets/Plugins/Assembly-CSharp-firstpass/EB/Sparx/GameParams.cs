namespace EB.Sparx
{
	public class GameParams
	{
		public uint ConnId;

		public long GameId;

		public string Url;

		public GameListener Listener;

		public GameParams(object obj)
		{
			ConnId = (uint)Dot.Long("connId", obj, 0L);
			GameId = Dot.Long("gameId", obj, 0L);
			Url = Dot.String("url", obj, string.Empty);
		}
	}
}
