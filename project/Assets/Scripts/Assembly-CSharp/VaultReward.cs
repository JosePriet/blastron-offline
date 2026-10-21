using UnityEngine;

public class VaultReward : MonoBehaviour
{
	protected int index;

	public int GetIndex()
	{
		return index;
	}

	public virtual void PrepareForAnimation()
	{
	}

	public virtual bool IsReady()
	{
		return true;
	}
}
