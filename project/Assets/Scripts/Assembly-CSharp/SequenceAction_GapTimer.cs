using EB;
using EB.Replication;
using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Actions/Gap Timer")]
public class SequenceAction_GapTimer : EB.Sequence.Runtime.Action
{
	[Variable(ExpectedType = typeof(float))]
	public Variable Interval = Variable.Null;

	[Variable(ExpectedType = typeof(uint))]
	public Variable NextPlayer = Variable.Null;

	private float _remaining;

	[Trigger]
	public Trigger Complete = new Trigger();

	[Entry]
	public void Start()
	{
		_remaining = Interval.GetValue<float>();
		uint value = NextPlayer.GetValue<uint>();
		Manager.RPC("StartGap", RPCMode.All, value, _remaining);
		Activate();
	}

	public override bool Update()
	{
		_remaining -= UnityEngine.Time.deltaTime;
		if (_remaining <= 0f)
		{
			Manager.RPC("EndGap", RPCMode.All, NextPlayer.GetValue<uint>());
			Complete.Invoke();
			return false;
		}
		return true;
	}

	public override void Serialize(BitStream bs)
	{
		bs.Serialize(ref _remaining);
	}
}
