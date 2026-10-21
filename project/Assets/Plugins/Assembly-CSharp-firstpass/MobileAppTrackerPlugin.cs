using UnityEngine;

public class MobileAppTrackerPlugin : MonoBehaviour
{
	public string AdvertiserId = string.Empty;

	public string AppKey = string.Empty;

	public string AppKeyAndroid = string.Empty;

	public string BundleId = string.Empty;

	private static bool _initialized;

	private static AndroidJavaObject _mobileAppTracker;

	private void Start()
	{
		if (!(UnityEngine.Application.platform == UnityEngine.RuntimePlatform.Android) || _initialized || string.IsNullOrEmpty(AdvertiserId) || string.IsNullOrEmpty(AppKeyAndroid))
		{
			return;
		}
		AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		if (androidJavaClass == null)
		{
			Debug.LogError("No com.unity3d.player.UnityPlayer");
			return;
		}
		AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
		if (androidJavaObject == null)
		{
			Debug.LogError("No playerActivityContext");
			return;
		}
		_mobileAppTracker = new AndroidJavaObject("com.mobileapptracker.MobileAppTracker", androidJavaObject, AdvertiserId.Trim(), AppKeyAndroid.Trim());
		if (_mobileAppTracker == null)
		{
			Debug.LogError("Couldn't create MobileAppTrackerPlugin");
			return;
		}
		_mobileAppTracker.Call("setDebugMode", true);
		_mobileAppTracker.Call<int>("trackInstall", new object[0]);
		_initialized = true;
		Debug.LogError("MobileAppTrackerPlugin initialized");
	}

	private void Awake()
	{
		Object.DontDestroyOnLoad(base.gameObject);
	}

	public static void Track(string action)
	{
		if (_initialized)
		{
			_mobileAppTracker.Call("trackAction", action);
		}
	}

	public static void SetUserId(string uid)
	{
		if (_initialized)
		{
			_mobileAppTracker.Call("setUserId", uid);
		}
	}
}
