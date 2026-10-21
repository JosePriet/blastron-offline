using UnityEngine;

public class UVScroller : MonoBehaviour
{
	public float scrollSpeed = 0.1f;

	public bool ScrollX;

	public bool ScrollY = true;

	private Material _material;

	private Vector2 _offset;

	private void Start()
	{
		if ((bool)base.renderer)
		{
			_material = base.renderer.material;
			UpdateOffset();
		}
	}

	private void UpdateOffset()
	{
		_offset = default(Vector2);
		if (ScrollX)
		{
			_offset.x = 1f;
		}
		if (ScrollY)
		{
			_offset.y = 1f;
		}
	}

	public void Update()
	{
		if ((bool)_material)
		{
			_material.mainTextureOffset = _offset * (Time.timeSinceLevelLoad * scrollSpeed % 1f);
		}
	}
}
