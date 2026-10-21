using UnityEngine;

public class FSUserDataComponent : MonoBehaviour
{
	public bool SetUserDataAsGameObject;

	private void Start()
	{
		FSBodyComponent component = GetComponent<FSBodyComponent>();
		if (component != null && SetUserDataAsGameObject)
		{
			component.PhysicsBody.UserData = base.gameObject;
		}
	}
}
