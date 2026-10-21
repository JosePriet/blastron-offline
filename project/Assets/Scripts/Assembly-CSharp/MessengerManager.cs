using System.Collections;
using EB;
using EB.Sparx;
using UnityEngine;

public class MessengerManager : SubSystem
{
	public int Count { get; set; }

	public override void Async(string message, object payload)
	{
		switch (message.ToLower())
		{
		case "count":
			Count = int.Parse(payload.ToString());
			NotifyChanged();
			break;
		case "sync":
			FetchCounts();
			break;
		}
	}

	public override void Initialize(Config config)
	{
	}

	public override IEnumerator LoadOfflineData()
	{
		Count = 0;
		yield break;
	}

	public override void Connect()
	{
		base.State = SubSystemState.Connected;
		Count = 0;
		FetchCounts();
	}

	public void LoadMotd(Action<Hashtable> callback)
	{
		EndPoint apiEndPoint = base.Hub.ApiEndPoint;
		Request request = apiEndPoint.Get("/messenger/motd");
		apiEndPoint.Service(request, (Response res) =>
		{
			if (res.sucessful)
			{
				callback(res.hashtable);
			}
		});
	}

	public override void Disconnect(bool isLogout)
	{
		Count = 0;
	}

	public void FetchCounts()
	{
		EndPoint apiEndPoint = base.Hub.ApiEndPoint;
		Request request = apiEndPoint.Get("/messenger/count");
		apiEndPoint.Service(request, (Response res) =>
		{
			if (res.sucessful)
			{
				Count = (int)res.number;
				NotifyChanged();
			}
		});
	}

	private void NotifyChanged()
	{
		ScreenManager.Instance.BroadcastMessage("OnMessageCountChanged", SendMessageOptions.DontRequireReceiver);
	}
}
