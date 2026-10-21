using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Events/Player Died (Tutorial)")]
public class SequenceEvent_PlayerDiedTutorial : EB.Sequence.Runtime.Event
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
