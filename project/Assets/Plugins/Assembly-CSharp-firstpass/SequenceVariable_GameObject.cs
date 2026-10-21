using EB;
using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Variables/GameObject", VariableType = typeof(GameObject))]
public class SequenceVariable_GameObject : Variable
{
	private GameObject GameObject;

	public override object Value
	{
		get
		{
			return GameObject;
		}
		set
		{
			GameObject = value as GameObject;
			ValueChanged();
		}
	}

	public override void Serialize(BitStream bs)
	{
	}
}
