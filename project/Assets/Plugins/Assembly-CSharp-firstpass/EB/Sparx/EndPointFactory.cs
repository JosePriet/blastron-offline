using System;

namespace EB.Sparx
{
	public static class EndPointFactory
	{
		public static EndPoint Create(string url, EndPointOptions options)
		{
			// Offline build: the Sparx servers are gone, answer locally.
			return (EndPoint)Activator.CreateInstance(Type.GetType("EB.Sparx.LocalEndPoint, Assembly-CSharp", true), url, options);
			Uri uri = new Uri(url);
			switch (uri.Scheme)
			{
			case "http":
			case "https":
				return new HttpEndPoint(url, options);
			case "ws":
			case "wss":
				return new WebSocketEndPoint(url, options);
			default:
				throw new ArgumentException("Unknown scheme " + uri.Scheme);
			}
		}
	}
}
