using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Actions/Random")]
public class SequenceAction_Random : Action
{
	[Property]
	public int[] Probabilities = new int[0];

	private float[] _ranges;

	[Trigger(Show = false)]
	public Trigger[] Triggers = new Trigger[0];

	public override void Init()
	{
		base.Init();
		float num = 0f;
		for (int i = 0; i < Probabilities.Length; i++)
		{
			num += (float)Probabilities[i];
		}
		float num2 = 0f;
		_ranges = new float[Probabilities.Length];
		for (int j = 0; j < Probabilities.Length; j++)
		{
			num2 += (float)Probabilities[j] / num;
			_ranges[j] = num2;
		}
		Triggers = new Trigger[Probabilities.Length];
		for (int k = 0; k < Triggers.Length; k++)
		{
			Triggers[k] = new Trigger();
		}
	}

	[Entry]
	public void RollTheDice()
	{
		Activate();
	}

	public override bool Update()
	{
		float value = Random.value;
		for (int i = 0; i < _ranges.Length; i++)
		{
			if (value <= _ranges[i])
			{
				Triggers[i].Invoke();
				break;
			}
		}
		return false;
	}
}
