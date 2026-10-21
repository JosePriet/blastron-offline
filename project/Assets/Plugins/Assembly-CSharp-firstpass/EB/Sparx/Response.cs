using System.Collections;

namespace EB.Sparx
{
	public class Response
	{
		public Request request { get; set; }

		public bool sucessful { get; set; }

		public int timeTaken { get; set; }

		public string text { get; set; }

		public object result { get; set; }

		public object error { get; set; }

		public bool fatal { get; set; }

		public bool sessionError { get; set; }

		public Hashtable hashtable { get; set; }

		public ArrayList arrayList { get; set; }

		public double number { get; set; }

		public string str { get; set; }

		public string localizedError
		{
			get
			{
				if (error != null)
				{
					string text = error.ToString();
					string value = string.Empty;
					if (Localizer.GetString(text, out value))
					{
						return value;
					}
					if (text.StartsWith("^"))
					{
						return text;
					}
					return Localizer.GetString("ID_SPARX_ERROR_UNKNOWN");
				}
				return string.Empty;
			}
		}

		public int id
		{
			get
			{
				return request.id;
			}
		}

		public string url
		{
			get
			{
				return request.url;
			}
		}

		public Response(Request r)
		{
			request = r;
		}
	}
}
