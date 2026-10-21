using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Actions/Play Player Anim")]
public class SequenceAction_PlayerPlayAnim : Action
{
	[Property]
	public string animName;

	[Trigger]
	public Trigger Finished = new Trigger();

	[Entry]
	public void Process()
	{
		Player player = LevelData.Instance.GetPlayer();
		player.PlayIdle();
		player.PlayAnim(animName, PlayMode.StopSameLayer, true);
		Finished.Invoke();
	}
}
