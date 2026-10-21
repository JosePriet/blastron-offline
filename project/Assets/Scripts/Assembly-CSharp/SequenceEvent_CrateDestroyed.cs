using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Events/Crate Destroyed")]
public class SequenceEvent_CrateDestroyed : EB.Sequence.Runtime.Event
{
	[Property]
	public string TutorialId;

	public override bool CheckActivate(GameObject instigator, object target)
	{
		string text = target.ToString();
		if (!string.IsNullOrEmpty(TutorialId))
		{
			return TutorialId == text;
		}
		return true;
	}
}
