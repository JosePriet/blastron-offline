using System;
using EB;
using UnityEngine;

public class MyRobotStatsUI : MonoBehaviour
{
	private MyRobotUI parentScreen;

	private void Start()
	{
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "CloseButton");
		if (objectExactMatch != null)
		{
			ButtonScript buttonScript = Util.FindComponent<ButtonScript>(objectExactMatch);
			buttonScript.onButtonClick += OnClosePanel;
		}
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "BackButton");
		if (objectExactMatch2 != null)
		{
			ButtonScript buttonScript2 = Util.FindComponent<ButtonScript>(objectExactMatch2);
			buttonScript2.onButtonClick += OnBack;
		}
		UpdateStats();
	}

	public void OnOpenPopup()
	{
		EBUI_TransitionManager.Instance.RunIntroTransitions(base.gameObject, null);
	}

	public void SetParentScreen(MyRobotUI parentScreen)
	{
		this.parentScreen = parentScreen;
	}

	private void OnClosePanel(GameObject caller)
	{
		if (parentScreen != null)
		{
			parentScreen.OnSubscreenClose();
			parentScreen = null;
		}
		EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, OnExit);
	}

	private void OnBack(GameObject caller)
	{
		parentScreen = null;
		EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, OnExit);
	}

	private void OnExit()
	{
		ScreenManager.Instance.ClosePopup();
	}

	private void UpdateStats()
	{
		foreach (int value in Enum.GetValues(typeof(StatManager.Stat)))
		{
			int num = Convert.ToInt32((StatManager.Stat)value);
			float stat2 = StatManager.Instance.GetStat(num);
			string text = ((StatManager.Stat)value).ToString();
			string text2 = "Attribute" + num;
			GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, text2);
			if (objectExactMatch != null)
			{
				UIUtils.SetLabelContents(objectExactMatch, "StatValue", string.Format("{0:0}", stat2));
			}
		}
	}
}
