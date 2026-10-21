using System;
using System.Collections.Generic;
using UnityEngine;

namespace EB.Sequence.Serialization
{
	[System.Serializable]
	public class Node : ICloneable
	{
		public NodeType nodeType;

		public string comment = string.Empty;

		public Rect rect = new Rect(0f, 0f, 100f, 100f);

		public int id;

		public string runtimeTypeName = string.Empty;

		public List<Property> properties = new List<Property>();

		public List<PropertyArray> propertyArrays = new List<PropertyArray>();

		public Type RuntimeType
		{
			get
			{
				return Utils.GetTypeFromName(runtimeTypeName);
			}
			set
			{
				runtimeTypeName = value.Name;
			}
		}

		public int GetPropertyCount()
		{
			int num = properties.Count;
			foreach (PropertyArray propertyArray in propertyArrays)
			{
				num += 1 + propertyArray.items.Count;
			}
			return num;
		}

		public Property FindByHint(string hint)
		{
			foreach (Property property in properties)
			{
				PropertyAttribute propertyAttribute = GetPropertyAttribute(property.name);
				if (propertyAttribute != null && propertyAttribute.Hint == hint)
				{
					return property;
				}
			}
			return null;
		}

		public PropertyAttribute GetPropertyAttribute(string name)
		{
			try
			{
				return Utils.GetPropertyAttribute(RuntimeType, name);
			}
			catch
			{
			}
			return null;
		}

		public Property GetOrAddProperty(string name)
		{
			foreach (Property property2 in properties)
			{
				if (property2.name == name)
				{
					return property2;
				}
			}
			Property property = new Property();
			property.name = name;
			properties.Add(property);
			return property;
		}

		public Property GetProperty(string name)
		{
			foreach (Property property in properties)
			{
				if (property.name == name)
				{
					return property;
				}
			}
			return null;
		}

		public PropertyArray GetPropertyArray(string name)
		{
			foreach (PropertyArray propertyArray in propertyArrays)
			{
				if (propertyArray.name == name)
				{
					return propertyArray;
				}
			}
			return null;
		}

		public object Clone()
		{
			Node node = (Node)MemberwiseClone();
			node.properties = new List<Property>();
			node.propertyArrays = new List<PropertyArray>();
			foreach (Property property in properties)
			{
				node.properties.Add((Property)property.Clone());
			}
			foreach (PropertyArray propertyArray in propertyArrays)
			{
				node.propertyArrays.Add((PropertyArray)propertyArray.Clone());
			}
			return node;
		}
	}
}
