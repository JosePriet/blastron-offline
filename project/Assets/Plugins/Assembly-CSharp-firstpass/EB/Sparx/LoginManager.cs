using System;
using System.Collections;

namespace EB.Sparx
{
	public abstract class LoginManager : SubSystem
	{
		public const string kCachedUserKey = "user";

		public const string kIsGuestLogin = "guestlogin";

		public const string kUserCredentials = "credentials";

		public const string kIsFacebookLogin = "facebooklogin";

		protected LoginConfig _config;

		protected User _offlineUser;

		protected User _onlineUser;

		protected Hashtable _loginData;

		public User LocalUser
		{
			get
			{
				return _onlineUser ?? _offlineUser;
			}
		}

		public Id LocalUserId
		{
			get
			{
				if (LocalUser != null)
				{
					return LocalUser.Id;
				}
				return Id.Null;
			}
		}

		public LoginState LoginState { get; private set; }

		public Hashtable LoginData
		{
			get
			{
				return _loginData;
			}
		}

		public bool IsSilentLoginFlow { get; protected set; }

		public bool IsGuestAccount
		{
			get
			{
				if (_onlineUser != null)
				{
					return !_onlineUser.HasEmail;
				}
				return true;
			}
		}

		public override void Initialize(Config config)
		{
			_config = config.LoginConfig;
			LoginState = LoginState.Disconnected;
			if (_config.Listener == null)
			{
				throw new ApplicationException("Missing Login Listener");
			}
			_offlineUser = base.Hub.UserManager.GetUser(new Hashtable { 
			{
				"uid",
				Id.Offline
			} });
			_loginData = new Hashtable();
		}

		public override IEnumerator LoadOfflineData()
		{
			_offlineUser = base.Hub.UserManager.GetUser(new Hashtable { 
			{
				"uid",
				Id.Offline
			} });
			_offlineUser.Load("user");
			yield break;
		}

		public abstract void Login(bool silent);

		public abstract void LoginEmail(string email, string password);

		public abstract void LoginGuest();

		public abstract void LoginFacebook();

		public abstract void Logout();

		public abstract void ForgotPassword(string email);

		public abstract void MergeAccount(string email, string password, Hashtable data);

		public abstract void CreateAccount(string email, string password, string password2, Hashtable data);

		public abstract void SelectName(string name);

		public virtual void ChangeName()
		{
			_loginData = null;
			LoginScreenFlags flags = LoginScreenFlags.NameSelect;
			_config.Listener.ShowLoginScreen(flags, null);
		}

		public virtual void Upgrade()
		{
			IsSilentLoginFlow = false;
			_config.Listener.ShowLoginScreen(LoginScreenFlags.None, null);
		}

		protected Action<string, Hashtable> GetLoginHandler()
		{
			if (IsSilentLoginFlow)
			{
				return OnSilentLoginResult;
			}
			return OnLoginResult;
		}

		protected void ClearLoginData()
		{
			_loginData = new Hashtable();
		}

		protected void SetState(LoginState state)
		{
			LoginState loginState = LoginState;
			LoginState = state;
			if (loginState != state || state == LoginState.LoggedIn)
			{
				base.Hub.SendMessage("OnLoginStateChanged", state);
			}
		}

		protected void ShowLoginScreen()
		{
			SetState(LoginState.Authenticating);
			LoginScreenFlags loginScreenFlags = LoginScreenFlags.None;
			loginScreenFlags = LoginScreenFlags.AllowSkip;
			_config.Listener.ShowLoginScreen(loginScreenFlags, null);
		}

		protected void ShowNameSelect()
		{
			SetState(LoginState.NameSelect);
			LoginScreenFlags flags = LoginScreenFlags.NameSelect;
			_config.Listener.ShowLoginScreen(flags, null);
		}

		protected void ShowAccountMerge(Hashtable data)
		{
			SetState(LoginState.Authenticating);
			LoginScreenFlags flags = LoginScreenFlags.AccountMerge;
			_config.Listener.ShowLoginScreen(flags, data);
		}

		protected void ShowCreateAccount(Hashtable data)
		{
			SetState(LoginState.Authenticating);
			LoginScreenFlags flags = LoginScreenFlags.CreateAccount;
			_config.Listener.ShowLoginScreen(flags, data);
		}

		protected void OnSilentLoginResult(string error, Hashtable data)
		{
			_config.Listener.EnableInput(true);
			Hashtable hashtable = Dot.Object("user", data, null);
			if (!string.IsNullOrEmpty(error) || hashtable == null)
			{
				_config.Listener.OnLoginFailed(string.Empty, true, false);
			}
			else
			{
				OnLoginResult(null, data);
			}
		}

		protected void OnLoginResult(string error, Hashtable data)
		{
			_config.Listener.EnableInput(true);
			if (!string.IsNullOrEmpty(error))
			{
				if (Dot.Bool("merge", data, false))
				{
					ShowAccountMerge(data);
					return;
				}
				if (Dot.Bool("create", data, false))
				{
					ShowCreateAccount(data);
					return;
				}
				string text = Dot.String("url", data, null);
				if (!string.IsNullOrEmpty(text))
				{
					if (IsSilentLoginFlow)
					{
						_config.Listener.OnLoginFailed(error, IsSilentLoginFlow, Dot.Bool("fatal", data, false));
					}
					else
					{
						_config.Listener.OnNewVersionRequired(text);
					}
					return;
				}
				bool flag = Dot.Bool("fatal", data, false);
				if (flag || _onlineUser == null)
				{
					SecurePrefs.DeleteKey("credentials");
					SecurePrefs.DeleteKey("guestlogin");
					SecurePrefs.DeleteKey("facebooklogin");
				}
				if (_onlineUser != null && flag)
				{
					FatalError(error);
				}
				else
				{
					_config.Listener.OnLoginFailed(error, IsSilentLoginFlow, flag);
				}
			}
			else
			{
				Hashtable data2 = Dot.Object("user", data, null);
				_onlineUser = base.Hub.UserManager.GetUser(data2);
				_loginData = Dot.Object("data", data, _loginData);
				_offlineUser.Update(data2);
				_offlineUser.Save("user");
				BugReport.AddData("uid", _onlineUser.Id.ToString());
				if (_config.RequireName && (!LocalUser.HasName || !ProfanityFilter.IsOk(LocalUser.Name)) && !IsSilentLoginFlow)
				{
					SetState(LoginState.NameSelect);
					ShowNameSelect();
				}
				else
				{
					SecurePrefs.SetInt("guestlogin", LocalUser.IsGuest ? 1 : 0);
					SecurePrefs.SetInt("facebooklogin", (LocalUser.HasFacebookId && base.Hub.FacebookManager.HasAccessToken) ? 1 : 0);
					SetState(LoginState.LoggedIn);
				}
			}
		}

		public override void Async(string message, object payload)
		{
			switch (message.ToLower())
			{
			case "logout":
			{
				Debug.LogError("Got forced logout from the server {0}", payload);
				string id = "ID_SPARX_ERROR_UNKNOWN";
				if (payload != null)
				{
					string text = payload.ToString();
					if (Localizer.HasString(text))
					{
						id = text;
					}
				}
				FatalError(Localizer.GetString(id));
				break;
			}
			}
		}

		public override void Connect()
		{
			base.State = SubSystemState.Connected;
		}

		public override void Disconnect(bool isLogout)
		{
			_onlineUser = null;
			LoginState = LoginState.Disconnected;
		}
	}
}
