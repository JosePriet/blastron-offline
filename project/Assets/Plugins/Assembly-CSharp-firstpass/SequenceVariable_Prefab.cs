using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Variables/Prefab", VariableType = typeof(GameObject))]
public class SequenceVariable_Prefab : Variable
{
	[Property]
	public GameObject Prefab;

	public override object Value
	{
		get
		{
			return Prefab;
		}
		set
		{
			throw new Exception(this, "Invalid Operation");
		}
	}
}
