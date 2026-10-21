using EB;
using UnityEngine;

public class Emote : MonoBehaviour
{
	public float lifetime;

	public Vector2 offset;

	private float startTime;

	private Camera uiCamera;

	private GameObject playerObj;

	public UIAtlas Emote_Atlas;

	public string Emote_SpriteName;

	private void Start()
	{
		startTime = UnityEngine.Time.time;
	}

	public void Init(GameObject playerObject, string spriteName)
	{
		playerObj = playerObject;
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "Sprite");
		UISprite uISprite = objectExactMatch.AddComponent<UISprite>();
		uISprite.atlas = Emote_Atlas;
		uISprite.spriteName = spriteName;
		uISprite.depth = 1;
		uISprite.MakePixelPerfect();
	}

	private void Update()
	{
		if (uiCamera == null)
		{
			uiCamera = NGUITools.FindCameraForLayer(base.gameObject.layer);
		}
		if (playerObj != null)
		{
			Player component = playerObj.GetComponent<Player>();
			Vector2 vector = new Vector2(offset.x, offset.y);
			GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "base");
			Vector3 localScale = objectExactMatch.transform.localScale;
			if (component.RightFacing)
			{
				localScale.x = Mathf.Abs(localScale.x);
			}
			else
			{
				vector.x = -1f * offset.x;
				localScale.x = -1f * Mathf.Abs(localScale.x);
			}
			GameObject objectExactMatch2 = Util.GetObjectExactMatch(playerObj, "Facial expression");
			Vector3 position = objectExactMatch2.transform.position;
			Vector3 position2 = Camera.main.WorldToViewportPoint(position);
			position2.x += vector.x;
			position2.y += vector.y;
			Vector3 position3 = uiCamera.ViewportToWorldPoint(position2);
			base.transform.position = position3;
			Vector3 localPosition = new Vector3(base.transform.localPosition.x, base.transform.localPosition.y, 0f);
			base.transform.localPosition = localPosition;
			objectExactMatch.transform.localScale = localScale;
		}
		if (UnityEngine.Time.time - startTime > lifetime && lifetime > 0f)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void OnDestroy()
	{
		EmoteManager.Instance.OnEmoteDestroyed(base.gameObject);
	}
}
