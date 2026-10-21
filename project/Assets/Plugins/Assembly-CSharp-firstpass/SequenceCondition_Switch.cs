using EB.Sequence;
using EB.Sequence.Runtime;

[MenuItem(Path = "Conditions/Switch")]
public class SequenceCondition_Switch : Condition
{
	[Variable(ExpectedType = typeof(object))]
	public Variable Value = Variable.Null;

	[Property]
	public string[] TestValues = new string[0];

	[Trigger(Show = false)]
	public Trigger[] Triggers = new Trigger[0];

	[Trigger(EditorName = "Default")]
	public Trigger Default = new Trigger();

	public override void Init()
	{
		base.Init();
		Triggers = new Trigger[TestValues.Length];
		for (int i = 0; i < Triggers.Length; i++)
		{
			Triggers[i] = new Trigger();
		}
	}

	public override bool Update()
	{
		string text = Value.Value.ToString();
		for (int i = 0; i < TestValues.Length; i++)
		{
			if (TestValues[i] == text)
			{
				Triggers[i].Invoke();
				return false;
			}
		}
		Default.Invoke();
		return false;
	}
}
