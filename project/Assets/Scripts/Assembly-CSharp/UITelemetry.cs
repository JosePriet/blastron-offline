using UnityEngine;

public class UITelemetry : MonoBehaviour
{
	private void Start()
	{
		Telemetry.Event("ui/" + base.name);
	}
}
