using System.Collections;
using System.Collections.Generic;

namespace EB.Sparx
{
	public class FacebookManager : SubSystem
	{
		private FacebookAPI _api;

		private FacebookSDK _sdk;

		private FacebookConfig _config;

		private Dictionary<string, FacebookUser> _users;

		private string _accessToken = string.Empty;

		private List<FacebookUser> _friends;

		private List<FacebookUser> _appFriends;

		private FacebookUser _me;

		public string AccessToken
		{
			get
			{
				return _accessToken;
			}
		}

		public bool HasAccessToken
		{
			get
			{
				return !string.IsNullOrEmpty(_accessToken);
			}
		}

		public FacebookUser[] Friends
		{
			get
			{
				return _friends.ToArray();
			}
		}

		public FacebookUser[] AppFriends
		{
			get
			{
				return _appFriends.ToArray();
			}
		}

		public FacebookUser LocalUser
		{
			get
			{
				return _me;
			}
		}

		public FacebookAPI API
		{
			get
			{
				return _api;
			}
		}

		public FacebookSDK SDK
		{
			get
			{
				return _sdk;
			}
		}

		public override void Initialize(Config config)
		{
			_config = config.FacebookConfig;
			_sdk = new FacebookSDK_Android();
			_users = new Dictionary<string, FacebookUser>();
			_friends = new List<FacebookUser>();
			_appFriends = new List<FacebookUser>();
		}

		public FacebookUser GetUser(Id id)
		{
			foreach (FacebookUser value in _users.Values)
			{
				if (value.UserId == id)
				{
					return value;
				}
			}
			return null;
		}

		public void Invite(string message)
		{
			_sdk.Invite(message);
		}

		public override IEnumerator LoadOfflineData()
		{
			yield break;
		}

		public void Authorize(bool silent, Action<string> callback)
		{
			_sdk.Authorize(silent, _config.Scope, (string err, string access_token) =>
			{
				if (!string.IsNullOrEmpty(err))
				{
					callback(err);
				}
				else if (string.IsNullOrEmpty(access_token))
				{
					callback("ID_SPARX_ERROR_FACEBOOK");
				}
				else
				{
					_accessToken = access_token;
					_api = new FacebookAPI(access_token);
					callback(null);
				}
			});
		}

		public void FetchUsers(List<string> fbIds, Action<List<FacebookUser>> cb)
		{
			Request request = base.Hub.ApiEndPoint.Post("/login/facebook/lookup");
			request.AddData("ids", fbIds);
			base.Hub.ApiEndPoint.Service(request, (Response res) =>
			{
				if (res.sucessful && res.arrayList != null)
				{
					List<FacebookUser> list = new List<FacebookUser>();
					foreach (Hashtable array in res.arrayList)
					{
						FacebookUser facebookUser = LoadUser(array);
						if (facebookUser.UserId.Valid)
						{
							list.Add(facebookUser);
						}
					}
					cb(list);
				}
			});
		}

		public void FetchSparxUsers(List<long> sparxIds, Action<string, List<FacebookUser>> cb)
		{
			Request request = base.Hub.ApiEndPoint.Post("/login/facebook/lookupSparx");
			request.AddData("ids", sparxIds.ToArray());
			base.Hub.ApiEndPoint.Service(request, (Response res) =>
			{
				List<FacebookUser> list = null;
				if (res.sucessful && res.arrayList != null)
				{
					list = new List<FacebookUser>();
					foreach (Hashtable array in res.arrayList)
					{
						FacebookUser facebookUser = LoadUser(array);
						if (facebookUser != null && facebookUser.UserId.Valid)
						{
							list.Add(facebookUser);
						}
					}
				}
				cb(res.localizedError, list);
			});
		}

		public override void Connect()
		{
			if (_api == null)
			{
				base.State = SubSystemState.Connected;
			}
			else
			{
				FetchMe();
			}
		}

		public override void Disconnect(bool isLogout)
		{
			if (isLogout)
			{
				_sdk.Logout();
				_accessToken = string.Empty;
			}
		}

		private void FetchMe()
		{
			if (_api != null && _config.FetchMe)
			{
				_api.Get("/me", null, OnFetchMe);
			}
			else
			{
				FetchFriends();
			}
		}

		private void OnFetchMe(string err, object result)
		{
			if (!string.IsNullOrEmpty(err))
			{
				base.Hub.FatalError(err);
				return;
			}
			if (result == null)
			{
				base.Hub.FatalError("Failed to fetch user");
				return;
			}
			Hashtable hashtable = (Hashtable)result;
			hashtable["fbid"] = hashtable["id"];
			hashtable["uid"] = base.Hub.LoginManager.LocalUserId;
			_me = LoadUser(hashtable);
			FetchFriends();
		}

		private FacebookUser LoadUser(Hashtable data)
		{
			string text = Dot.String("fbid", data, string.Empty);
			if (string.IsNullOrEmpty(text))
			{
				return null;
			}
			FacebookUser value;
			if (!_users.TryGetValue(text, out value))
			{
				value = new FacebookUser(text);
				_users.Add(text, value);
			}
			value.Update(data);
			return value;
		}

		private void FetchFriends()
		{
			if (_api != null && _config.FetchFriends)
			{
				string value = "SELECT uid,first_name,last_name,is_app_user from user where uid in (select uid2 from friend where uid1=me())";
				_api.Get("/fql", new Hashtable { { "q", value } }, OnFetchFriends);
			}
			else
			{
				base.State = SubSystemState.Connected;
			}
		}

		private void DoLookup(List<string> fbIds)
		{
			Request request = base.Hub.ApiEndPoint.Post("/login/facebook/lookup");
			request.AddData("ids", fbIds.ToArray());
			base.Hub.ApiEndPoint.Service(request, (Response res) =>
			{
				if (res.sucessful && res.arrayList != null)
				{
					foreach (Hashtable array in res.arrayList)
					{
						LoadUser(array);
					}
				}
				_appFriends.RemoveAll((FacebookUser obj) => !obj.UserId.Valid);
				base.State = SubSystemState.Connected;
			});
		}

		private void OnFetchFriends(string err, object results)
		{
			if (!string.IsNullOrEmpty(err))
			{
				base.Hub.FatalError(err);
				return;
			}
			_friends.Clear();
			_appFriends.Clear();
			List<string> list = new List<string>();
			ArrayList arrayList = Dot.Array("data", results, null);
			if (arrayList != null)
			{
				foreach (Hashtable item in arrayList)
				{
					item["fbid"] = item["uid"];
					item.Remove("uid");
					FacebookUser facebookUser = LoadUser(item);
					if (facebookUser.IsAppUser)
					{
						_appFriends.Add(facebookUser);
						if (!facebookUser.UserId.Valid)
						{
							list.Add(facebookUser.FacebookId);
						}
					}
					_friends.Add(facebookUser);
				}
			}
			if (list.Count > 0)
			{
				DoLookup(list);
			}
			else
			{
				base.State = SubSystemState.Connected;
			}
		}
	}
}
