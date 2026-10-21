using EB;
using EB.TestFlight;
using UnityEngine;

public class TestFlightPlugin : MonoBehaviour
{
	public string ApiToken = string.Empty;

	private void Start()
	{
		if (!string.IsNullOrEmpty(ApiToken) && Version.GetVersion() != "1.0.0")
		{
			Plugin.SetUDID();
			Plugin.TakeOff(ApiToken);
			Application.RegisterLogCallbackThreaded(Logger);
		}
		Object.DontDestroyOnLoad(base.gameObject);
	}

	private void Logger(string stack, string message, LogType type)
	{
		switch (type)
		{
		case LogType.Log:
			Plugin.Log("I: " + message + "\n" + stack);
			break;
		case LogType.Assert:
			Plugin.Log("A: " + message + "\n" + stack);
			break;
		case LogType.Warning:
			Plugin.Log("W: " + message + "\n" + stack);
			break;
		case LogType.Exception:
			Plugin.Log("EX: " + message + "\n" + stack);
			break;
		case LogType.Error:
			Plugin.Log("E: " + message + "\n" + stack);
			break;
		}
	}
}
