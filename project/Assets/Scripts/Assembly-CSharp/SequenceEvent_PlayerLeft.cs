using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Events/Player Left")]
public class SequenceEvent_PlayerLeft : EB.Sequence.Runtime.Event
{
	[Variable(ExpectedType = typeof(uint))]
	public Variable Filter = Variable.Null;

	public override bool CheckActivate(GameObject instigator, object target)
	{
		if (!Filter.IsNull)
		{
			uint value = Filter.GetValue<uint>();
			if (value != (uint)target)
			{
				return false;
			}
		}
		return true;
	}
}
