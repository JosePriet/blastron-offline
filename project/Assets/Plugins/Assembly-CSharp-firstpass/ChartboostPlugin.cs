using UnityEngine;

public class ChartboostPlugin : MonoBehaviour
{
	private static AndroidJavaClass _chartboost_wrapper;

	public string appId = string.Empty;

	public string appSignature = string.Empty;

	public bool showInterstitial;

	public string appIdAndroid = string.Empty;

	public string appSignatureAndroid = string.Empty;

	private bool _paused;

	public static ChartboostPlugin Instance { get; private set; }

	private void Start()
	{
		Instance = this;
		Object.DontDestroyOnLoad(base.gameObject);
		if (!(UnityEngine.Application.platform == UnityEngine.RuntimePlatform.Android) || string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(appSignature))
		{
			return;
		}
		_chartboost_wrapper = new AndroidJavaClass("com.explodingbarrel.chartboostwrapper.ChartboostWrapper");
		if (_chartboost_wrapper == null)
		{
			Debug.LogError("no com.explodingbarrel.chartboostwrapper.ChartboostWrapper");
			return;
		}
		Debug.LogError("_chartboost_wrapper");
		_chartboost_wrapper.CallStatic("StartSession", appIdAndroid.Trim(), appSignatureAndroid.Trim());
		Debug.LogError("Chartboost StartSession");
		if (showInterstitial)
		{
			_chartboost_wrapper.CallStatic("ShowInterstitial");
		}
		Debug.LogError("Chartboost initialized");
	}

	public void CacheInterstitial()
	{
		if (_chartboost_wrapper == null)
		{
			return;
		}
		_chartboost_wrapper.CallStatic("cacheInterstitial");
	}

	public void ShowInterstitial(string location)
	{
		if (_chartboost_wrapper == null)
		{
			return;
		}
		if (!string.IsNullOrEmpty(location))
		{
			_chartboost_wrapper.CallStatic("ShowInterstitial", location);
		}
		else
		{
			_chartboost_wrapper.CallStatic("ShowInterstitial");
		}
	}

	public void ShowMoreApps()
	{
		if (_chartboost_wrapper == null)
		{
			return;
		}
		_chartboost_wrapper.CallStatic("ShowMoreApps");
	}

	public void CacheMoreApps()
	{
		if (_chartboost_wrapper == null)
		{
			return;
		}
		_chartboost_wrapper.CallStatic("CacheMoreApps");
	}

	private void OnApplicationPause(bool pause)
	{
		if (!pause && _paused)
		{
			Start();
		}
		_paused = pause;
	}
}
