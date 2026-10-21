using EB;
using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Variables/Sequence", VariableType = typeof(GameObject))]
public class SequenceVariable_Sequence : Variable
{
	[Property(Hint = "Sequence", NonEditable = true)]
	public string SequenceId = string.Empty;

	public override object Value
	{
		get
		{
			GameObject gameObject = GameObject.Find(SequenceId);
			if (gameObject != null)
			{
				EB.Sequence.Component component = gameObject.GetComponent<EB.Sequence.Component>();
				if (component == null)
				{
					throw new Exception(this, "ERROR - SEQUENCE VARIABLE NOT A VALID SEQUENCE - SEQUENCE ID:" + SequenceId);
				}
			}
			else
			{
				EB.Debug.Log("SEQUNVARIABLE_SEQUENCE ERROR - CANNOT FIND SEQUENCE " + SequenceId);
			}
			return gameObject;
		}
		set
		{
		}
	}
}
