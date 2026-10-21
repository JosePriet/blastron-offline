using UnityEngine;

public class Planet : MonoBehaviour
{
	public float _spinRate;

	private void Update()
	{
		base.transform.RotateAroundLocal(Vector3.up, _spinRate * Time.deltaTime);
	}
}
