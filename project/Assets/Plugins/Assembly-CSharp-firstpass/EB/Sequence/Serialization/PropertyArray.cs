using System;
using System.Collections.Generic;

namespace EB.Sequence.Serialization
{
	[System.Serializable]
	public class PropertyArray : ICloneable
	{
		public string name = string.Empty;

		public PropertyType type;

		public List<Property> items = new List<Property>();

		public bool Resize(int count)
		{
			bool result = false;
			if (count < items.Count)
			{
				items.RemoveRange(count, items.Count - count);
				result = true;
			}
			else
			{
				while (items.Count < count)
				{
					Property property = new Property();
					property.name = string.Format("{0}[{1}]", name, items.Count);
					property.type = type;
					items.Add(property);
					result = true;
				}
			}
			return result;
		}

		public object Clone()
		{
			PropertyArray propertyArray = (PropertyArray)MemberwiseClone();
			propertyArray.items = new List<Property>();
			foreach (Property item in items)
			{
				propertyArray.items.Add((Property)item.Clone());
			}
			return propertyArray;
		}
	}
}
