using System.Collections.Generic;
using EB;
using EB.Sequence;
using EB.Sequence.Runtime;

[MenuItem(Path = "Actions/Log")]
public class SequenceAction_Log : EB.Sequence.Runtime.Action
{
	[Property]
	public string Format = string.Empty;

	[Variable]
	public Variable[] Args = new Variable[0];

	[Entry]
	public void Log()
	{
		List<object> list = new List<object>();
		Variable[] args = Args;
		foreach (Variable variable in args)
		{
			list.Add(variable.Value);
		}
		Debug.Log(Format, list.ToArray());
	}
}
