using EB.Replication;
using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Actions/RPC")]
public class SequenceAction_RPC : Action
{
	[Property]
	public string name = string.Empty;

	[Property(MapTo = typeof(RPCMode))]
	public int Mode = 2;

	[Variable]
	public Variable[] Args = new Variable[0];

	[Trigger]
	public Trigger Complete = new Trigger();

	[Entry]
	public void Call()
	{
		object[] array = new object[Args.Length];
		for (int i = 0; i < Args.Length; i++)
		{
			array[i] = Args[i].Value;
		}
		Manager.RPC(name, (RPCMode)Mode, array);
		Complete.Invoke();
	}
}
