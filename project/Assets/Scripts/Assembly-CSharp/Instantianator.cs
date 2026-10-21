using System.Collections.Generic;
using UnityEngine;

public class Instantianator : MonoBehaviour
{
	private class Entry
	{
		public GameObject _template;

		public Vector3 _p;

		public Quaternion _q;

		public float _elapsed;

		public void Create()
		{
			Object.Instantiate(_template, _p, _q);
		}

		public void Update(float interval)
		{
			_elapsed += interval;
		}
	}

	private const float r2_0 = 0f;

	private const float r2_1 = 100f;

	private const float t_0 = 0f;

	private const float t_1 = 1f;

	private List<Entry> _queue = new List<Entry>();

	private List<Entry> _recent = new List<Entry>();

	public static Instantianator Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
	}

	public void Enqueue(GameObject template)
	{
		Enqueue(template, Vector3.zero, Quaternion.identity);
	}

	public void Enqueue(GameObject template, Vector3 p, Quaternion q)
	{
		if (!Misc.IsSlow() || (!RecentSimilar(template, p, _queue) && !RecentSimilar(template, p, _recent)))
		{
			_queue.Add(new Entry
			{
				_template = template,
				_p = p,
				_q = q
			});
		}
	}

	private bool RecentSimilar(GameObject template, Vector3 p, List<Entry> list)
	{
		foreach (Entry item in list)
		{
			if (item._template == template)
			{
				float num = 100f * (1f - item._elapsed / 1f);
				if (Vector3.SqrMagnitude(p - item._p) < num)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void Update()
	{
		if (Misc.IsSlow())
		{
			for (int i = 0; i < _recent.Count; i++)
			{
				_recent[i].Update(Time.deltaTime);
				if (_recent[i]._elapsed > 1f)
				{
					_recent.RemoveAt(i);
					i--;
				}
			}
		}
		if (_queue.Count > 0)
		{
			_queue[0].Create();
			if (Misc.IsSlow())
			{
				_recent.Add(_queue[0]);
			}
			_queue.RemoveAt(0);
		}
	}
}
