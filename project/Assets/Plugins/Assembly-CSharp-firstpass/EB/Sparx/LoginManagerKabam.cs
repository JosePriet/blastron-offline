using System.Collections;

namespace EB.Sparx
{
	public class LoginManagerKabam : LoginManager
	{
		private KabamAPI _api;

		private string _naid = string.Empty;

		private string _country = string.Empty;

		public string Country
		{
			get
			{
				return _country;
			}
		}

		public override void Initialize(Config config)
		{
			base.Initialize(config);
			_api = new KabamAPI(base.Hub.ApiEndPoint, config.Locale);
		}

		private void OnSilentRegister(string error, Hashtable data)
		{
			_naid = Dot.String("naid", data, string.Empty);
			_country = Dot.String("country", data, string.Empty);
		}

		public void GetSupportUrl(Action<string, string> callback)
		{
			_api.GetSupportUrl((string err, object res) =>
			{
				callback(err, Dot.String("url", res, null));
			});
		}

		public override void Login(bool silent)
		{
			Debug.Log("Login: " + silent);
			base.IsSilentLoginFlow = silent;
			Coroutines.Run(Protect.CalculateSha1((string sha1) =>
			{
				_api._sha1 = sha1;
				_api.Register(OnRegister);
			}));
		}

		public override void LoginFacebook()
		{
			base.IsSilentLoginFlow = false;
			_config.Listener.EnableInput(false);
			base.Hub.FacebookManager.Authorize(base.IsSilentLoginFlow, OnFacebookAuthorize);
		}

		public override void MergeAccount(string email, string password, Hashtable data)
		{
			_config.Listener.EnableInput(false);
			_api.Login(_naid, email, password, data, GetLoginHandler());
		}

		public override void ForgotPassword(string email)
		{
			_config.Listener.EnableInput(false);
			_api.ForgotPassword(email, (string err, object obj1) =>
			{
				_config.Listener.EnableInput(true);
				if (!string.IsNullOrEmpty(err))
				{
					_config.Listener.OnLoginFailed(err, base.IsSilentLoginFlow, false);
				}
				else
				{
					_config.Listener.OnLoginFailed(Localizer.Format("ID_SPARX_ERROR_EMAIL_RESET", email), base.IsSilentLoginFlow, false);
				}
			});
		}

		public override void LoginEmail(string email, string password)
		{
			ClearLoginData();
			_config.Listener.EnableInput(false);
			SaveCredentials(email, password);
			_api.Login(_naid, email, password, null, base.OnLoginResult);
		}

		public override void CreateAccount(string email, string password, string password2, Hashtable data)
		{
			ClearLoginData();
			_config.Listener.EnableInput(false);
			SaveCredentials(email, password);
			_api.Upgrade(_naid, email, password, password2, data, base.OnLoginResult);
		}

		public override void LoginGuest()
		{
			ClearLoginData();
			_config.Listener.EnableInput(false);
			_api.LoginGuest(_naid, base.OnLoginResult);
		}

		public override void SelectName(string name)
		{
			_config.Listener.EnableInput(false);
			if (!ProfanityFilter.IsOk(name))
			{
				Coroutines.NextFrame(() =>
				{
					OnLoginResult("ID_SPARX_ERROR_PROFANITY", null);
				});
			}
			else
			{
				_api.SelectName(name, base.OnLoginResult);
			}
		}

		private void SaveCredentials(string email, string password)
		{
			Hashtable hashtable = new Hashtable();
			hashtable.Add("email", email);
			hashtable.Add("password", password);
			Hashtable json = hashtable;
			SecurePrefs.SetJSON("credentials", json);
			SecurePrefs.Save();
		}

		private void OnRegister(string error, Hashtable data)
		{
			if (!string.IsNullOrEmpty(error))
			{
				string text = Dot.String("url", data, null);
				if (text != null && !base.IsSilentLoginFlow)
				{
					_config.Listener.OnNewVersionRequired(text);
				}
				else
				{
					_config.Listener.OnLoginFailed(error, base.IsSilentLoginFlow, Dot.Bool("fatal", data, false));
				}
				return;
			}
			_naid = Dot.String("naid", data, _naid);
			_country = Dot.String("country", data, _country);
			_loginData = new Hashtable();
			if (string.IsNullOrEmpty(_naid))
			{
				_config.Listener.OnLoginFailed(Localizer.GetString("ID_SPARX_ERROR_UNKNOWN"), base.IsSilentLoginFlow, true);
				return;
			}
			string text2 = Dot.String("fbAppId", data, _naid);
			if (!string.IsNullOrEmpty(text2))
			{
				base.Hub.FacebookManager.SDK.Init(text2);
			}
			MobileAppTrackerPlugin.SetUserId(_naid);
			if (SecurePrefs.GetInt("facebooklogin", 0) != 0)
			{
				LoginFacebook();
				return;
			}
			string salt = Dot.String("salt", data, string.Empty);
			Coroutines.Run(Protect.CalculateHmac(salt, (string challenge) =>
			{
				_api._salt = salt;
				_api._chal = challenge;
				object jSON = SecurePrefs.GetJSON("credentials");
				string text3 = Dot.String("email", jSON, string.Empty);
				string text4 = Dot.String("password", jSON, string.Empty);
				if (!string.IsNullOrEmpty(text3) && !string.IsNullOrEmpty(text4))
				{
					_api.Login(_naid, text3, text4, null, GetLoginHandler());
				}
				else if (base.IsSilentLoginFlow)
				{
					_api.LoginGuest(_naid, GetLoginHandler());
				}
				else
				{
					ShowLoginScreen();
				}
			}));
		}

		public override void Disconnect(bool isLogout)
		{
			base.Disconnect(isLogout);
			_naid = string.Empty;
		}

		public override void Logout()
		{
			_naid = string.Empty;
			SecurePrefs.DeleteKey("credentials");
			SecurePrefs.DeleteKey("user");
			SecurePrefs.DeleteKey("guestlogin");
			SecurePrefs.DeleteKey("facebooklogin");
			base.Hub.Disconnect(true);
		}

		private void OnFacebookAuthorize(string err)
		{
			Debug.Log("Kabam:OnFacebookAuthorize {0}", err);
			if (!string.IsNullOrEmpty(err))
			{
				_config.Listener.EnableInput(true);
				_config.Listener.OnLoginFailed(err, base.IsSilentLoginFlow, false);
				return;
			}
			_config.Listener.EnableInput(false);
			object jSON = SecurePrefs.GetJSON("credentials");
			string text = null;
			if (jSON != null)
			{
				text = Dot.String("email", jSON, string.Empty);
				Debug.Log("Have email! " + text);
			}
			_api.LoginFacebook(_naid, base.Hub.FacebookManager.AccessToken, text, GetLoginHandler());
		}
	}
}
