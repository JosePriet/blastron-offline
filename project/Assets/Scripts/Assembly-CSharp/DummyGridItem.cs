using EB;
using UnityEngine;

public class DummyGridItem : GridItem
{
	public Action<GameObject, int> LoadCb;

	public Action<GameObject, int> UnloadCb;

	public int index;

	private bool _first = true;

	private bool _visible;

	private GameObject tex;

	protected new virtual void OnAwake()
	{
		tex = Util.GetObject(base.gameObject, "tex");
	}

	public override void Load()
	{
		if (_first)
		{
			_first = false;
		}
		else if (!_visible)
		{
			_visible = true;
			if ((bool)tex)
			{
				tex.active = false;
			}
			if (LoadCb != null)
			{
				LoadCb(base.gameObject, index);
			}
		}
	}

	public override void Unload()
	{
		if (_visible)
		{
			_visible = false;
			if ((bool)tex)
			{
				tex.active = true;
			}
			if (UnloadCb != null)
			{
				UnloadCb(base.gameObject, index);
			}
		}
	}
}
