using UnityEngine;

public class Persistent : MonoBehaviour
{
	public void Awake()
	{
		Object.DontDestroyOnLoad(base.gameObject);
	}
}
