using System.Collections.Generic;
using UnityEngine;

public class TutorialHook : MonoBehaviour
{
	public class Parameters
	{
		public string tutorialId;

		public bool hideItem;
	}

	public string _tutorialId;

	private Dictionary<UIWidget, float> _savedWidgetAlphas = new Dictionary<UIWidget, float>();

	private void Awake()
	{
		UIWidget[] componentsInChildren = GetComponentsInChildren<UIWidget>();
		foreach (UIWidget uIWidget in componentsInChildren)
		{
			_savedWidgetAlphas.Add(uIWidget, uIWidget.alpha);
		}
	}

	private void EnableButton(Parameters param)
	{
		if (!(param.tutorialId == _tutorialId))
		{
			return;
		}
		if (base.gameObject.collider != null)
		{
			base.gameObject.collider.enabled = true;
		}
		foreach (UIWidget key in _savedWidgetAlphas.Keys)
		{
			key.alpha = _savedWidgetAlphas[key];
		}
	}

	private void DisableButton(Parameters param)
	{
		if (!(param.tutorialId == _tutorialId))
		{
			return;
		}
		if (base.gameObject.collider != null)
		{
			base.gameObject.collider.enabled = false;
		}
		if (!param.hideItem)
		{
			return;
		}
		foreach (UIWidget key in _savedWidgetAlphas.Keys)
		{
			key.alpha = 0f;
		}
	}
}
