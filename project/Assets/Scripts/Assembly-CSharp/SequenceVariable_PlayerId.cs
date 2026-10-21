using EB;
using EB.Sequence;
using EB.Sequence.Runtime;

[MenuItem(Path = "Variables/PlayerId", VariableType = typeof(uint))]
public class SequenceVariable_PlayerId : Variable
{
	private uint _value;

	public override object Value
	{
		get
		{
			return _value;
		}
		set
		{
			uint num = (uint)value;
			if (_value != num)
			{
				_value = num;
				ValueChanged();
			}
		}
	}

	public override void Serialize(BitStream bs)
	{
		bs.Serialize(ref _value);
	}
}
