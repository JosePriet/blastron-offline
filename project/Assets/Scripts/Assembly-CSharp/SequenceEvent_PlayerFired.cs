using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Events/Player Fired")]
public class SequenceEvent_PlayerFired : EB.Sequence.Runtime.Event
{
	[Variable(ExpectedType = typeof(uint), Direction = Direction.Out)]
	public Variable PlayerId = Variable.Null;

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
		PlayerId.Value = target;
		return true;
	}
}
