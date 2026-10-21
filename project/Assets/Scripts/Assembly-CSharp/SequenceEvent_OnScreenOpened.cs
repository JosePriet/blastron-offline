using EB;
using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Events/OnScreenOpened")]
public class SequenceEvent_OnScreenOpened : EB.Sequence.Runtime.Event
{
	[Property]
	public string ScreenId = string.Empty;

	[Variable(ExpectedType = typeof(string))]
	public Variable Target = Variable.Null;

	public override bool CheckActivate(GameObject instigator, object target)
	{
		if (target != null && target is string)
		{
			string text = target as string;
			if (!Target.IsNull)
			{
				string text2 = Target.Value as string;
				EB.Debug.LogWarning(string.Format("Checking {0} against {1}", text2, (string)target));
				return text2 == (string)target;
			}
			return ScreenId == text;
		}
		return true;
	}
}
