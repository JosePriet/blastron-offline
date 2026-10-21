using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Anchor")]
public class UIAnchor : MonoBehaviour
{
	public enum Side
	{
		BottomLeft = 0,
		Left = 1,
		TopLeft = 2,
		Top = 3,
		TopRight = 4,
		Right = 5,
		BottomRight = 6,
		Bottom = 7,
		Center = 8
	}

	private bool mIsWindows;

	public bool disableWhenReady = true;

	private int destroyCount;

	public Camera uiCamera;

	public UIWidget widgetContainer;

	public UIPanel panelContainer;

	public Side side = Side.Center;

	public bool halfPixelOffset = true;

	public float depthOffset;

	public Vector2 relativeOffset = Vector2.zero;

	private UIRoot _root;

	private void OnEnable()
	{
		mIsWindows = Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsWebPlayer || Application.platform == RuntimePlatform.WindowsEditor;
		if (uiCamera == null)
		{
			uiCamera = NGUITools.FindCameraForLayer(base.gameObject.layer);
		}
		Update();
	}

	private void Update()
	{
		if (destroyCount >= 30)
		{
			if (disableWhenReady)
			{
				base.enabled = false;
				return;
			}
			destroyCount = 0;
		}
		Rect rect = default(Rect);
		bool flag = false;
		if (_root == null)
		{
			_root = NGUITools.FindInParents<UIRoot>(base.gameObject);
		}
		if (panelContainer != null)
		{
			if (panelContainer.clipping == UIDrawCall.Clipping.None)
			{
				float num = Screen.width;
				float num2 = Screen.height;
				if (_root != null && !_root.automatic && num2 > 1f)
				{
					float num3 = (float)_root.manualHeight / num2;
					num *= num3;
					num2 *= num3;
				}
				rect.xMin = (0f - num) * 0.5f;
				rect.yMin = (0f - num2) * 0.5f;
				rect.xMax = 0f - rect.xMin;
				rect.yMax = 0f - rect.yMin;
			}
			else
			{
				Vector4 clipRange = panelContainer.clipRange;
				rect.x = clipRange.x - clipRange.z * 0.5f;
				rect.y = clipRange.y - clipRange.w * 0.5f;
				rect.width = clipRange.z;
				rect.height = clipRange.w;
			}
		}
		else if (widgetContainer != null)
		{
			Transform cachedTransform = widgetContainer.cachedTransform;
			Vector3 localScale = cachedTransform.localScale;
			Vector3 localPosition = cachedTransform.localPosition;
			Vector3 vector = widgetContainer.pivotOffset;
			vector.y -= widgetContainer.relativeSize.y;
			vector.x *= widgetContainer.relativeSize.x * localScale.x;
			vector.y *= widgetContainer.relativeSize.y * localScale.y;
			rect.x = localPosition.x + vector.x;
			rect.y = localPosition.y + vector.y;
			rect.width = localScale.x;
			rect.height = localScale.y;
		}
		else
		{
			if (!(uiCamera != null))
			{
				return;
			}
			flag = true;
			rect = uiCamera.pixelRect;
		}
		float x = (rect.xMin + rect.xMax) * 0.5f;
		float y = (rect.yMin + rect.yMax) * 0.5f;
		Vector3 vector2 = new Vector3(x, y, depthOffset);
		if (side != Side.Center)
		{
			if (side == Side.Right || side == Side.TopRight || side == Side.BottomRight)
			{
				vector2.x = rect.xMax;
			}
			else if (side == Side.Top || side == Side.Center || side == Side.Bottom)
			{
				vector2.x = x;
			}
			else
			{
				vector2.x = rect.xMin;
			}
			if (side == Side.Top || side == Side.TopRight || side == Side.TopLeft)
			{
				vector2.y = rect.yMax;
			}
			else if (side == Side.Left || side == Side.Center || side == Side.Right)
			{
				vector2.y = y;
			}
			else
			{
				vector2.y = rect.yMin;
			}
		}
		float width = rect.width;
		float height = rect.height;
		vector2.x += relativeOffset.x * width;
		vector2.y += relativeOffset.y * height;
		if (flag)
		{
			float z = base.gameObject.transform.position.z;
			if (uiCamera.orthographic)
			{
				vector2.x = Mathf.RoundToInt(vector2.x);
				vector2.y = Mathf.RoundToInt(vector2.y);
				if (halfPixelOffset && mIsWindows)
				{
					vector2.x -= 0.5f;
					vector2.y += 0.5f;
				}
			}
			vector2 = uiCamera.ScreenToWorldPoint(vector2);
			vector2.z = z;
			if (base.transform.position != vector2)
			{
				base.transform.position = vector2;
			}
		}
		else
		{
			vector2.x = Mathf.RoundToInt(vector2.x);
			vector2.y = Mathf.RoundToInt(vector2.y);
			float z2 = base.gameObject.transform.localPosition.z;
			vector2.z = z2;
			if (base.transform.localPosition != vector2)
			{
				base.transform.localPosition = vector2;
			}
		}
		if (disableWhenReady && Application.isPlaying)
		{
			destroyCount++;
		}
	}
}
