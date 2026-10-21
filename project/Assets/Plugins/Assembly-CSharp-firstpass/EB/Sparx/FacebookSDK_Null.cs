using System;

namespace EB.Sparx
{
	public class FacebookSDK_Null : FacebookSDK
	{
		public void Authorize(bool silent, string[] scope, Action<string, string> callback)
		{
			throw new NotImplementedException();
		}

		public void Logout()
		{
		}

		public void Init(string appId)
		{
		}

		public void Invite(string message)
		{
		}

		public void Request(string to, string message, string dataStr, Action<string, object> callback)
		{
		}

		public void StreamPost(string title, string subTitle, string desc, string ImageURL, string LinkURL)
		{
		}

		public void Buy(string itemId)
		{
		}
	}
}
