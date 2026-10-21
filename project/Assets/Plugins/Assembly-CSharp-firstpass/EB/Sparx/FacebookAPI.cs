using System.Collections;
using UnityEngine;

namespace EB.Sparx
{
	public class FacebookAPI
	{
		private const int _numRetries = 5;

		private string _accessToken;

		public FacebookAPI(string access_token)
		{
			_accessToken = access_token;
		}

		public Coroutine Post(string uri, Hashtable parameters, Action<string, object> callback)
		{
			return Coroutines.Run(_Graph("POST", uri, parameters, callback));
		}

		public Coroutine Get(string uri, Hashtable parameters, Action<string, object> callback)
		{
			return Coroutines.Run(_Graph("GET", uri, parameters, callback));
		}

		private int GetStatusCode(WWW www)
		{
			string value;
			if (www.responseHeaders.TryGetValue("STATUS", out value))
			{
				string[] array = value.Split(' ');
				if (array.Length > 2)
				{
					int result = 0;
					int.TryParse(array[1], out result);
					return result;
				}
			}
			return 0;
		}

		private static WWWForm BuildForm(Hashtable options)
		{
			WWWForm wWWForm = new WWWForm();
			options.Remove("access_token");
			foreach (DictionaryEntry option in options)
			{
				object key = option.Key;
				object value = option.Value;
				string value2 = value.ToString();
				if (value is byte[])
				{
					wWWForm.AddBinaryData(key.ToString(), (byte[])value);
					continue;
				}
				if (value is ICollection)
				{
					value2 = JSON.Stringify(value);
				}
				wWWForm.AddField(key.ToString(), value2);
			}
			return wWWForm;
		}

		private IEnumerator _Graph(string method, string uri, Hashtable parameters, Action<string, object> callback)
		{
			string url = "https://graph.facebook.com" + uri;
			if (parameters == null)
			{
				parameters = new Hashtable();
			}
			string lastError = string.Empty;
			for (int i = 0; i < 5; i++)
			{
				WWW www = null;
				if (method == "POST")
				{
					www = new WWW(form: BuildForm(parameters), url: url + "?access_token=" + _accessToken);
				}
				else
				{
					parameters["access_token"] = _accessToken;
					www = new WWW(url + "?" + QueryString.Stringify(parameters));
				}
				Debug.Log("url: " + www.url);
				yield return www;
				int status = GetStatusCode(www);
				if (string.IsNullOrEmpty(www.error))
				{
					string text = www.text;
					try
					{
						Debug.Log("FbResult: " + text);
						object result = JSON.Parse(text);
						callback(null, result);
						yield break;
					}
					catch
					{
					}
					continue;
				}
				if (status >= 400 && status <= 499)
				{
					lastError = www.error;
					break;
				}
				Debug.Log("Error headers: {0}", www.responseHeaders);
				lastError = www.error;
				yield return new WaitForSeconds(0.5f);
			}
			callback(lastError, null);
		}
	}
}
