using UnityEngine;

[ExecuteInEditMode]
public class ReassignMissingTexture : MonoBehaviour
{
	public Texture texture;

	private void Awake()
	{
		UITexture component = base.gameObject.GetComponent<UITexture>();
		if (component != null && component.mainTexture == null)
		{
			component.mainTexture = texture;
		}
	}
}
