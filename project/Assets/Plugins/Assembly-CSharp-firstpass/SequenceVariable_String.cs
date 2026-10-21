using EB;
using EB.Sequence;
using EB.Sequence.Runtime;

[MenuItem(Path = "Variables/String", VariableType = typeof(string))]
public class SequenceVariable_String : Variable
{
	[Property]
	public string IntialValue = string.Empty;

	[Property]
	public string Name = string.Empty;

	private string _value;

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
				return VariableManager.Instance.GetString(Name, IntialValue);
			}
			return _value ?? IntialValue;
		}
		set
		{
			if (IsGlobal)
			{
				VariableManager.Instance.SetVariable(Name, (string)value);
				return;
			}
			_value = (string)value;
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

	public override void Serialize(BitStream bs)
	{
		if (bs.isWriting)
		{
			string data = (string)Value;
			bs.Serialize(ref data);
		}
		else
		{
			string data2 = string.Empty;
			bs.Serialize(ref data2);
			Value = data2;
		}
	}
}
