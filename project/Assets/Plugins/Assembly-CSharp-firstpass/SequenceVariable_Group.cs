using System.Collections;
using System.Collections.Generic;
using EB;
using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Variables/Group", VariableType = typeof(GameObject))]
public class SequenceVariable_Group : Variable
{
	private List<object> _items = new List<object>();

	public override object Value
	{
		get
		{
			return _items.ToArray();
		}
		set
		{
			if (value != null)
			{
				if (value is IEnumerable)
				{
					IEnumerable enumerable = (IEnumerable)value;
					{
						foreach (object item in enumerable)
						{
							_items.Add(item);
						}
						return;
					}
				}
				_items.Add(value);
			}
			else
			{
				_items.Clear();
			}
		}
	}

	public override void Serialize(BitStream bs)
	{
	}
}
