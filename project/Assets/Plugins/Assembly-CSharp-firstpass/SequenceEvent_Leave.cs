using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Events/Leave")]
public class SequenceEvent_Leave : EB.Sequence.Runtime.Event
{
	[Variable(ExpectedType = typeof(GameObject))]
	public Variable Source = Variable.Null;

	[Variable(ExpectedType = typeof(GameObject))]
	public Variable Target = Variable.Null;

	[Variable(ExpectedType = typeof(GameObject), Direction = Direction.Out)]
	public Variable SourceOut = Variable.Null;

	[Variable(ExpectedType = typeof(GameObject), Direction = Direction.Out)]
	public Variable TargetOut = Variable.Null;

	public override bool CheckActivate(GameObject instigator, object target)
	{
		if (CheckFilter(instigator, Source) && CheckFilter(target as GameObject, Target))
		{
			SourceOut.Value = instigator;
			TargetOut.Value = target;
			return true;
		}
		return false;
	}

	private bool CheckFilter(GameObject go, Variable filter)
	{
		if (filter.IsNull)
		{
			return true;
		}
		if (Utils.Contains(filter.Value, go))
		{
			return true;
		}
		return false;
	}
}
