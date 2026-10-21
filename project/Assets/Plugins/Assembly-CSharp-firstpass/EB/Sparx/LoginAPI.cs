using System.Collections;

namespace EB.Sparx
{
	public class LoginAPI
	{
		private EndPoint _endPoint;

		public LoginAPI(EndPoint endpoint)
		{
			_endPoint = endpoint;
		}

		private void HandleLoginResult(Response result, Action<string, Hashtable> callback)
		{
			if (result.sucessful)
			{
				string value = Dot.String("stoken", result.hashtable, null);
				if (!string.IsNullOrEmpty(value))
				{
					_endPoint.AddData("stoken", value);
					BugReport.AddData("stoken", value);
				}
				callback(null, result.hashtable);
			}
			else
			{
				callback(result.localizedError, null);
			}
		}

		public void LoginEmail(string email, string password, Action<string, Hashtable> callback)
		{
			Debug.Log("LoginAPI: LoginEmail " + email);
			Request request = _endPoint.Post("/login/email");
			request.AddData("email", email);
			request.AddData("password", password);
			request.AddData("udid", Device.UniqueIdentifier);
			_endPoint.Service(request, (Response result) =>
			{
				HandleLoginResult(result, callback);
			});
		}

		public void CreateAccount(string email, string password, string password2, Action<string, Hashtable> callback)
		{
			Debug.Log("LoginAPI: Create Account" + email);
			Request request = _endPoint.Post("/login/new");
			request.AddData("email", email);
			request.AddData("password", password);
			request.AddData("password2", password2);
			request.AddData("udid", Device.UniqueIdentifier);
			_endPoint.Service(request, (Response result) =>
			{
				HandleLoginResult(result, callback);
			});
		}

		public void CreateTempAccount(Action<string, Hashtable> callback)
		{
			Debug.Log("LoginAPI: CreateTempAccount ");
			Request request = _endPoint.Post("/login/temp");
			request.AddData("udid", Device.UniqueIdentifier);
			_endPoint.Service(request, (Response result) =>
			{
				HandleLoginResult(result, callback);
			});
		}

		public void SelectName(string name, Action<string, Hashtable> callback)
		{
			Request request = _endPoint.Post("/login/name");
			request.AddData("name", name);
			_endPoint.Service(request, (Response result) =>
			{
				HandleLoginResult(result, callback);
			});
		}

		public void LoginUDID(Action<string, Hashtable> callback)
		{
			Debug.Log("LoginAPI: LoginUDID ");
			Request request = _endPoint.Post("/login/udid");
			request.AddData("udid", Device.UniqueIdentifier);
			_endPoint.Service(request, (Response result) =>
			{
				HandleLoginResult(result, callback);
			});
		}

		public void LoginFacebook(string access_token, Action<string, Hashtable> callback)
		{
			Request request = _endPoint.Post("/login/facebook");
			request.AddData("udid", Device.UniqueIdentifier);
			request.AddData("access_token", access_token);
			_endPoint.Service(request, (Response result) =>
			{
				HandleLoginResult(result, callback);
			});
		}

		public void LoginFacebookEditor(Action<string, Hashtable> callback)
		{
			Request request = _endPoint.Post("/facebook/editor");
			request.AddData("udid", Device.UniqueIdentifier);
			_endPoint.Service(request, (Response result) =>
			{
				HandleLoginResult(result, callback);
			});
		}

		public void GetFacebookAccessToken(Action<string, Hashtable> callback)
		{
			Request request = _endPoint.Get("/facebook/editor");
			request.AddData("udid", Device.UniqueIdentifier);
			_endPoint.Service(request, (Response result) =>
			{
				HandleLoginResult(result, callback);
			});
		}
	}
}
