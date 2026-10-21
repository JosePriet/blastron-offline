using System.Collections;
using EB.Sequence.Runtime;
using EB.Sparx;
using UnityEngine;

namespace EB.Sequence
{
	public class VariableManager : MonoBehaviour, ISaveLoadInterface
	{
		private Hashtable _values = new Hashtable();

		private Hashtable _listeners = new Hashtable();

		public static VariableManager Instance { get; private set; }

		public string Key
		{
			get
			{
				return "seqvars";
			}
		}

		public bool PostLoadSaveRequired
		{
			get
			{
				return false;
			}
		}

		public void AddNode(Node node, string name)
		{
			ArrayList arrayList = (ArrayList)_listeners[name];
			if (arrayList == null)
			{
				arrayList = new ArrayList();
				_listeners[name] = arrayList;
			}
			arrayList.Remove(node);
			arrayList.Add(node);
		}

		public void RmvNode(Node node, string name)
		{
			ArrayList arrayList = (ArrayList)_listeners[name];
			if (arrayList != null)
			{
				arrayList.Remove(node);
			}
		}

		public void Awake()
		{
			Instance = this;
			Hub.Instance.SaveLoadManager.Register(this);
		}

		public double GetNumber(string name, double defaultV)
		{
			object variable = GetVariable(name);
			double result;
			if (variable != null && double.TryParse(variable.ToString(), out result))
			{
				return result;
			}
			SetVariable(name, defaultV);
			return defaultV;
		}

		public string GetString(string name, string defaultV)
		{
			object variable = GetVariable(name);
			if (variable != null)
			{
				return variable.ToString();
			}
			SetVariable(name, defaultV);
			return defaultV;
		}

		public virtual object GetVariable(string name)
		{
			return _values[name];
		}

		public void RemoveVariable(string name)
		{
			_values.Remove(name);
		}

		public void SetVariable(string name, object value)
		{
			_values[name] = value;
			if (name.StartsWith("s_"))
			{
				Hub.Instance.SaveLoadManager.Save(this);
			}
			ArrayList arrayList = (ArrayList)_listeners[name];
			if (arrayList == null)
			{
				return;
			}
			foreach (Node item in arrayList)
			{
				item.GlobalValueChanged();
			}
		}

		public IEnumerator Save(Id userId, Hashtable data)
		{
			foreach (DictionaryEntry entry in _values)
			{
				if ((entry.Key as string).StartsWith("s_"))
				{
					data[entry.Key] = entry.Value;
				}
			}
			yield break;
		}

		public IEnumerator Load(Id userId, Hashtable data)
		{
			_values = new Hashtable(data);
			yield break;
		}

		public IEnumerator PostLoad(Id userId)
		{
			yield break;
		}
	}
}
