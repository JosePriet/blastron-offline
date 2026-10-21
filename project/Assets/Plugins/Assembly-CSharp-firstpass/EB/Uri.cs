namespace EB
{
	public class Uri
	{
		public enum Component
		{
			Original = 0,
			Protocol = 1,
			User = 2,
			Password = 3,
			Host = 4,
			Port = 5,
			Path = 6,
			Query = 7,
			Count = 8
		}

		private string[] _components = new string[8];

		public string Protocol
		{
			get
			{
				return GetComponent(Component.Protocol, string.Empty);
			}
		}

		public string Scheme
		{
			get
			{
				return GetComponent(Component.Protocol, string.Empty);
			}
		}

		public string User
		{
			get
			{
				return GetComponent(Component.User, string.Empty);
			}
		}

		public string Password
		{
			get
			{
				return GetComponent(Component.Password, string.Empty);
			}
		}

		public string Host
		{
			get
			{
				return GetComponent(Component.Host, string.Empty);
			}
		}

		public string Query
		{
			get
			{
				return GetComponent(Component.Query, string.Empty);
			}
		}

		public string Path
		{
			get
			{
				return GetComponent(Component.Path, "/");
			}
		}

		public int Port
		{
			get
			{
				return int.Parse(GetComponent(Component.Port, GetDefaultPort().ToString()));
			}
		}

		public string HostAndPort
		{
			get
			{
				string text = Host;
				if (Port != GetDefaultPort())
				{
					text = text + ":" + Port;
				}
				return text;
			}
		}

		public string PathAndQuery
		{
			get
			{
				string text = Path;
				if (!string.IsNullOrEmpty(Query))
				{
					text = text + '?' + Query;
				}
				return text;
			}
		}

		public Uri(string url)
		{
			Parse(url);
		}

		public Uri()
		{
		}

		public bool Parse(string url)
		{
			int num = 0;
			SetComponent(Component.Original, url);
			num = url.IndexOf(':');
			if (num <= 0)
			{
				Debug.LogError("failed to parse protocol " + url);
				return false;
			}
			string value = url.Substring(0, num);
			SetComponent(Component.Protocol, value);
			url = url.Substring(num);
			if (!url.StartsWith("://"))
			{
				Debug.LogError("failed to parse protocol " + url);
				return false;
			}
			url = url.Substring(3);
			int num2 = url.IndexOf('/');
			int num3 = url.IndexOf('@');
			if (num3 >= 0 && (num2 < 0 || num2 > num3))
			{
				string text = url.Substring(0, num3);
				url = url.Substring(num3 + 1);
				num = text.IndexOf(':');
				if (num >= 0)
				{
					SetComponent(Component.User, text.Substring(0, num));
					SetComponent(Component.User, text.Substring(num + 1));
				}
				else
				{
					SetComponent(Component.User, text);
				}
			}
			num = url.IndexOfAny(new char[3] { '/', '?', ':' });
			if (num < 0)
			{
				SetComponent(Component.Host, url);
			}
			else
			{
				SetComponent(Component.Host, url.Substring(0, num));
				char c = url[num];
				url = url.Substring(num + 1);
				if (c == ':')
				{
					num = url.IndexOfAny(new char[2] { '/', '?' });
					string empty = string.Empty;
					if (num < 0)
					{
						empty = url;
						url = string.Empty;
					}
					else
					{
						empty = url.Substring(0, num);
						url = url.Substring(num + 1);
					}
					int result = 0;
					if (!int.TryParse(empty, out result))
					{
						Debug.LogError("failed to parse port " + url);
						return false;
					}
					SetComponent(Component.Port, result.ToString());
				}
				num = url.IndexOf('?');
				if (num < 0)
				{
					SetComponent(Component.Path, "/" + url);
				}
				else
				{
					SetComponent(Component.Path, "/" + url.Substring(0, num));
					SetComponent(Component.Query, url.Substring(num + 1));
				}
			}
			return true;
		}

		public int GetDefaultPort()
		{
			switch (Protocol)
			{
			case "http":
			case "ws":
				return 80;
			case "https":
			case "wss":
				return 443;
			case "ssh":
				return 22;
			case "ftp":
				return 21;
			default:
				return 0;
			}
		}

		public void Print()
		{
			for (int i = 0; i < _components.Length; i++)
			{
				Debug.Log("Component {0}:{1}", (Component)i, _components[i]);
			}
		}

		public override string ToString()
		{
			string text = Protocol + "://";
			if (!string.IsNullOrEmpty(User))
			{
				text += User;
				if (!string.IsNullOrEmpty(Password))
				{
					text = text + ":" + Password;
				}
				text += "@";
			}
			text += HostAndPort;
			return text + PathAndQuery;
		}

		public void SetComponent(Component c, string value)
		{
			_components[(int)c] = value;
		}

		public string GetComponent(Component c, string def)
		{
			string text = _components[(int)c];
			if (!string.IsNullOrEmpty(text))
			{
				return text;
			}
			return def;
		}
	}
}
