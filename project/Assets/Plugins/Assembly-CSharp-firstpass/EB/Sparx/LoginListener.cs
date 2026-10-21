using System.Collections;

namespace EB.Sparx
{
	public interface LoginListener
	{
		void ShowLoginScreen(LoginScreenFlags flags, Hashtable data);

		void CloseLoginScreen();

		void EnableInput(bool enabled);

		void OnLoggedIn();

		void OnLoginFailed(string error, bool silent, bool fatal);

		void OnDisconnected(string error);

		void OnOfflineDataLoaded();

		void OnNewVersionRequired(string storeUrl);
	}
}
