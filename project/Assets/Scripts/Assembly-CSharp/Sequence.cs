using System.Collections.Generic;
using EB;
using EB.Sequence;
using UnityEngine;

public class Sequence : EB.Sequence.Component
{
	private class ActiveSeq
	{
		public string _name;

		public Sequence _seq;

		public ActiveSeq(string s, Sequence seq)
		{
			_name = s;
			_seq = seq;
		}
	}

	private static List<ActiveSeq> _sActive = new List<ActiveSeq>();

	protected virtual void Start()
	{
		ActiveSeq activeSeq = _sActive.Find((ActiveSeq activeSeq2) => activeSeq2._name == base.name);
		if (activeSeq != null)
		{
			Object.Destroy(this);
			return;
		}
		_sActive.Add(new ActiveSeq(base.name, this));
		StartSequence();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		int num = _sActive.FindIndex((ActiveSeq activeSeq) => activeSeq._seq == this);
		if (num >= 0)
		{
			_sActive.RemoveAt(num);
		}
	}

	private void DumpActive()
	{
		foreach (ActiveSeq item in _sActive)
		{
			EB.Debug.Log("GOT : " + item._name);
		}
	}

	public static void KillAll()
	{
		foreach (ActiveSeq item in _sActive)
		{
			Object.Destroy(item._seq.gameObject);
		}
		_sActive.Clear();
	}
}
