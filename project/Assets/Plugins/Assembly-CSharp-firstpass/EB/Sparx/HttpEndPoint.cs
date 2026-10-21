using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using EB.Collections;
using EB.Net;
using UnityEngine;

namespace EB.Sparx
{
	public class HttpEndPoint : EndPoint
	{
		private const string SessionKey = "stoken";

		private EB.Collections.Queue<Request> _queue = new EB.Collections.Queue<Request>();

		private Hashtable _data = new Hashtable();

		private Request _current;

		private int _nextId = 1;

		private int _doneId;

		private object _interval;

		private int _lastActivity = -1;

		private Hmac _hmac;

		public HttpEndPoint(string endPoint, EndPointOptions options)
			: base(endPoint, options)
		{
			if (options.Key != null)
			{
				_hmac = Hmac.Sha1(options.Key);
			}
		}

		public override void Dispose()
		{
			StopKeepAlive();
			base.Dispose();
		}

		public override void StartKeepAlive()
		{
			StopKeepAlive();
			if (base.Options.KeepAlive && base.Options.KeepAliveInterval > 0 && !string.IsNullOrEmpty(base.Options.KeepAliveUrl))
			{
				_interval = Coroutines.SetInterval(OnInterval, base.Options.KeepAliveInterval * 1000 / 4);
			}
		}

		public override void StopKeepAlive()
		{
			Coroutines.ClearInterval(_interval);
			_interval = null;
			_lastActivity = -1;
		}

		private void OnInterval()
		{
			if (_lastActivity <= 0 || Time.Now - _lastActivity <= base.Options.KeepAliveInterval)
			{
				return;
			}
			Request request = Post(base.Options.KeepAliveUrl);
			Service(request, (Response r) =>
			{
				if (!r.sucessful && r.fatal)
				{
					Hub.Instance.FatalError(r.localizedError);
				}
			});
		}

		public override void AddData(string name, string value)
		{
			_data[name] = value;
		}

		public override string GetData(string key)
		{
			object obj = _data[key];
			if (obj != null)
			{
				return obj.ToString();
			}
			return string.Empty;
		}

		private Request Prepare(Request r)
		{
			foreach (DictionaryEntry datum in _data)
			{
				r.AddQuery(datum.Key.ToString(), datum.Value);
			}
			return r;
		}

		public override int Service(Request request, Action<Response> callback)
		{
			request.userData = callback;
			request.id = _nextId++;
			_queue.Enqueue(request);
			ServiceNext();
			return request.id;
		}

		public override bool IsDone(int requestId)
		{
			return _doneId >= requestId;
		}

		private void ServiceNext()
		{
			if (_current == null && _queue.Count > 0)
			{
				_current = _queue.Dequeue();
				Prepare(_current);
				Coroutines.Run(Fetch(_current));
			}
		}

		private string Sign(Request r, string method, string data)
		{
			if (base.Options.Key != null)
			{
				StringBuilder stringBuilder = new StringBuilder(2048);
				stringBuilder.Append(method);
				stringBuilder.Append("\n");
				stringBuilder.Append(r.uri.Host);
				stringBuilder.Append("\n");
				stringBuilder.Append(r.uri.Path);
				stringBuilder.Append("\n");
				stringBuilder.Append(data);
				stringBuilder.Append("\n");
				byte[] bytes = _hmac.Hash(Encoding.GetBytes(stringBuilder.ToString()));
				return Encoding.ToBase64String(bytes);
			}
			return string.Empty;
		}

		private WebRequest Generate(Request r, int retry)
		{
			WebRequest webRequest = null;
			if (Time.Valid)
			{
				r.AddQuery("ts", Time.Now);
			}
			if (retry > 0)
			{
				r.AddQuery("retry", retry);
			}
			if (r.isPost)
			{
				string text = JSON.Stringify(r.data);
				byte[] bytes = Encoding.GetBytes(text);
				string value = Sign(r, "POST", text);
				r.AddQuery("sig", value);
				Hashtable hashtable = new Hashtable();
				hashtable["Content-Type"] = "application/json";
				Debug.Log("Post: " + r.url + "\n" + text);
				webRequest = new WebRequest(r.url, bytes, hashtable);
			}
			else
			{
				r.query.Remove("sig");
				string value2 = Sign(r, "GET", QueryString.StringifySorted(r.query));
				r.AddQuery("sig", value2);
				Debug.Log("Get: " + r.url);
				webRequest = new WebRequest(r.url);
			}
			webRequest.OnDataCallback = r.dataCallback;
			webRequest.OnHeaders = r.headersCallback;
			webRequest.Start();
			return webRequest;
		}

		private IEnumerator Fetch(Request req)
		{
			Response res = new Response(req);
			int kNumRetries = req.numRetries;
			for (int i = 0; i <= kNumRetries; i++)
			{
				if (!base.HasInternetConnectivity)
				{
					Debug.Log("No internet connectivity");
					res.error = "ID_SPARX_ERROR_NOT_CONNECTED";
					res.fatal = true;
					break;
				}
				if (i > 1)
				{
					yield return new WaitForSeconds(Mathf.Pow(2f, i));
				}
				if (SafeValue.Breach || (bool)Memory.Breach)
				{
					res.fatal = true;
					res.error = "ID_SPARX_ERROR_UNKNOWN";
					break;
				}
				WebRequest www = Generate(req, i);
				while (!www.isDone)
				{
					yield return 1;
				}
				res.timeTaken = www.responseTime;
				if (string.IsNullOrEmpty(www.error) && req.dataCallback != null)
				{
					res.sucessful = true;
					res.fatal = false;
					res.sessionError = false;
					break;
				}
				if (string.IsNullOrEmpty(www.error) && !string.IsNullOrEmpty(www.text))
				{
					res.text = www.text ?? string.Empty;
					Debug.Log("Result: " + res.text);
					res.result = null;
					res.error = null;
					try
					{
						object obj = JSON.Parse(www.text);
						if (obj == null)
						{
							res.error = "Failed to decode json: " + www.text;
						}
						else if (obj is Hashtable)
						{
							Hashtable response = obj as Hashtable;
							res.result = response["result"];
							res.error = response["err"];
							Time.Now = Dot.Integer("ts", response, Time.Now);
							_lastActivity = Time.Now;
						}
						else
						{
							res.error = "Obj is " + obj.ToString();
						}
					}
					catch (Exception ex)
					{
						Debug.LogWarning("failed to decode www " + ex);
						res.error = ex;
					}
					if (res.result != null)
					{
						if (res.result is Hashtable)
						{
							res.hashtable = (Hashtable)res.result;
						}
						else if (res.result is ArrayList)
						{
							res.arrayList = (ArrayList)res.result;
						}
						else if (res.result is double)
						{
							res.number = (double)res.result;
						}
						else if (res.result is string)
						{
							res.str = (string)res.result;
						}
					}
					if (res.error == null)
					{
						res.sucessful = true;
						res.fatal = false;
						res.sessionError = false;
						break;
					}
					res.fatal = Dot.Bool("fatal", res.hashtable, true);
					res.sessionError = Dot.Bool("invalid_session", res.hashtable, false);
					if (!Dot.Bool("retry", res.hashtable, true))
					{
						Debug.LogWarning("skipping retry");
						break;
					}
				}
				else
				{
					if (www.statusCode != 0)
					{
						PrintError(req, www);
					}
					res.error = www.error ?? "unknown";
					res.fatal = true;
					if (www.statusCode >= 400 && www.statusCode <= 499)
					{
						break;
					}
				}
			}
			OnComplete(res);
		}

		private void PrintError(Request r, WebRequest www)
		{
			Debug.LogError("Url: " + r.url + " Error: " + (www.error ?? "null") + " Status: " + www.statusCode);
			if (www.responseHeaders == null || www.responseHeaders.Count <= 0)
			{
				return;
			}
			foreach (KeyValuePair<string, string> responseHeader in www.responseHeaders)
			{
				Debug.LogError("Error Header: {0}:{1}", responseHeader.Key ?? string.Empty, responseHeader.Value ?? string.Empty);
			}
		}

		private void OnComplete(Response result)
		{
			Request request = result.request;
			if (_current != result.request)
			{
				Debug.LogError("Something went seriously wrong in the service queue");
			}
			if (request.userData != null && request.userData is Action<Response>)
			{
				Action<Response> action = (Action<Response>)request.userData;
				action(result);
			}
			_doneId = _current.id;
			_current = null;
			ServiceNext();
		}
	}
}
