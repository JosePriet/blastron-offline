using EB;
using UnityEngine;

[Serializable(Tag = "csd")]
public class CrateSpawnData : MonoBehaviour
{
	[Serializable(Tag = "sd")]
	public CrateManager.TypeSpawningData[] _spawnData;
}
