using System;
using System.Collections.Generic;

namespace EB.Sparx
{
	public static class NetworkFactory
	{
		private static Dictionary<string, Type> _types = new Dictionary<string, Type>();

		public static void Register(string scheme, Type type)
		{
			_types.Add(scheme, type);
		}

		public static Network Create(string url)
		{
			Uri uri = new Uri(url);
			Type value = null;
			if (_types.TryGetValue(uri.Scheme, out value))
			{
				return (Network)Activator.CreateInstance(value);
			}
			throw new Exception("Uknown network type: " + uri.Scheme);
		}
	}
}
