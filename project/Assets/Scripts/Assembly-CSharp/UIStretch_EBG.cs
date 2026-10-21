public class UIStretch_EBG : UIStretch
{
	private void Awake()
	{
		OnOpenPopup();
	}

	private void OnOpenPopup()
	{
		if (mRoot == null)
		{
			mRoot = NGUITools.FindInParents<UIRoot>(base.gameObject);
		}
		base.Update();
	}

	private void OnOpenScreen()
	{
		if (mRoot == null)
		{
			mRoot = NGUITools.FindInParents<UIRoot>(base.gameObject);
		}
		base.Update();
	}
}
