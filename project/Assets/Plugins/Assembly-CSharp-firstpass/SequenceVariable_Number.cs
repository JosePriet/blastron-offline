using EB;
using EB.Sequence;
using EB.Sequence.Runtime;

[MenuItem(Path = "Variables/Number", VariableType = typeof(float))]
public class SequenceVariable_Number : Variable
{
	[Property]
	public float IntialValue;

	[Property]
	public string Name = string.Empty;

	private float? _value;

	public bool IsGlobal
	{
		get
		{
			return !string.IsNullOrEmpty(Name);
		}
	}

	public override object Value
	{
		get
		{
			if (IsGlobal)
			{
				return (float)VariableManager.Instance.GetNumber(Name, IntialValue);
			}
			float? value = _value;
			return (!value.HasValue) ? IntialValue : value.Value;
		}
		set
		{
			if (IsGlobal)
			{
				VariableManager.Instance.SetVariable(Name, (float)value);
				return;
			}
			_value = (float)value;
			ValueChanged();
		}
	}

	public override void Init()
	{
		base.Init();
		if (IsGlobal)
		{
			VariableManager.Instance.AddNode(this, Name);
		}
	}

	public override void Dispose()
	{
		if (IsGlobal)
		{
			VariableManager.Instance.RmvNode(this, Name);
		}
		base.Dispose();
	}

	[Entry]
	public void Inc()
	{
		Value = (float)Value + 1f;
	}

	[Entry]
	public void Dec()
	{
		Value = (float)Value - 1f;
	}

	public override void Serialize(BitStream bs)
	{
		if (bs.isWriting)
		{
			float data = (float)Value;
			bs.Serialize(ref data);
		}
		else
		{
			float data2 = 0f;
			bs.Serialize(ref data2);
			Value = data2;
		}
	}
}
