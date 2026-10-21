using System.Collections;

namespace EB.Sparx
{
	public class Data
	{
		public Id userId { get; private set; }

		public Hashtable data { get; private set; }

		public Hashtable fragments { get; private set; }

		public bool fullFetch { get; private set; }

		public Data(Id id)
		{
			userId = id;
			data = new Hashtable();
			fragments = new Hashtable();
			fullFetch = false;
		}

		public void Load(string key)
		{
			object jSON = SecurePrefs.GetJSON(key);
			if (jSON != null && jSON is Hashtable)
			{
				StoreResult((Hashtable)jSON, true);
			}
		}

		public void Save(string key)
		{
			string text = SecurePrefs.SetJSON(key, fragments);
			Debug.Log("Save Offline: \n" + text);
		}

		public void StoreResult(Hashtable obj, bool full)
		{
			if (obj == null)
			{
				return;
			}
			if (full)
			{
				fullFetch = true;
			}
			foreach (DictionaryEntry item in obj)
			{
				string key = item.Key.ToString();
				data[key] = item.Value;
				fragments[key] = new JSON.Fragment(JSON.Stringify(item.Value));
			}
		}

		public Hashtable Delta(Hashtable obj)
		{
			Hashtable hashtable = new Hashtable();
			foreach (DictionaryEntry item in obj)
			{
				object obj2 = fragments[item.Key];
				JSON.Fragment fragment = new JSON.Fragment(JSON.Stringify(item.Value));
				if (obj2 == null || !Equal(obj2, fragment))
				{
					hashtable.Add(item.Key, fragment);
				}
				else
				{
					Debug.Log("skipping save for key: " + item.Key);
				}
			}
			return hashtable;
		}

		private bool Equal(object o1, object o2)
		{
			return JSON.Stringify(o1) == JSON.Stringify(o2);
		}
	}
}
