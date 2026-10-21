using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Events/Begin Turn")]
public class SequenceEvent_BeginTurn : EB.Sequence.Runtime.Event
{
	[Variable(ExpectedType = typeof(uint), Direction = Direction.Out)]
	public Variable TriggeringPlayerId = Variable.Null;

	[Property]
	public override bool CheckActivate(GameObject instigator, object target)
	{
		TriggeringPlayerId.Value = target;
		return true;
	}
}
