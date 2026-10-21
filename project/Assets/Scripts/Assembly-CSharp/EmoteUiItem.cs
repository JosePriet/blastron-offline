using EB;
using UnityEngine;

public class EmoteUiItem : MonoBehaviour
{
	public Color emptyColor;

	private bool isDrag;

	private bool _isEmpty;

	private string _emoteName;

	private UISprite _baseSprite;

	private UISprite _emoteSprite;

	public bool isEmpty
	{
		get
		{
			return _isEmpty;
		}
		set
		{
			_isEmpty = value;
			baseSprite.color = ((!_isEmpty) ? Color.white : emptyColor);
			emoteSprite.enabled = !_isEmpty;
		}
	}

	public string emoteName
	{
		get
		{
			return _emoteName;
		}
		set
		{
			_emoteName = value;
			emoteSprite.spriteName = value;
			emoteSprite.MakePixelPerfect();
			float num = 0.75f;
			emoteSprite.transform.localScale *= num;
		}
	}

	protected UISprite baseSprite
	{
		get
		{
			return UIUtils.GetOrAssign(ref _baseSprite, base.gameObject, "base");
		}
	}

	protected UISprite emoteSprite
	{
		get
		{
			return UIUtils.GetOrAssign(ref _emoteSprite, base.gameObject, "expression");
		}
	}

	private void OnDrag(Vector2 delta)
	{
		isDrag = true;
	}

	private void OnPress(bool down)
	{
		if (!down && !isDrag && !isEmpty)
		{
			SendMessageUpwards("OnEmoteSelected", emoteName);
		}
		isDrag = false;
	}
}
