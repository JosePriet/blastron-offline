using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
	private static TutorialManager _this;

	private Dictionary<string, List<UITutorialArrow>> activeIds = new Dictionary<string, List<UITutorialArrow>>();

	private List<string> arrowsRequested = new List<string>();

	public static TutorialManager Instance
	{
		get
		{
			return _this;
		}
	}

	private void Awake()
	{
		_this = this;
	}

	public bool IsArrowEnabled(string id)
	{
		return arrowsRequested.Contains(id);
	}

	public void EnableArrow(string id, bool bState)
	{
		if (bState)
		{
			if (!arrowsRequested.Contains(id))
			{
				arrowsRequested.Add(id);
			}
		}
		else
		{
			arrowsRequested.Remove(id);
		}
		ProcessArrow(id);
	}

	private List<UITutorialArrow> GetList(string id)
	{
		List<UITutorialArrow> value = null;
		if (!activeIds.TryGetValue(id, out value))
		{
			value = new List<UITutorialArrow>();
			activeIds[id] = value;
		}
		return value;
	}

	public void RegisterArrow(string id, UITutorialArrow arrow)
	{
		if (!string.IsNullOrEmpty(id))
		{
			List<UITutorialArrow> list = GetList(id);
			if (!list.Contains(arrow))
			{
				list.Add(arrow);
			}
			ProcessArrow(id);
		}
	}

	public void UnRegisterArrow(string id, UITutorialArrow arrow)
	{
		List<UITutorialArrow> list = GetList(id);
		list.Remove(arrow);
		if (list.Count == 0)
		{
			activeIds.Remove(id);
		}
	}

	public void ProcessArrow(string id)
	{
		List<UITutorialArrow> list = GetList(id);
		bool flag = arrowsRequested.Contains(id);
		foreach (UITutorialArrow item in list)
		{
			if (item != null)
			{
				if (flag)
				{
					item.ShowArrow();
				}
				else
				{
					item.HideArrow();
				}
			}
		}
	}
}
