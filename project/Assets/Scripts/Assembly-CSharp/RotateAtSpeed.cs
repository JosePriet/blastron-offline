using UnityEngine;

public class RotateAtSpeed : MonoBehaviour
{
	public float speed;

	private void Start()
	{
	}

	private void Update()
	{
		base.transform.Rotate(new Vector3(0f, 0f, speed * Time.deltaTime));
	}
}
