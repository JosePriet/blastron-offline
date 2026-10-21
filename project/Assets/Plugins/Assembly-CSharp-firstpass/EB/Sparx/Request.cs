using System.Collections;
using EB.Net;

namespace EB.Sparx
{
	public class Request
	{
		private Hashtable _data;

		private Hashtable _query;

		private Uri _url;

		private bool _isPost;

		public object userData { get; set; }

		public int id { get; set; }

		public Action<byte[]> dataCallback { get; set; }

		public Action<WebRequest> headersCallback { get; set; }

		public Hashtable query
		{
			get
			{
				return _query;
			}
		}

		public Hashtable data
		{
			get
			{
				return _data;
			}
		}

		public bool isPost
		{
			get
			{
				return _isPost;
			}
		}

		public Uri uri
		{
			get
			{
				return _url;
			}
		}

		public int numRetries { get; set; }

		public string url
		{
			get
			{
				return _url.Scheme + "://" + _url.HostAndPort + _url.Path + "?" + QueryString.Stringify(_query);
			}
		}

		public Request(string url, bool isPost)
		{
			_url = new Uri(url);
			_isPost = isPost;
			numRetries = 5;
			_query = QueryString.Parse(_url.Query);
			if (isPost)
			{
				_data = new Hashtable();
			}
			else
			{
				_data = _query;
			}
		}

		public void AddQuery(string key, object value)
		{
			_query[key] = value;
		}

		public void AddData(string key, object value)
		{
			_data[key] = value;
		}
	}
}
