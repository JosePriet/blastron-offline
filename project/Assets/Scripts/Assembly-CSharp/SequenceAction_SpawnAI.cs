using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Actions/SpawnAI")]
public class SequenceAction_SpawnAI : Action
{
	[Property]
	public int aiIndex;

	[Property]
	public string spawnPoint;

	[Property]
	public string tutorialId;

	[Property]
	public Vector3 tutorialArrowOffset;

	[Property]
	public float _healthPercent = 100f;

	[Trigger]
	public Trigger Finished = new Trigger();

	[Entry]
	public void Spawn()
	{
		LevelData.Instance.SpawnAI(aiIndex, spawnPoint, tutorialId, tutorialArrowOffset, _healthPercent);
		Finished.Invoke();
	}
}
