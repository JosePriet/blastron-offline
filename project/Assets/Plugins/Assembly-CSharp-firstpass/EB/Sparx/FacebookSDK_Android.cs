using System.Collections;
using UnityEngine;

namespace EB.Sparx
{
	public class FacebookSDK_Android : FacebookSDK
	{
		private readonly string kMessageHandlerName = "FacebookSDK_Android_Callbacks";

		private SparxFacebookSDK_Android Callbacks;

		private string AppId = string.Empty;

		private AndroidJavaObject CurrentActivity;

		public FacebookSDK_Android()
		{
			Debug.Log("!*!*!*Facebook ctr");
			GameObject gameObject = new GameObject(kMessageHandlerName, typeof(SparxFacebookSDK_Android));
			Object.DontDestroyOnLoad(gameObject);
			Callbacks = gameObject.GetComponent<SparxFacebookSDK_Android>();
			if (Callbacks != null)
			{
				Debug.Log("!*!*!*Facebook ctr: Success");
			}
			else
			{
				Debug.LogError("!*!*!*Facebook ctr: Failed - Couldn't construct callback object");
			}
		}

		public void Init(string appId)
		{
			Debug.Log("!*!*!*Facebook Init: appId = " + appId);
			if (!string.IsNullOrEmpty(appId))
			{
				if (string.IsNullOrEmpty(AppId))
				{
					AppId = appId;
					AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
					if (androidJavaClass != null)
					{
						CurrentActivity = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
						if (CurrentActivity != null)
						{
							if (CurrentActivity.Call<bool>("FacebookInit", new object[1] { AppId }))
							{
								Debug.Log("!*!*!*Facebook Init: Success");
							}
							else
							{
								Debug.LogError("!*!*!*Facebook Init: Failed - FacebookInit returned false. Check FacebookIAP stream for more info");
							}
						}
						else
						{
							Debug.LogError("!*!*!*Facebook Init: Failed - Could not get the currentActivity");
						}
					}
					else
					{
						Debug.LogError("!*!*!*Facebook Init: Failed - com.unity3d.player.UnityPlayer lookup failed");
					}
				}
				else
				{
					Debug.LogError("!*!*!*Facebook Init: Failed - Duplicate initialization");
				}
			}
			else
			{
				Debug.LogError("!*!*!*Facebook Init: Failed - appId is empty or null");
			}
		}

		public void Authorize(bool silent, string[] scope, Action<string, string> callback)
		{
			string text = string.Join(",", scope);
			Debug.Log("!*!*!*Facebook Authorize: silent = " + silent + " scope = " + text);
			if (CurrentActivity != null)
			{
				Callbacks.Emitter.Once("authorize", (object result) =>
				{
					string obj = Dot.String("err", result, null);
					string obj2 = Dot.String("access_token", result, null);
					callback(obj, obj2);
				});
				bool flag = !silent;
				if (CurrentActivity.Call<bool>("FacebookLogin", new object[5] { text, flag, kMessageHandlerName, "OnAuthorize", "OnAuthorizeFailed" }))
				{
					Debug.Log("!*!*!*Facebook Authorize: Pending - Login request is pending completion. Waiting for Callback");
				}
				else
				{
					Debug.LogError("!*!*!*Facebook Authorize: Failed - FacebookLogin returned false. Check FacebookIAP stream for more info");
				}
			}
			else
			{
				Debug.LogError("!*!*!*Facebook Authorize: Failed - CurrentActivity was null. Either Init was not called or it failed");
			}
		}

		public void Logout()
		{
			Debug.Log("!*!*!*Facebook Logout");
			if (CurrentActivity != null)
			{
				if (CurrentActivity.Call<bool>("FacebookLogout", new object[0]))
				{
					Debug.Log("!*!*!*Facebook Logout: Success");
				}
				else
				{
					Debug.LogError("!*!*!*Facebook Logout: Failed - FacebookLogout returned false. Check FacebookIAP stream for more info");
				}
			}
			else
			{
				Debug.LogError("!*!*!*Facebook Logout: Failed - CurrentActivity was null. Either Init was not called or it failed");
			}
		}

		public void Invite(string message)
		{
			Debug.Log("!*!*!*Facebook Invite: message = " + message);
			if (CurrentActivity != null)
			{
				Hashtable hashtable = new Hashtable();
				hashtable["message"] = message;
				hashtable["data"] = JSON.Stringify(new Hashtable { { "type", "invite" } });
				if (CurrentActivity.Call<bool>("FacebookUI", new object[5]
				{
					"apprequests",
					JSON.Stringify(hashtable),
					kMessageHandlerName,
					"OnInviteSuccess",
					"OnInviteFailed"
				}))
				{
					Debug.Log("!*!*!*Facebook Invite: Pending");
				}
				else
				{
					Debug.LogError("!*!*!*Facebook Invite: Failed - FacebookUI returned false. Check FacebookIAP stream for more info");
				}
			}
			else
			{
				Debug.LogError("!*!*!*Facebook Invite: Failed - CurrentActivity was null. Either Init was not called or it failed");
			}
		}

		public void Request(string to, string message, string dataStr, Action<string, object> callback)
		{
			Debug.Log("!*!*!*Facebook Request: to = " + to + " message = " + message + " dataStr = " + dataStr);
			if (CurrentActivity != null)
			{
				Hashtable hashtable = new Hashtable();
				hashtable["method"] = "apprequests";
				hashtable["message"] = message;
				hashtable["to"] = to;
				hashtable["data"] = dataStr;
				if (CurrentActivity.Call<bool>("FacebookUI", new object[5]
				{
					"apprequests",
					JSON.Stringify(hashtable),
					kMessageHandlerName,
					"OnRequestSuccess",
					"OnRequestFailed"
				}))
				{
					Debug.Log("!*!*!*Facebook Request: Pending");
				}
				else
				{
					Debug.LogError("!*!*!*Facebook Request: Failed - FacebookUI returned false. Check FacebookIAP stream for more info");
				}
			}
			else
			{
				Debug.LogError("!*!*!*Facebook Request: Failed - CurrentActivity was null. Either Init was not called or it failed");
			}
		}

		public void StreamPost(string title, string subTitle, string desc, string ImageURL, string LinkURL)
		{
			Debug.Log("!*!*!*Facebook StreamPost: title = " + title + " subTitle = " + subTitle + " desc = " + desc + " ImageURL = " + ImageURL + " LinkURL = " + LinkURL);
			if (CurrentActivity != null)
			{
				Hashtable hashtable = new Hashtable();
				hashtable["method"] = "feed";
				hashtable["name"] = title;
				hashtable["link"] = LinkURL;
				hashtable["caption"] = subTitle;
				hashtable["picture"] = ImageURL;
				hashtable["description"] = desc;
				if (CurrentActivity.Call<bool>("FacebookUI", new object[5]
				{
					"feed",
					JSON.Stringify(hashtable),
					kMessageHandlerName,
					"OnStreamPostSuccess",
					"OnStreamPostFailed"
				}))
				{
					Debug.Log("!*!*!*Facebook StreamPost: Pending");
				}
				else
				{
					Debug.LogError("!*!*!*Facebook StreamPost: Failed - FacebookUI returned false. Check FacebookIAP stream for more info");
				}
			}
			else
			{
				Debug.LogError("!*!*!*Facebook StreamPost: Failed - CurrentActivity was null. Either Init was not called or it failed");
			}
		}

		public void Buy(string itemId)
		{
		}
	}
}
