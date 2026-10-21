using EB.Sparx;

public static class Telemetry
{
	public static int UserLevel
	{
		set
		{
			TelemetryManager.UserLevel = value;
		}
	}

	public static void Event(string name, int value = 1, object eventData = null)
	{
		TelemetryManager.Event(name, value, eventData);
	}

	public static void Flush()
	{
		if (Hub.Instance != null)
		{
			Hub.Instance.TelemetryManager.Flush();
		}
	}
}
