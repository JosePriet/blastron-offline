using UnityEngine;

public class PrefabRef : MonoBehaviour
{
	public Object Prefab;

	private void Awake()
	{
		if (Prefab != null)
		{
			GameObject gameObject = (GameObject)Object.Instantiate(Prefab, base.transform.position, base.transform.rotation);
			gameObject.name = base.name;
			Object.Destroy(base.gameObject);
		}
	}
}
