using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Variables/Locator", VariableType = typeof(GameObject))]
public class SequenceVariable_Locator : Variable
{
	[Property(Hint = "Scene")]
	public string targetScene = string.Empty;

	[Property(Description = "Locator Name", Hint = "Locator")]
	public string LocatorName = string.Empty;

	public override object Value
	{
		get
		{
			return GameObject.Find(LocatorName);
		}
		set
		{
			throw new Exception(this, "Invalid Operation");
		}
	}
}
