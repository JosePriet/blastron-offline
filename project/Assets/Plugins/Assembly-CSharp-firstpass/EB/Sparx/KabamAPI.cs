using System.Collections;
using UnityEngine;

namespace EB.Sparx
{
	public class KabamAPI
	{
		private EndPoint _endPoint;

		private Hashtable _deviceInfo;

		private string _udid;

		private string _lang;

		public string _sha1 = string.Empty;

		public string _salt = string.Empty;

		public string _chal = string.Empty;

		public KabamAPI(EndPoint endpoint, Language locale)
		{
			_endPoint = endpoint;
			_deviceInfo = Version.GetDeviceInfo();
			_udid = Device.UniqueIdentifier;
			_lang = Localizer.GetOldLanguageCode(locale);
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
				string text = Dot.String("legacy_udid", result.hashtable, null);
				if (!string.IsNullOrEmpty(text))
				{
					Debug.Log("Got legacy UDID!:" + text);
					_udid = text;
				}
				callback(null, result.hashtable);
			}
			else
			{
				callback(result.localizedError, result.hashtable);
			}
		}

		private void AddData(Request request)
		{
			request.AddData("platform", Device.MobilePlatform);
			request.AddData("device", Device.DeviceModel);
			request.AddData("version", Version.GetVersion());
			request.AddData("locale", Version.GetLocale());
			request.AddData("udid", _udid);
			request.AddData("info", _deviceInfo);
			request.AddData("cellular", (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork) ? 1 : 0);
			request.AddData("lang", _lang);
			if (!string.IsNullOrEmpty(_sha1))
			{
				request.AddData("sha1", _sha1);
			}
			if (!string.IsNullOrEmpty(_salt))
			{
				request.AddData("salt", _salt);
			}
			if (!string.IsNullOrEmpty(_chal))
			{
				request.AddData("chal", _chal);
			}
		}

		public void GetSupportUrl(Action<string, object> callback)
		{
			Request request = _endPoint.Post("/kabam/support");
			AddData(request);
			_endPoint.Service(request, (Response result) =>
			{
				if (result.sucessful)
				{
					callback(null, result.result);
				}
				else
				{
					callback(result.localizedError, result.result);
				}
			});
		}

		public void ForgotPassword(string email, Action<string, object> callback)
		{
			Request request = _endPoint.Post("/kabam/forgot");
			request.AddData("email", email);
			AddData(request);
			_endPoint.Service(request, (Response result) =>
			{
				if (result.sucessful)
				{
					callback(null, result.result);
				}
				else
				{
					callback(result.localizedError, result.result);
				}
			});
		}

		public void Login(string naid, string email, string password, Hashtable data, Action<string, Hashtable> callback)
		{
			Request request = _endPoint.Post("/kabam/login");
			request.AddData("email", email);
			request.AddData("naid", naid);
			request.AddData("password", password);
			if (data != null)
			{
				foreach (DictionaryEntry datum in data)
				{
					request.AddData(datum.Key.ToString(), datum.Value);
				}
			}
			AddData(request);
			_endPoint.Service(request, (Response result) =>
			{
				HandleLoginResult(result, callback);
			});
		}

		public void LoginGuest(string naid, Action<string, Hashtable> callback)
		{
			Request request = _endPoint.Post("/kabam/guest");
			request.AddData("naid", naid);
			AddData(request);
			_endPoint.Service(request, (Response result) =>
			{
				HandleLoginResult(result, callback);
			});
		}

		public void Upgrade(string naid, string email, string password, string password2, Hashtable data, Action<string, Hashtable> callback)
		{
			Request request = _endPoint.Post("/kabam/upgrade");
			request.AddData("email", email);
			request.AddData("naid", naid);
			request.AddData("password", password);
			request.AddData("password2", password2);
			if (data != null)
			{
				foreach (DictionaryEntry datum in data)
				{
					request.AddData(datum.Key.ToString(), datum.Value);
				}
			}
			AddData(request);
			_endPoint.Service(request, (Response result) =>
			{
				HandleLoginResult(result, callback);
			});
		}

		public void Register(Action<string, Hashtable> callback)
		{
			Request request = _endPoint.Post("/kabam/register");
			request.numRetries = 2;
			request.AddData("legacy", 1);
			AddData(request);
			_endPoint.Service(request, (Response result) =>
			{
				HandleLoginResult(result, callback);
			});
		}

		public void LoginFacebook(string naid, string accessToken, string email, Action<string, Hashtable> callback)
		{
			Request request = _endPoint.Post("/kabam/facebook");
			request.AddData("naid", naid);
			request.AddData("access_token", accessToken);
			if (!string.IsNullOrEmpty(email))
			{
				request.AddData("email", email);
			}
			AddData(request);
			_endPoint.Service(request, (Response result) =>
			{
				HandleLoginResult(result, callback);
			});
		}

		public void SelectName(string name, Action<string, Hashtable> callback)
		{
			Request request = _endPoint.Post("/kabam/name");
			request.AddData("name", name);
			_endPoint.Service(request, (Response result) =>
			{
				HandleLoginResult(result, callback);
			});
		}
	}
}
