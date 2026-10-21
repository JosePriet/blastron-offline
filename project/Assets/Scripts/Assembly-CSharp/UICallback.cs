using UnityEngine;

[RequireComponent(typeof(Collider))]
[AddComponentMenu("NGUI/UICallback")]
public class UICallback : MonoBehaviour
{
	public MonoBehaviour Target;

	public string Method;

	private void OnClick()
	{
		if (Target != null)
		{
			Target.SendMessage(Method, base.gameObject, SendMessageOptions.RequireReceiver);
		}
		else
		{
			base.gameObject.SendMessage(Method, base.gameObject, SendMessageOptions.RequireReceiver);
		}
	}
}
