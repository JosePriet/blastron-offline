using System.Collections.Generic;
using UnityEngine;

public class GizmoDisplay : MonoBehaviour
{
	private struct GizmoEntry
	{
		public Vector3 _p;

		public Color _c;
	}

	private static GizmoDisplay _instance;

	private Dictionary<string, List<GizmoEntry>> _gizmos = new Dictionary<string, List<GizmoEntry>>();

	public static GizmoDisplay Instance
	{
		get
		{
			if (_instance == null)
			{
				GameObject gameObject = new GameObject("_gizmoDisplay", typeof(GizmoDisplay));
				_instance = gameObject.GetComponent<GizmoDisplay>();
			}
			return _instance;
		}
		set
		{
			_instance = value;
		}
	}

	private void Awake()
	{
		Instance = this;
	}

	private void OnDrawGizmos()
	{
		foreach (List<GizmoEntry> value in _gizmos.Values)
		{
			foreach (GizmoEntry item in value)
			{
				Gizmos.color = item._c;
				Gizmos.DrawCube(item._p, Vector3.one);
			}
		}
	}

	public void Add(string owner, Vector3 pos, Color c)
	{
		List<GizmoEntry> value;
		if (!_gizmos.TryGetValue(owner, out value))
		{
			_gizmos.Add(owner, new List<GizmoEntry>());
		}
		_gizmos[owner].Add(new GizmoEntry
		{
			_p = pos,
			_c = c
		});
	}

	private void OnLevelWasLoaded()
	{
		_gizmos.Clear();
	}
}
