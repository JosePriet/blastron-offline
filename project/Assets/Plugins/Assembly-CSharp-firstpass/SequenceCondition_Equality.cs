using EB;
using EB.Sequence;
using EB.Sequence.Runtime;

[MenuItem(Path = "Conditions/Equality")]
public class SequenceCondition_Equality : Condition
{
	[Variable(ExpectedType = typeof(float))]
	public Variable A = Variable.Null;

	[Variable(ExpectedType = typeof(float))]
	public Variable B = Variable.Null;

	[Trigger(EditorName = "A == B")]
	public Trigger Check1 = new Trigger();

	[Trigger(EditorName = "A > B")]
	public Trigger Check2 = new Trigger();

	[Trigger(EditorName = "A < B")]
	public Trigger Check3 = new Trigger();

	[Trigger(EditorName = "A >= B")]
	public Trigger Check4 = new Trigger();

	[Trigger(EditorName = "A <= B")]
	public Trigger Check5 = new Trigger();

	[Trigger(EditorName = "A != B")]
	public Trigger Check6 = new Trigger();

	public override bool Update()
	{
		object value = A.Value;
		object value2 = B.Value;
		if (value != null && value2 != null)
		{
			float num = (float)value;
			float num2 = (float)value2;
			if (num == num2)
			{
				Check1.Invoke();
			}
			if (num > num2)
			{
				Check2.Invoke();
			}
			if (num < num2)
			{
				Check3.Invoke();
			}
			if (num >= num2)
			{
				Check4.Invoke();
			}
			if (num <= num2)
			{
				Check5.Invoke();
			}
			if (num != num2)
			{
				Check6.Invoke();
			}
		}
		else
		{
			Debug.Log("Equality test failed, item A or B is null - Parent:" + base.Parent.name);
		}
		return false;
	}
}
