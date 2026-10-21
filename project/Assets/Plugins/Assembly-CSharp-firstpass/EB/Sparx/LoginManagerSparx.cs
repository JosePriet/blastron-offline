using System;
using System.Collections;

namespace EB.Sparx
{
	public class LoginManagerSparx : LoginManager
	{
		private LoginAPI _api;

		public override void Initialize(Config config)
		{
			base.Initialize(config);
			_api = new LoginAPI(base.Hub.ApiEndPoint);
		}

		public override void Login(bool silent)
		{
			LoginAndroid(silent);
		}

		public override void ForgotPassword(string email)
		{
			throw new NotImplementedException();
		}

		public override void MergeAccount(string email, string password, Hashtable data)
		{
			throw new NotImplementedException();
		}

		public override void LoginEmail(string email, string password)
		{
			_config.Listener.EnableInput(false);
			_api.LoginEmail(email, password, base.OnLoginResult);
		}

		public override void CreateAccount(string email, string password, string password2, Hashtable data)
		{
			_config.Listener.EnableInput(false);
			_api.CreateAccount(email, password, password2, base.OnLoginResult);
		}

		public override void LoginGuest()
		{
			_config.Listener.EnableInput(false);
			_api.CreateTempAccount(base.OnLoginResult);
		}

		public override void SelectName(string name)
		{
			_config.Listener.EnableInput(false);
			_api.SelectName(name, base.OnLoginResult);
		}

		private void LoginStandAlone(bool silent)
		{
			Debug.Log("LoginStandAlone");
			ShowLoginScreen();
		}

		private void LoginAndroid(bool silent)
		{
			if (silent)
			{
				_api.LoginUDID(OnLoginUDID);
			}
			else
			{
				ShowLoginScreen();
			}
		}

		private void OnLoginUDID(string error, Hashtable data)
		{
			Hashtable hashtable = Dot.Object("user", data, null);
			if (hashtable != null)
			{
				OnLoginResult(null, data);
			}
			else
			{
				ShowLoginScreen();
			}
		}

		public override void Logout()
		{
			base.Hub.Disconnect(true);
		}

		public override void LoginFacebook()
		{
			base.Hub.FacebookManager.Authorize(base.IsSilentLoginFlow, OnFacebookAuthorize);
		}

		private void OnFacebookAuthorize(string err)
		{
			if (!string.IsNullOrEmpty(err))
			{
				_config.Listener.OnLoginFailed(err, base.IsSilentLoginFlow, false);
			}
			else
			{
				_api.LoginFacebook(base.Hub.FacebookManager.AccessToken, base.OnLoginResult);
			}
		}
	}
}
