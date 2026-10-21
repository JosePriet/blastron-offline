using EB;
using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Events/OnUpgradeComplete")]
public class SequenceEvent_OnUpgradeComplete : EB.Sequence.Runtime.Event
{
	public class PartEventInfo
	{
		public int PartType { get; private set; }

		public int Tier { get; private set; }

		public PartEventInfo(int pt, int t)
		{
			PartType = pt;
			Tier = t;
		}

		public override string ToString()
		{
			return string.Format("PartType {0}, Tier {1}", PartType, Tier);
		}
	}

	[Variable(ExpectedType = typeof(float), Direction = Direction.Out)]
	public Variable PartCategoryOut = Variable.Null;

	[Variable(ExpectedType = typeof(float), Direction = Direction.Out)]
	public Variable TierOut = Variable.Null;

	public override bool CheckActivate(GameObject instigator, object target)
	{
		PartEventInfo partEventInfo = target as PartEventInfo;
		if (partEventInfo != null)
		{
			EB.Debug.LogError(string.Format("UpgradeEvent checking {0}", partEventInfo.ToString()));
			PartCategoryOut.Value = (float)partEventInfo.PartType;
			TierOut.Value = (float)partEventInfo.Tier;
			return true;
		}
		return true;
	}
}
