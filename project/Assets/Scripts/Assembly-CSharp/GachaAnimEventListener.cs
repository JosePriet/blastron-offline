using UnityEngine;

public class GachaAnimEventListener : MonoBehaviour
{
	public GachaSpinner target;

	public void PlayAnimEvent(string name)
	{
		target.PlayAnimEvent(name);
	}
}
