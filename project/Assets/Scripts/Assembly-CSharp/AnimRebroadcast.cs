using UnityEngine;

public class AnimRebroadcast : MonoBehaviour
{
	private void OnAnimEvent(string stringParm)
	{
		string[] array = stringParm.Split(':');
		if (array.Length == 1)
		{
			SendMessageUpwards(array[0], SendMessageOptions.DontRequireReceiver);
		}
		else if (array.Length == 2)
		{
			SendMessageUpwards(array[0], array[1], SendMessageOptions.DontRequireReceiver);
		}
	}
}
