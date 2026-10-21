using EB;
using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Events/Campaign Started Event")]
public class SequenceEvent_CampaignStartedEvent : EB.Sequence.Runtime.Event
{
	[Property]
	public string campaignId = string.Empty;

	public override bool CheckActivate(GameObject instigator, object target)
	{
		EB.Debug.Log(string.Concat("target is ", target, ", campaignId = ", campaignId));
		return target.ToString() == campaignId;
	}
}
