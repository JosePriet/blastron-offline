using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Events/Damage Taken")]
public class SequenceEvent_DamageTaken : EB.Sequence.Runtime.Event
{
	[Variable(ExpectedType = typeof(uint), Direction = Direction.Out)]
	public Variable PlayerId = Variable.Null;

	[Variable(ExpectedType = typeof(float), Direction = Direction.Out)]
	public Variable PlayerHealthPercent = Variable.Null;

	public override bool CheckActivate(GameObject instigator, object target)
	{
		if (target != null)
		{
			Player player = LevelData.Instance.GetPlayer((uint)target);
			PlayerId.Value = target;
			PlayerHealthPercent.Value = player.HealthPercent;
		}
		return true;
	}
}
