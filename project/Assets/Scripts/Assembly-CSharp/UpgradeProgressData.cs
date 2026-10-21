using EB;
using UnityEngine;

[Serializable(Tag = "upd")]
public class UpgradeProgressData : MonoBehaviour
{
	[Serializable(Tag = "pbs")]
	public PartAttributes.Bracket[] ProgressBrackets;
}
