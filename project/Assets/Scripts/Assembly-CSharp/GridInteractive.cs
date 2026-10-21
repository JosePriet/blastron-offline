using UnityEngine;

public class GridInteractive : MonoBehaviour
{
	protected bool isDrag;

	protected void OnDrag(Vector2 delta)
	{
		isDrag = true;
	}

	protected void OnPress(bool down)
	{
		isDrag = false;
	}

	protected void OnClick()
	{
		SendMessageUpwards("OnItemSelected", base.gameObject);
	}
}
