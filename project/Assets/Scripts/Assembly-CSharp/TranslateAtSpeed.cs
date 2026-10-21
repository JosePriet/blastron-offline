using UnityEngine;

public class TranslateAtSpeed : MonoBehaviour
{
	public float X_Speed;

	public float Y_Speed;

	public float Z_Speed;

	private void Start()
	{
	}

	private void Update()
	{
		base.transform.Translate(new Vector3(X_Speed * Time.deltaTime, Y_Speed * Time.deltaTime, Z_Speed * Time.deltaTime));
	}
}
