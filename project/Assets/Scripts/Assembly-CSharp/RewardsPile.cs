using System.Collections.Generic;
using System.Linq;
using EB;
using UnityEngine;

public class RewardsPile : MonoBehaviour
{
	public float timeToPopIn = 0.1f;

	public float[] yLocationByRank;

	private List<UISprite> pile;

	private float timeToNextPopIn;

	private bool isPoppingIn;

	public void StartPoppingIn()
	{
		isPoppingIn = true;
	}

	public void SetPosition(int pos)
	{
		if (pos < 0 || pos >= yLocationByRank.Length)
		{
			return;
		}
		Transform transform = base.gameObject.transform;
		Vector3 localPosition = transform.localPosition;
		localPosition.y = yLocationByRank[pos];
		transform.localPosition = localPosition;
		List<UISprite> list = new List<UISprite>();
		foreach (UISprite item in pile)
		{
			float num = item.transform.localPosition.y + localPosition.y;
			if (num < 0f)
			{
				list.Add(item);
			}
		}
		EB.Debug.Log("Removing " + list.Count + " elements from the pile ...");
		foreach (UISprite item2 in list)
		{
			pile.Remove(item2);
			Object.Destroy(item2.gameObject);
		}
	}

	private void Awake()
	{
		UISprite[] source = Util.FindAllComponents<UISprite>(base.gameObject);
		pile = source.ToList();
		pile.Sort(SpriteComparison);
		foreach (UISprite item in pile)
		{
			item.enabled = false;
		}
	}

	private void Update()
	{
		if (!isPoppingIn)
		{
			return;
		}
		timeToNextPopIn -= UnityEngine.Time.deltaTime;
		if (timeToNextPopIn <= 0f)
		{
			UISprite uISprite = pile[0];
			pile.RemoveAt(0);
			uISprite.enabled = true;
			timeToNextPopIn = timeToPopIn;
			if (pile.Count < 1)
			{
				isPoppingIn = false;
			}
		}
	}

	private int SpriteComparison(UISprite a, UISprite b)
	{
		float num = a.transform.localPosition.y - b.transform.localPosition.y;
		if (num < 0f)
		{
			return -1;
		}
		if (num > 0f)
		{
			return 1;
		}
		return 0;
	}
}
