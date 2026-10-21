using EB.Sequence;
using EB.Sequence.Runtime;

[MenuItem(Path = "Variables/Part Type", VariableType = typeof(float))]
public class SequenceVariable_PartType : Variable
{
	[Property(MapTo = typeof(CharacterDB.Category))]
	public int _partCategory;

	public override object Value
	{
		get
		{
			return (float)_partCategory;
		}
		set
		{
			float num = (float)value;
			_partCategory = (int)num;
		}
	}
}
