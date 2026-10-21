using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Events/OnButtonPressed")]
public class SequenceEvent_OnButtonPressed : EB.Sequence.Runtime.Event
{
	[Property]
	public string TutorialId = string.Empty;

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
