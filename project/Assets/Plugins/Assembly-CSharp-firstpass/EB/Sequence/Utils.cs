using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using EB.Sequence.Runtime;
using EB.Sequence.Serialization;
using UnityEngine;

namespace EB.Sequence
{
	public static class Utils
	{
		public enum ValidateLinkResult
		{
			Ok = 0,
			InvalidType = 1,
			InvalidLink = 2
		}

		public enum LinkType
		{
			None = 0,
			Variable = 1,
			Trigger = 2,
			Entry = 3
		}

		private static Assembly[] _assemblies = new Assembly[0];

		private static char[] UpperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

		private static char[] kSplits = new char[2] { '[', ']' };

		public static void Initialize(Assembly[] assemblies)
		{
			_assemblies = assemblies;
		}

		public static MenuItemAttribute GetMenuAttribute(Type type)
		{
			object[] customAttributes = type.GetCustomAttributes(typeof(MenuItemAttribute), false);
			if (customAttributes.Length > 0)
			{
				return customAttributes[0] as MenuItemAttribute;
			}
			return null;
		}

		public static PropertyAttribute GetPropertyAttribute(Type type, string name)
		{
			FieldInfo field = type.GetField(name);
			if (field != null)
			{
				object[] customAttributes = field.GetCustomAttributes(typeof(PropertyAttribute), true);
				if (customAttributes.Length > 0)
				{
					return customAttributes[0] as PropertyAttribute;
				}
			}
			return null;
		}

		public static VariableAttribute GetVariableAttribute(Type type, string name)
		{
			FieldInfo field = type.GetField(name);
			if (field != null)
			{
				object[] customAttributes = field.GetCustomAttributes(typeof(VariableAttribute), true);
				if (customAttributes.Length > 0)
				{
					return customAttributes[0] as VariableAttribute;
				}
			}
			return null;
		}

		public static TriggerAttribute GetTriggerAttribute(Type type, string name)
		{
			FieldInfo field = type.GetField(name);
			if (field != null)
			{
				object[] customAttributes = field.GetCustomAttributes(typeof(TriggerAttribute), true);
				if (customAttributes.Length > 0)
				{
					return customAttributes[0] as TriggerAttribute;
				}
			}
			return null;
		}

		public static EntryAttribute GetEntryAttribute(Type type, string name)
		{
			MethodInfo method = type.GetMethod(name);
			if (method != null)
			{
				object[] customAttributes = method.GetCustomAttributes(typeof(EntryAttribute), true);
				if (customAttributes.Length > 0)
				{
					return customAttributes[0] as EntryAttribute;
				}
			}
			return null;
		}

		public static Type[] GetAllTypes()
		{
			List<Type> list = new List<Type>();
			Assembly[] assemblies = _assemblies;
			foreach (Assembly assembly in assemblies)
			{
				Type[] types = assembly.GetTypes();
				foreach (Type type in types)
				{
					if (type.GetCustomAttributes(typeof(MenuItemAttribute), false).Length > 0)
					{
						list.Add(type);
					}
				}
			}
			return list.ToArray();
		}

		public static NodeType GetNodeType(Type type)
		{
			if (type.IsSubclassOf(typeof(EB.Sequence.Runtime.Event)))
			{
				return NodeType.Event;
			}
			if (type.IsSubclassOf(typeof(EB.Sequence.Runtime.Action)))
			{
				return NodeType.Action;
			}
			if (type.IsSubclassOf(typeof(Condition)))
			{
				return NodeType.Condition;
			}
			if (type.IsSubclassOf(typeof(Variable)))
			{
				return NodeType.Variable;
			}
			throw new System.Exception("Invalid node type of type: " + type.Name);
		}

		public static PropertyType GetPropertyType(Type type)
		{
			if (type == typeof(int))
			{
				return PropertyType.Int;
			}
			if (type == typeof(float))
			{
				return PropertyType.Float;
			}
			if (type == typeof(string))
			{
				return PropertyType.String;
			}
			if (type == typeof(GameObject))
			{
				return PropertyType.GameObject;
			}
			if (type == typeof(bool))
			{
				return PropertyType.Boolean;
			}
			if (type == typeof(Color))
			{
				return PropertyType.Color;
			}
			if (type == typeof(Vector2))
			{
				return PropertyType.Vector2;
			}
			if (type == typeof(Vector3))
			{
				return PropertyType.Vector3;
			}
			if (type == typeof(Vector4))
			{
				return PropertyType.Vector4;
			}
			throw new System.Exception("Invalid property of type: " + type.Name);
		}

		public static Type GetTypeFromName(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return null;
			}
			Type type = Type.GetType(name, false);
			if (type == null)
			{
				Assembly[] assemblies = _assemblies;
				foreach (Assembly assembly in assemblies)
				{
					type = assembly.GetType(name, false);
					if (type != null)
					{
						return type;
					}
				}
			}
			return type;
		}

		public static EB.Sequence.Serialization.Node CreateSerializationNode(Type runtimeType, int id)
		{
			EB.Sequence.Serialization.Node node = new EB.Sequence.Serialization.Node();
			node.nodeType = GetNodeType(runtimeType);
			node.id = id;
			node.RuntimeType = runtimeType;
			UpdateSerializationNode(node);
			return node;
		}

		public static void UpdateSerializationNode(EB.Sequence.Serialization.Node node)
		{
			Type runtimeType = node.RuntimeType;
			if (runtimeType == null)
			{
				return;
			}
			object obj = Activator.CreateInstance(runtimeType);
			FieldInfo[] fields = runtimeType.GetFields();
			foreach (FieldInfo fieldInfo in fields)
			{
				PropertyAttribute[] array = (PropertyAttribute[])fieldInfo.GetCustomAttributes(typeof(PropertyAttribute), true);
				if (array.Length <= 0)
				{
					continue;
				}
				if (fieldInfo.FieldType.IsArray)
				{
					PropertyArray propertyArray = node.GetPropertyArray(fieldInfo.Name);
					if (propertyArray == null)
					{
						propertyArray = new PropertyArray();
						propertyArray.name = fieldInfo.Name;
						propertyArray.type = GetPropertyType(fieldInfo.FieldType.GetElementType());
						node.propertyArrays.Add(propertyArray);
					}
					foreach (Property item in propertyArray.items)
					{
						item.Value = item.Value;
					}
				}
				else
				{
					Property property = node.GetProperty(fieldInfo.Name);
					if (property == null)
					{
						property = new Property();
						property.name = fieldInfo.Name;
						property.type = GetPropertyType(fieldInfo.FieldType);
						property.Value = fieldInfo.GetValue(obj);
						node.properties.Add(property);
					}
					property.Value = property.Value;
				}
			}
			Property[] array2 = node.properties.ToArray();
			foreach (Property property2 in array2)
			{
				FieldInfo field = runtimeType.GetField(property2.name);
				if (field == null || field.GetCustomAttributes(typeof(PropertyAttribute), true).Length == 0)
				{
					node.properties.Remove(property2);
				}
			}
			PropertyArray[] array3 = node.propertyArrays.ToArray();
			foreach (PropertyArray propertyArray2 in array3)
			{
				FieldInfo field2 = runtimeType.GetField(propertyArray2.name);
				if (field2 == null || field2.GetCustomAttributes(typeof(PropertyAttribute), true).Length == 0)
				{
					node.propertyArrays.Remove(propertyArray2);
				}
			}
		}

		public static EB.Sequence.Runtime.Node CreateRuntimeNode(EB.Sequence.Serialization.Node node, Component parent)
		{
			Type runtimeType = node.RuntimeType;
			if (runtimeType == null)
			{
				Debug.LogError("No type for class " + node.runtimeTypeName);
				return null;
			}
			EB.Sequence.Runtime.Node node2 = (EB.Sequence.Runtime.Node)Activator.CreateInstance(runtimeType);
			node2.Parent = parent;
			node2.Id = node.id;
			foreach (Property property in node.properties)
			{
				FieldInfo field = runtimeType.GetField(property.name);
				if (field != null && !field.FieldType.IsArray)
				{
					field.SetValue(node2, property.Value);
				}
			}
			foreach (PropertyArray propertyArray in node.propertyArrays)
			{
				FieldInfo field2 = runtimeType.GetField(propertyArray.name);
				if (field2 == null || !field2.FieldType.IsArray)
				{
					Debug.LogWarning("Failed to find property:" + propertyArray.name);
					continue;
				}
				Array array = Array.CreateInstance(field2.FieldType.GetElementType(), propertyArray.items.Count);
				for (int i = 0; i < propertyArray.items.Count; i++)
				{
					array.SetValue(propertyArray.items[i].Value, i);
				}
				field2.SetValue(node2, array);
			}
			node2.Init();
			return node2;
		}

		public static void SetProperyValue(EB.Sequence.Serialization.Node node, string name, object value)
		{
			foreach (Property property in node.properties)
			{
				if (property.name == name)
				{
					property.Value = value;
				}
			}
		}

		public static string EditorName(string name, Type type)
		{
			TriggerAttribute triggerAttribute = GetTriggerAttribute(type, name);
			if (triggerAttribute != null && !string.IsNullOrEmpty(triggerAttribute.EditorName))
			{
				return triggerAttribute.EditorName;
			}
			return NiceName(name);
		}

		public static string NiceName(string name)
		{
			string[] array = name.Split('_');
			name = array[array.Length - 1];
			string text = string.Empty;
			while (name.Length > 0)
			{
				int num = name.IndexOfAny(UpperCase, 1);
				if (num > 0)
				{
					text = text + name.Substring(0, num) + " ";
					name = name.Substring(num);
				}
				else
				{
					text += name;
					name = string.Empty;
				}
			}
			return text;
		}

		public static bool IsCompatableType(Type type1, Type type2)
		{
			type1 = type1 ?? typeof(object);
			type2 = type2 ?? typeof(object);
			if (type1 == type2)
			{
				return true;
			}
			if (type1.IsArray)
			{
				return IsCompatableType(type1.GetElementType(), type2);
			}
			return type1.IsSubclassOf(type2);
		}

		public static bool ValidateTrigger(EB.Sequence.Serialization.Node node, string name)
		{
			FieldInfo field = node.RuntimeType.GetField(FieldName(name));
			if (field != null)
			{
				return IsCompatableType(field.FieldType, typeof(Trigger));
			}
			return false;
		}

		public static bool ValidateVariable(EB.Sequence.Serialization.Node node, string name)
		{
			FieldInfo field = node.RuntimeType.GetField(FieldName(name));
			if (field != null)
			{
				return IsCompatableType(field.FieldType, typeof(Variable));
			}
			if (node.nodeType == NodeType.Variable && name == "Value")
			{
				return true;
			}
			return false;
		}

		public static bool ValidateVariableTypes(EB.Sequence.Serialization.Node variable, EB.Sequence.Serialization.Node input, string name)
		{
			MenuItemAttribute menuAttribute = GetMenuAttribute(variable.RuntimeType);
			VariableAttribute variableAttribute = GetVariableAttribute(input.RuntimeType, FieldName(name));
			if (menuAttribute != null && variableAttribute != null)
			{
				return IsCompatableType(menuAttribute.VariableType, variableAttribute.ExpectedType);
			}
			return false;
		}

		public static bool ValidateEntry(EB.Sequence.Serialization.Node node, string name)
		{
			EntryAttribute entryAttribute = GetEntryAttribute(node.RuntimeType, name);
			if (entryAttribute != null)
			{
				return true;
			}
			return false;
		}

		public static T[] GetObjectList<T>(object value)
		{
			List<T> list = new List<T>();
			if (value != null)
			{
				if (value is T)
				{
					list.Add((T)value);
				}
				else if (value is IEnumerable)
				{
					IEnumerable enumerable = (IEnumerable)value;
					foreach (object item in enumerable)
					{
						list.AddRange(GetObjectList<T>(item));
					}
				}
			}
			return list.ToArray();
		}

		public static bool Filter<T>(Variable variable, T obj)
		{
			if (variable.IsNull)
			{
				return true;
			}
			return Contains(variable.Value, obj);
		}

		public static bool Contains<T>(object value, T obj)
		{
			if (value != null)
			{
				if (value is T)
				{
					return obj.Equals(value);
				}
				if (value is IEnumerable)
				{
					IEnumerable enumerable = (IEnumerable)value;
					foreach (object item in enumerable)
					{
						if (Contains(item, obj))
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		public static LinkFieldInfo ParseField(string name)
		{
			string[] array = name.Split(kSplits, StringSplitOptions.RemoveEmptyEntries);
			LinkFieldInfo result = default(LinkFieldInfo);
			if (array.Length == 1)
			{
				result.field = name;
				result.index = -1;
			}
			else
			{
				result.field = array[0];
				result.index = int.Parse(array[1]);
			}
			return result;
		}

		public static string FieldName(string name)
		{
			int num = name.IndexOf('[');
			if (num >= 0)
			{
				return name.Substring(0, num);
			}
			return name;
		}

		public static LinkType GetLinkType(EB.Sequence.Serialization.Node node, string name)
		{
			if (node.nodeType == NodeType.Variable && (name == "Value" || name == string.Empty))
			{
				return LinkType.Variable;
			}
			string name2 = FieldName(name);
			VariableAttribute variableAttribute = GetVariableAttribute(node.RuntimeType, name2);
			if (variableAttribute != null)
			{
				return LinkType.Variable;
			}
			TriggerAttribute triggerAttribute = GetTriggerAttribute(node.RuntimeType, name2);
			if (triggerAttribute != null)
			{
				return LinkType.Trigger;
			}
			EntryAttribute entryAttribute = GetEntryAttribute(node.RuntimeType, name2);
			if (entryAttribute != null)
			{
				return LinkType.Entry;
			}
			return LinkType.None;
		}

		public static ValidateLinkResult ValidateLink(EB.Sequence.Serialization.Node outNode, string outName, EB.Sequence.Serialization.Node inNode, string inName)
		{
			LinkType linkType = GetLinkType(outNode, outName);
			LinkType linkType2 = GetLinkType(inNode, inName);
			Debug.Log(string.Format("ValidateLink: ({0}:{1} => {2}:{3})", linkType, outName, linkType2, inName));
			switch (linkType)
			{
			case LinkType.Trigger:
				if (ValidateTrigger(outNode, outName) && linkType2 == LinkType.Entry && ValidateEntry(inNode, inName))
				{
					return ValidateLinkResult.Ok;
				}
				break;
			case LinkType.Variable:
				if (ValidateVariable(outNode, outName) && linkType2 == LinkType.Variable && ValidateVariable(inNode, inName))
				{
					if (ValidateVariableTypes(outNode, inNode, inName))
					{
						return ValidateLinkResult.Ok;
					}
					Debug.LogWarning("Invalid Variable Type: " + inName);
					return ValidateLinkResult.InvalidType;
				}
				break;
			}
			return ValidateLinkResult.InvalidLink;
		}
	}
}
