using EB.Sequence;
using EB.Sequence.Runtime;

[MenuItem(Path = "Conditions/Boolean Evaluation")]
public class SequenceCondition_BooleanEvaluation : Condition
{
	[Property]
	public int Inputs;

	[Property]
	public string[] Evaluations = new string[0];

	[Variable(ExpectedType = typeof(float), Show = false)]
	public Variable[] Input = new Variable[0];

	[Trigger(Show = false)]
	public Trigger[] Triggers = new Trigger[0];

	public override void Init()
	{
		base.Init();
		Triggers = new Trigger[Input.Length];
		for (int i = 0; i < Triggers.Length; i++)
		{
			Triggers[i] = new Trigger();
		}
	}

	public void EvaluationExpression(string eval, Trigger trigger)
	{
		if (eval.Length == 0)
		{
			return;
		}
		string text = eval;
		for (int i = 0; i < Input.Length; i++)
		{
			Variable variable = Input[i] ?? Variable.Null;
			object value = variable.Value;
			if (value != null)
			{
				text = text.Replace(Node.kAlphabet.Substring(i, 1), value.ToString());
			}
		}
		if (BooleanEvaluator.Evaluate(text) && trigger != null)
		{
			trigger.Invoke();
		}
	}

	public override bool Update()
	{
		for (int i = 0; i < Evaluations.Length; i++)
		{
			EvaluationExpression(Evaluations[i], Triggers[i]);
		}
		return false;
	}
}
