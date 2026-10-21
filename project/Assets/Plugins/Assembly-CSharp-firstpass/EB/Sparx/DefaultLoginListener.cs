using System.Collections;

namespace EB.Sparx
{
	public class DefaultLoginListener : LoginListener
	{
		public void ShowLoginScreen(LoginScreenFlags flags, Hashtable data)
		{
			if ((flags & LoginScreenFlags.NameSelect) != 0)
			{
				Hub.Instance.LoginManager.SelectName("newb" + Hub.Instance.LoginManager.LocalUserId);
			}
			else
			{
				Hub.Instance.LoginManager.LoginGuest();
			}
		}

		public void CloseLoginScreen()
		{
		}

		public void EnableInput(bool enabled)
		{
		}

		public void OnLoggedIn()
		{
			Util.BroadcastMessage("OnLoggedIn");
		}

		public void OnLoginFailed(string error, bool silent, bool fatal)
		{
			Util.BroadcastMessage("OnLoginFailed", error);
		}

		public void OnDisconnected(string error)
		{
			Util.BroadcastMessage("OnDisconnected", error);
		}

		public void OnOfflineDataLoaded()
		{
			Util.BroadcastMessage("OnOfflineDataLoaded");
		}

		public void OnNewVersionRequired(string storeUrl)
		{
			Util.BroadcastMessage("OnNewVersionRequired", storeUrl);
		}
	}
}
