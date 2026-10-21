using SmoothMoves;
using UnityEngine;

public class SMMaterialReplace : MonoBehaviour
{
	public Material OrigMat;

	public Material NewMat;

	private void Start()
	{
		BoneAnimation component = GetComponent<BoneAnimation>();
		if (component != null)
		{
			component.SwapMaterial(OrigMat, NewMat);
		}
	}
}
