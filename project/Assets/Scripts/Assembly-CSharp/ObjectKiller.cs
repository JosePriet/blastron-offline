using UnityEngine;

public class ObjectKiller : MonoBehaviour
{
	public float _killFloor = -100f;

	private void Update()
	{
		if (base.transform.position.y < _killFloor)
		{
			base.gameObject.SendMessage("OnObjectKillerDeath", SendMessageOptions.DontRequireReceiver);
			Object.Destroy(base.gameObject);
		}
	}
}
