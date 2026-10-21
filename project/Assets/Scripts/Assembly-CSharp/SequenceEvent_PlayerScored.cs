using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Events/Player Scored")]
public class SequenceEvent_PlayerScored : EB.Sequence.Runtime.Event
{
	[Variable(ExpectedType = typeof(uint), Direction = Direction.Out)]
	public Variable PlayerId = Variable.Null;

	public override bool CheckActivate(GameObject instigator, object target)
	{
		PlayerId.Value = target;
		return true;
	}
}
