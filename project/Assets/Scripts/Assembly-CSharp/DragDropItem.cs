using UnityEngine;

[AddComponentMenu("NGUI/Examples/Drag & Drop Item")]
public class DragDropItem : MonoBehaviour
{
	public GameObject prefab;

	private Transform mTrans;

	private bool mIsDragging;

	private Transform mParent;

	private void UpdateTable()
	{
		UITable uITable = NGUITools.FindInParents<UITable>(base.gameObject);
		if (uITable != null)
		{
			uITable.repositionNow = true;
		}
	}

	private void Drop()
	{
		Collider collider = UICamera.lastHit.collider;
		DragDropContainer dragDropContainer = ((!(collider != null)) ? null : collider.gameObject.GetComponent<DragDropContainer>());
		if (dragDropContainer != null)
		{
			mTrans.parent = dragDropContainer.transform;
			Vector3 localPosition = mTrans.localPosition;
			localPosition.z = 0f;
			mTrans.localPosition = localPosition;
		}
		else
		{
			mTrans.parent = mParent;
		}
		UpdateTable();
		BroadcastMessage("CheckParent", SendMessageOptions.DontRequireReceiver);
	}

	private void Awake()
	{
		mTrans = base.transform;
	}

	private void OnDrag(Vector2 delta)
	{
		if (UICamera.currentTouchID > -2)
		{
			if (!mIsDragging)
			{
				mIsDragging = true;
				mParent = mTrans.parent;
				mTrans.parent = DragDropRoot.root;
				Vector3 localPosition = mTrans.localPosition;
				localPosition.z = 0f;
				mTrans.localPosition = localPosition;
				mTrans.BroadcastMessage("CheckParent", SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				mTrans.localPosition += (Vector3)delta;
			}
		}
	}

	private void OnPress(bool isPressed)
	{
		mIsDragging = false;
		Collider collider = base.collider;
		if (collider != null)
		{
			collider.enabled = !isPressed;
		}
		if (!isPressed)
		{
			Drop();
		}
	}
}
