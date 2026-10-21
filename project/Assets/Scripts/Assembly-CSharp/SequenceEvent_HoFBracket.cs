using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Events/HoF Bracket Change")]
public class SequenceEvent_HoFBracket : EB.Sequence.Runtime.Event
{
	[Property]
	public int bracketToTriggerOn;

	public override bool CheckActivate(GameObject instigator, object target)
	{
		if (bracketToTriggerOn == (int)target)
		{
			return true;
		}
		return false;
	}
}
