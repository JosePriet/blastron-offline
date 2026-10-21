namespace EB.Sparx
{
	public interface FacebookSDK
	{
		void Init(string appId);

		void Authorize(bool silent, string[] scope, Action<string, string> callback);

		void Logout();

		void Invite(string message);

		void Request(string to, string message, string dataStr, Action<string, object> callback);

		void StreamPost(string title, string subTitle, string desc, string ImageURL, string LinkURL);

		void Buy(string itemId);
	}
}
