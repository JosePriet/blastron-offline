using System;
using System.Reflection;
using EB;
using EB.Sequence;
using EB.Sequence.Runtime;

[MenuItem(Path = "Variables/Tuning", VariableType = typeof(float))]
public class SequenceVariable_Tuning : Variable
{
	[Property(Hint = "Tuning")]
	public string Name = string.Empty;

	private FieldInfo _field;

	public override object Value
	{
		get
		{
			if (_field != null)
			{
				return Convert.ToSingle(_field.GetValue(Tuning.Instance));
			}
			return 0;
		}
		set
		{
		}
	}

	public override void Init()
	{
		base.Init();
		_field = typeof(Tuning).GetField(Name);
		if (_field == null)
		{
			Debug.LogError("Failed to find tuning variable " + Name);
		}
	}
}
