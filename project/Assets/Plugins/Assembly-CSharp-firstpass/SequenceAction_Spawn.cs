using EB;
using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Actions/Spawn")]
public class SequenceAction_Spawn : EB.Sequence.Runtime.Action
{
	[Property]
	public GameObject Prefab;

	[Property]
	public string locatorTarget;

	[Property]
	public float X;

	[Property]
	public float Y;

	[Property]
	public float Z;

	[Variable(ExpectedType = typeof(GameObject), Direction = Direction.Out)]
	public Variable Spawned = Variable.Null;

	[Trigger]
	public Trigger Finished = new Trigger();

	[Entry]
	public void Spawn()
	{
		GameObject prefab = GetPrefab();
		if (prefab == null)
		{
			EB.Debug.LogError("Invalid prefab on node " + base.Id + " parent: " + base.Parent.name);
			return;
		}
		GameObject gameObject = GameObject.Find(locatorTarget);
		if (gameObject != null)
		{
			GameObject gameObject2 = Object.Instantiate(prefab, gameObject.transform.position + new Vector3(X, Y, Z), gameObject.transform.rotation) as GameObject;
			gameObject2.hideFlags = HideFlags.DontSave;
			gameObject2.transform.parent = gameObject.transform;
			Spawned.Value = gameObject2;
		}
		else
		{
			EB.Debug.LogError("Cannot find a suitable locator for spawning Object " + prefab.name);
		}
		Finished.Invoke();
	}

	private GameObject GetPrefab()
	{
		return Prefab;
	}
}
