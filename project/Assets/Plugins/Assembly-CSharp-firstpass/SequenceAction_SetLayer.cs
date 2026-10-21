using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Actions/Set Layer")]
public class SequenceAction_SetLayer : Action
{
	[Variable(ExpectedType = typeof(GameObject))]
	public Variable Target = Variable.Null;

	[Property]
	public string Layer = string.Empty;

	private int _layer;

	[Trigger]
	public Trigger Finished = new Trigger();

	public override void Init()
	{
		base.Init();
		if (!string.IsNullOrEmpty(Layer))
		{
			_layer = LayerMask.NameToLayer(Layer);
		}
	}

	[Entry]
	public void SetLayer()
	{
		GameObject[] objectList = Utils.GetObjectList<GameObject>(Target.Value);
		GameObject[] array = objectList;
		foreach (GameObject gameObject in array)
		{
			if (gameObject != null)
			{
				gameObject.layer = _layer;
			}
		}
		Finished.Invoke();
	}
}
