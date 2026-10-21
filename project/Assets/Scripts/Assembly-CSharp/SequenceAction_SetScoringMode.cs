using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Actions/Set Scoring Mode")]
public class SequenceAction_SetScoringMode : Action
{
	[Property(MapTo = typeof(Scoring.ScoringMode))]
	public int Mode;

	[Entry]
	public void Set()
	{
		Scoring instance = Scoring.Instance;
		instance.GetComponent<ReplicationView>().RPC("SetScoringMode", RPCMode.All, Mode);
	}
}
