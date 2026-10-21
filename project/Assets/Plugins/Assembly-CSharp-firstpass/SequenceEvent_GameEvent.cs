using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Events/Game Event")]
public class SequenceEvent_GameEvent : EB.Sequence.Runtime.Event
{
	[Property]
	public string eventName = string.Empty;

	public override bool CheckActivate(GameObject instigator, object target)
	{
		return target.ToString() == eventName;
	}
}
