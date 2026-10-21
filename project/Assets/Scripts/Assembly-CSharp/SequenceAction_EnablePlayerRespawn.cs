using EB.Replication;
using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Actions/Enable Player Respawn")]
public class SequenceAction_EnablePlayerRespawn : Action
{
	[Property]
	public bool enable = true;

	[Property]
	public bool localPlayerEnableOverride;

	[Trigger]
	public Trigger Finished = new Trigger();

	[Entry]
	public void Process()
	{
		Manager.RPC("SetPlayerRespawn", RPCMode.AllBuffered, enable, localPlayerEnableOverride);
		Finished.Invoke();
	}
}
