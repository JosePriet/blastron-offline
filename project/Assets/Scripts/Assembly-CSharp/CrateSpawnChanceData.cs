using EB;
using UnityEngine;

[Serializable(Tag = "cscd")]
public class CrateSpawnChanceData : MonoBehaviour
{
	[Serializable(Tag = "tsd")]
	public CrateManager.ChanceToSpawn[] _turnSpawnData;
}
