using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Events/OnScreenStarted")]
public class SequenceEvent_OnScreenStarted : EB.Sequence.Runtime.Event
{
	[Property]
	public string ScreenId = string.Empty;

	public override bool CheckActivate(GameObject instigator, object target)
	{
		string text = target.ToString();
		if (!string.IsNullOrEmpty(ScreenId))
		{
			return ScreenId == text;
		}
		return true;
	}
}
