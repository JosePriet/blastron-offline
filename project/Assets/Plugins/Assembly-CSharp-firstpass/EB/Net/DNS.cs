using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace EB.Net
{
	public static class DNS
	{
		private class Entry
		{
			public IPAddress last;

			public DateTime lastT;

			public IPAddress[] addresses = new IPAddress[0];
		}

		private static Dictionary<string, Entry> _entries = new Dictionary<string, Entry>();

		private static Entry GetEntry(string host)
		{
			lock (_entries)
			{
				Entry value;
				if (!_entries.TryGetValue(host, out value))
				{
					value = new Entry();
					_entries.Add(host, value);
				}
				return value;
			}
		}

		public static void StoreLast(string hostname, IPAddress addr)
		{
			Entry entry = GetEntry(hostname);
			lock (entry)
			{
				entry.last = addr;
				entry.lastT = DateTime.Now;
			}
		}

		public static IPAddress[] Lookup(string hostname)
		{
			IPAddress address;
			if (IPAddress.TryParse(hostname, out address))
			{
				return new IPAddress[1] { address };
			}
			Entry entry = GetEntry(hostname);
			List<IPAddress> list = new List<IPAddress>();
			try
			{
				IAsyncResult asyncResult = Dns.BeginGetHostAddresses(hostname, null, null);
				DateTime dateTime = DateTime.Now + TimeSpan.FromSeconds(5.0);
				while (DateTime.Now < dateTime)
				{
					if (!asyncResult.IsCompleted)
					{
						continue;
					}
					IPAddress[] array = Dns.EndGetHostAddresses(asyncResult);
					if (array == null)
					{
						break;
					}
					IPAddress[] array2 = array;
					foreach (IPAddress iPAddress in array2)
					{
						if (iPAddress.AddressFamily == AddressFamily.InterNetwork)
						{
							list.Add(iPAddress);
						}
					}
					break;
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("failed to lookup " + hostname + " " + ex);
			}
			lock (entry)
			{
				if (list.Count > 0)
				{
					entry.addresses = list.ToArray();
				}
				else
				{
					Debug.LogError("failed to lookup " + hostname);
				}
				if (entry.last != null)
				{
					int num = Array.IndexOf(entry.addresses, entry.last);
					if (num >= 0)
					{
						IPAddress iPAddress2 = entry.addresses[0];
						entry.addresses[0] = entry.addresses[num];
						entry.addresses[num] = iPAddress2;
					}
				}
				return entry.addresses;
			}
		}
	}
}
