using System;
using System.Collections;
using EB;
using EB.Sparx;

public class TournamentManager : SubSystem
{
	public class TournamentInfo
	{
		public string name;

		public string message;

		public int starts;

		public int ends;

		public int rank;

		public GameAttributes.GameModeType tournamentType = GameAttributes.GameModeType.Any;

		public TournamentInfo(object data)
		{
			name = Dot.String("name", data, string.Empty);
			message = Dot.String("message", data, string.Empty);
			starts = Dot.Integer("starts", data, 0);
			ends = Dot.Integer("ends", data, 0);
			rank = Dot.Integer("rank", data, 0);
			string value = Dot.String("type", data, string.Empty);
			if (Enum.IsDefined(typeof(GameAttributes.GameModeType), value))
			{
				tournamentType = (GameAttributes.GameModeType)(int)Enum.Parse(typeof(GameAttributes.GameModeType), value);
			}
		}
	}

	public TournamentInfo Active { get; private set; }

	public override void Initialize(Config config)
	{
	}

	public override IEnumerator LoadOfflineData()
	{
		yield break;
	}

	public override void Connect()
	{
		if (base.Hub.LoginManager.LoginData != null)
		{
			OnFetch(string.Empty, Dot.Object("tournaments", base.Hub.LoginManager.LoginData, null));
		}
		else
		{
			Fetch();
		}
		base.State = SubSystemState.Connected;
	}

	public override void Disconnect(bool isLogout)
	{
		Active = null;
	}

	private void OnFetch(string err, object data)
	{
		Active = null;
		if (data != null)
		{
			Active = new TournamentInfo(data);
			Debug.Log("There is a active tournament: " + Active.name);
		}
	}

	private void Fetch()
	{
		Request request = base.Hub.ApiEndPoint.Get("/tournaments/active");
		base.Hub.ApiEndPoint.Service(request, (Response res) =>
		{
			if (res.sucessful)
			{
				OnFetch(string.Empty, res.result);
			}
		});
	}

	public override void Async(string message, object payload)
	{
		base.Async(message, payload);
		switch (message.ToLower())
		{
		case "addsoft":
		{
			int value2 = Dot.Integer("value", payload, 0);
			Profile.Instance.AddSoftCurrency(value2);
			break;
		}
		case "addxp":
		{
			int value = Dot.Integer("value", payload, 0);
			Profile.Instance.AddSessionXP(value);
			break;
		}
		case "fetch":
		{
			int ms = Dot.Integer("value", payload, 0);
			Coroutines.SetTimeout(Fetch, ms);
			break;
		}
		}
	}

	public void Report(Hashtable scores, Hashtable attributes, Hashtable playerAttributes)
	{
		Request request = base.Hub.ApiEndPoint.Post("/tournaments/report");
		request.AddData("scores", scores);
		request.AddData("attributes", attributes);
		request.AddData("player", playerAttributes);
		request.AddData("nonce", Nonce.Generate());
		base.Hub.ApiEndPoint.Service(request, (Response res) =>
		{
		});
	}
}
