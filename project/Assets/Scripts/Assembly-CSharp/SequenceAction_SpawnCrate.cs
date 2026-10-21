using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Actions/SpawnCrate")]
public class SequenceAction_SpawnCrate : Action
{
	[Property]
	public GameObject cratePrefab;

	[Property(MapTo = typeof(Crate.Type))]
	public int crateType;

	[Property]
	public string spawnPoint;

	[Property]
	public string tutorialId;

	[Property]
	public Vector3 tutorialArrowOffset;

	[Trigger]
	public Trigger Finished = new Trigger();

	[Entry]
	public void Spawn()
	{
		CrateManager.Instance.SequenceSpawnCrate(cratePrefab, (Crate.Type)crateType, spawnPoint, tutorialId, tutorialArrowOffset);
		Finished.Invoke();
	}
}
