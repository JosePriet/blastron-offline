using EB;
using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Actions/Timer")]
public class SequenceAction_Timer : EB.Sequence.Runtime.Action
{
	[Variable(ExpectedType = typeof(float))]
	public Variable Interval = Variable.Null;

	[Property]
	public bool OneShot;

	private float _remaining;

	private bool _running;

	[Variable(ExpectedType = typeof(float))]
	public Variable PercentTimeElapsed = Variable.Null;

	[Trigger]
	public Trigger OnTimer = new Trigger();

	[Entry]
	public void Start()
	{
		Activate();
		_remaining = Interval.GetValue<float>();
		_running = true;
	}

	[Entry]
	public void Stop()
	{
		_running = false;
	}

	public override void Dispose()
	{
		Stop();
		base.Dispose();
	}

	public override bool Update()
	{
		if (!_running)
		{
			return false;
		}
		_remaining -= UnityEngine.Time.deltaTime;
		PercentTimeElapsed.Value = 1f - _remaining / Interval.GetValue<float>();
		if (_remaining <= 0f)
		{
			OnTimer.Invoke();
			if (OneShot)
			{
				return false;
			}
			Start();
			return true;
		}
		return true;
	}

	public override void Serialize(BitStream bs)
	{
		bs.Serialize(ref _remaining);
		bs.Serialize(ref _running);
		if (bs.isReading)
		{
			PercentTimeElapsed.Value = 1f - _remaining / Interval.GetValue<float>();
		}
	}
}
