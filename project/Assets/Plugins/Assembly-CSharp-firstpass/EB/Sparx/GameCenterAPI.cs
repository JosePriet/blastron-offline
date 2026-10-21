namespace EB.Sparx
{
	public class GameCenterAPI
	{
		private EndPoint _ep;

		public GameCenterAPI(EndPoint ep)
		{
			_ep = ep;
		}

		public void SetGameCenterId(string gameCenterId, string gameCenterName, Action<string> callback)
		{
			Request request = _ep.Post("/gamecenter/id");
			request.AddData("gcid", gameCenterId);
			request.AddData("gcname", gameCenterName);
			_ep.Service(request, (Response result) =>
			{
				if (result.sucessful)
				{
					callback(null);
				}
				else
				{
					callback(result.localizedError);
				}
			});
		}
	}
}
