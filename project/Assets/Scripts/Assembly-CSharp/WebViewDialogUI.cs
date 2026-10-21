using System.Collections;
using EB;
using EB.Sparx;
using UnityEngine;

public class WebViewDialogUI : PopupScript
{
	private GameObject _callbacks;

	protected override void OnAwake()
	{
		base.OnAwake();
		_callbacks = new GameObject("webview_callbacks", typeof(WebViewCallbacks));
		_callbacks.GetComponent<WebViewCallbacks>().parent = this;
	}

	private void CloseWebView()
	{
		AndroidJavaClass androidJavaClass = (UnityEngine.Application.platform == UnityEngine.RuntimePlatform.Android) ? new AndroidJavaClass("com.unity3d.player.UnityPlayer") : null;
		if (androidJavaClass != null)
		{
			AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
			if (androidJavaObject != null)
			{
				if (androidJavaObject.Call<bool>("WebViewPopupClose", new object[0]))
				{
					EB.Debug.Log("!*!*!*WebViewClose: Success");
				}
				else
				{
					EB.Debug.LogError("!*!*!*WebViewClose Failed - WebViewClose returned false. Check FacebookIAP stream for more info");
				}
			}
			else
			{
				EB.Debug.LogError("!*!*!*WebViewClose Init: Failed - Could not get the currentActivity");
			}
		}
		else
		{
			EB.Debug.LogError("!*!*!*WebViewClose Init: Failed - com.unity3d.player.UnityPlayer lookup failed");
		}
	}

	private void OnApplicationPause(bool pause)
	{
		if (pause)
		{
			OnClose();
		}
	}

	private void OnDestroy()
	{
		CloseWebView();
	}

	private void OnHover()
	{
		CloseWebView();
	}

	protected override void OnClose()
	{
		base.OnClose();
		CloseWebView();
	}

	public void OpenUrl(string title, string uri, Hashtable query)
	{
		UIRoot uIRoot = NGUITools.FindInParents<UIRoot>(base.gameObject);
		Transform transform = Util.GetObject(base.gameObject, "InnerBackground", true).transform;
		Vector3 localScale = transform.localScale;
		Vector3 localPosition = transform.localPosition;
		float num = uIRoot.manualHeight;
		float num2 = num * ((float)Screen.width / (float)Screen.height);
		Vector2 vector = new Vector2(localScale.x / num2, localScale.y / num);
		Vector2 vector2 = new Vector2(0.5f - vector.x * 0.5f - localPosition.x / num2, 0.5f - vector.y * 0.5f - localPosition.y / num);
		query = query ?? new Hashtable();
		query["stoken"] = Hub.Instance.ApiEndPoint.GetData("stoken");
		query["width"] = vector.x;
		query["height"] = vector.y;
		string text = Hub.Instance.ApiEndPoint.Url + uri + "?" + QueryString.Stringify(query);
		text = text.Replace("https://", "http://");
		if (!string.IsNullOrEmpty(title))
		{
			UIUtils.SetLabelContents(base.gameObject, "LabelPopupTitle", title);
		}
		AndroidJavaClass androidJavaClass = (UnityEngine.Application.platform == UnityEngine.RuntimePlatform.Android) ? new AndroidJavaClass("com.unity3d.player.UnityPlayer") : null;
		if (androidJavaClass != null)
		{
			AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
			if (androidJavaObject != null)
			{
				if (androidJavaObject.Call<bool>("WebViewPopup", new object[6] { text, 1024, vector2.x, vector2.y, vector.x, vector.y }))
				{
					UnityEngine.Debug.Log("!*!*!*WebViewPopup: Success");
				}
				else
				{
					UnityEngine.Debug.LogError("!*!*!*WebViewPopup Failed - WebViewPopup returned false. Check FacebookIAP stream for more info");
				}
			}
			else
			{
				UnityEngine.Debug.LogError("!*!*!*WebViewPopup Init: Failed - Could not get the currentActivity");
			}
		}
		else
		{
			UnityEngine.Debug.LogError("!*!*!*WebViewPopup Init: Failed - com.unity3d.player.UnityPlayer lookup failed");
		}
	}

	public static void TestOpenUrl(string title, string url, Vector2 pt, Vector2 size)
	{
		AndroidJavaClass androidJavaClass = (UnityEngine.Application.platform == UnityEngine.RuntimePlatform.Android) ? new AndroidJavaClass("com.unity3d.player.UnityPlayer") : null;
		if (androidJavaClass != null)
		{
			AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
			if (androidJavaObject != null)
			{
				if (androidJavaObject.Call<bool>("WebViewPopup", new object[6] { url, 1024, pt.x, pt.y, size.x, size.y }))
				{
					UnityEngine.Debug.Log("!*!*!*WebViewPopup: Success");
				}
				else
				{
					UnityEngine.Debug.LogError("!*!*!*WebViewPopup Failed - WebViewPopup returned false. Check FacebookIAP stream for more info");
				}
			}
			else
			{
				UnityEngine.Debug.LogError("!*!*!*WebViewPopup Init: Failed - Could not get the currentActivity");
			}
		}
		else
		{
			UnityEngine.Debug.LogError("!*!*!*WebViewPopup Init: Failed - com.unity3d.player.UnityPlayer lookup failed");
		}
	}

	public void OnLoad()
	{
		UnityEngine.Debug.LogError("OnLoad");
		Util.SetLayerRecursive(base.gameObject, "loading_nibbles", 0);
	}

	public void OnUrl(string url)
	{
		UnityEngine.Debug.LogError("OnUrl:" + url);
		Uri uri = new Uri(url);
		string path = uri.Path;
		Hashtable obj = QueryString.Parse(uri.Query);
		ScreenManager.Instance.ClosePopup();
		switch (path.ToLower())
		{
		case "/popup":
		{
			string text = Dot.String("name", obj, string.Empty);
			int num = Dot.Integer("fader", obj, 0);
			int num2 = Dot.Integer("prev", obj, 0);
			ScreenManager.Instance.OpenPopup(text, num2 != 0, num != 0);
			break;
		}
		case "/parts":
		{
			GameObject gameObject = ScreenManager.Instance.OpenPopup("MyRobotUI");
			gameObject.GetComponent<MyRobotUI>().showOwned = false;
			break;
		}
		case "/weapons":
		{
			GameObject obj2 = ScreenManager.Instance.OpenPopup("StoreWeaponUI");
			StoreWeaponUI storeWeaponUI = Util.FindComponent<StoreWeaponUI>(obj2);
			storeWeaponUI.startInStore = true;
			break;
		}
		case "/boosts":
			ScreenManager.Instance.OpenPopup("StoreWeaponUI");
			break;
		case "/web":
		{
			int page = Dot.Integer("page", obj, 0);
			HomeUI.OpenWebPage((HomeUI.PageType)page);
			break;
		}
		}
	}
}
