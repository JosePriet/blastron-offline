using UnityEngine;

public class ScrollUV : MonoBehaviour
{
	private UISprite spr;

	public float offsetSpeed = 0.1f;

	private Material _mat;

	private void Awake()
	{
		spr = GetComponent<UISprite>();
		_mat = spr.material;
	}

	private void OnDestroy()
	{
		if ((bool)_mat)
		{
			_mat.mainTextureOffset = Vector2.zero;
		}
	}

	private void Update()
	{
		float y = Time.time * offsetSpeed % 1f;
		_mat.mainTextureOffset = new Vector2(0f, y);
	}
}
