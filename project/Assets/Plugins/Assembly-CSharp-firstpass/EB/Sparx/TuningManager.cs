using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace EB.Sparx
{
	public class TuningManager : SubSystem
	{
		private Hashtable _data = new Hashtable();

		public override void Initialize(Config config)
		{
		}

		public override IEnumerator LoadOfflineData()
		{
			yield break;
		}

		public override void Connect()
		{
			Refresh();
		}

		public override void Disconnect(bool isLogout)
		{
		}

		public void Refresh()
		{
			Request request = base.Hub.ApiEndPoint.Get("/tuning");
			base.Hub.ApiEndPoint.Service(request, (Response res) =>
			{
				if (res.sucessful)
				{
					OnRefresh(string.Empty, res.arrayList);
				}
				else
				{
					OnRefresh(res.localizedError, null);
				}
			});
		}

		private void OnRefresh(string error, ArrayList data)
		{
			if (!string.IsNullOrEmpty(error))
			{
				FatalError(error);
				return;
			}
			base.State = SubSystemState.Connected;
			_data.Clear();
			foreach (Hashtable datum in data)
			{
				Dot.DeepCopy(datum, _data);
			}
			Debug.Log("Got data: " + JSON.Stringify(_data));
		}

		public void Apply(object instance, FieldInfo field, string name)
		{
			if (field.FieldType.IsPrimitive || field.FieldType == typeof(string))
			{
				object obj = Dot.Find(name, _data);
				if (obj != null)
				{
					obj = Convert.ChangeType(obj, field.FieldType);
					if (obj != null)
					{
						field.SetValue(instance, obj);
					}
				}
			}
			else if (field.FieldType.IsEnum)
			{
				object obj2 = Dot.Find(name, _data);
				if (obj2 != null)
				{
					obj2 = Enum.Parse(field.FieldType, obj2.ToString(), true);
					if (obj2 != null)
					{
						field.SetValue(instance, obj2);
					}
				}
			}
			else if (field.FieldType.IsArray || field.FieldType.IsSubclassOf(typeof(IList)))
			{
				IList list = (IList)field.GetValue(instance);
				if (list == null)
				{
					return;
				}
				for (int i = 0; i < list.Count; i++)
				{
					string name2 = name + '.' + i;
					object obj3 = Dot.Find(name2, _data);
					if (obj3 == null)
					{
						continue;
					}
					object obj4 = list[i];
					if (obj4 == null)
					{
						continue;
					}
					if (obj4.GetType() == typeof(string))
					{
						list[i] = obj3.ToString();
					}
					else if (obj4.GetType().IsPrimitive)
					{
						obj3 = Convert.ChangeType(obj3, obj4.GetType());
						if (obj3 != null)
						{
							list[i] = obj3;
						}
					}
					else
					{
						Apply(obj4, name2);
					}
				}
			}
			else if (field.FieldType.IsClass)
			{
				Serialization.CachedClassInfo classInfo = Serialization.GetClassInfo(field.FieldType);
				if (classInfo != null)
				{
					object value = field.GetValue(instance);
					if (value != null)
					{
						Apply(value, name);
					}
				}
			}
			else if (instance != null)
			{
				Serialization.CachedClassInfo classInfo2 = Serialization.GetClassInfo(instance.GetType());
				if (classInfo2 != null)
				{
					Apply(instance, name);
				}
			}
		}

		public void Apply(object instance, string name)
		{
			if (instance == null)
			{
				return;
			}
			Serialization.CachedClassInfo classInfo = Serialization.GetClassInfo(instance.GetType());
			if (classInfo == null)
			{
				return;
			}
			foreach (KeyValuePair<string, FieldInfo> field in classInfo.fields)
			{
				string name2 = name + "." + StringUtil.SafeKey(field.Key);
				Apply(instance, field.Value, name2);
			}
		}

		public string GetString(string name, string defaultValue)
		{
			return Dot.String(name, _data, defaultValue);
		}

		public int GetInt(string name, int defaultValue)
		{
			return Dot.Integer(name, _data, defaultValue);
		}

		public bool GetBool(string name, bool defaultValue)
		{
			return Dot.Bool(name, _data, defaultValue);
		}

		public float GetFloat(string name, float defaultValue)
		{
			return Dot.Single(name, _data, defaultValue);
		}
	}
}
