using System;
using System.Collections.Generic;
using Life;
using UnityEngine;

public class TerritoryControl : MonoBehaviour
{
	private Renderer _territoryRenderer;

	private Vector3 _territoryPosition;

	private List<LifeBase> _runningLife = new List<LifeBase>();

	private Material[] _mats;

	public Vector3 Position
	{
		get
		{
			return _territoryPosition;
		}
	}

	private void Awake()
	{
		foreach (Transform item in base.transform)
		{
			if (item.name.StartsWith("ground"))
			{
				_territoryRenderer = item.renderer as MeshRenderer;
				if (_territoryRenderer != null)
				{
					_mats = _territoryRenderer.materials;
				}
				_territoryPosition = MeshFilterToPos(item.GetComponent<MeshFilter>());
			}
		}
	}

	public void ActivateBorderFlash(float duration, float oscillations, Color c)
	{
		int num = Array.FindIndex(_mats, (Material m) => m.name.StartsWith("world_border"));
		if (num != -1)
		{
			Flash item = new Flash(_territoryRenderer, duration, oscillations, c, _mats, num);
			_runningLife.Add(item);
		}
	}

	public void DeactivateBorderFlash()
	{
		_runningLife.ForEach((LifeBase l) =>
		{
			l.Restore();
		});
		_runningLife.Clear();
	}

	public void Dim(bool dim)
	{
		int num = Array.FindIndex(_mats, (Material m) => !m.name.StartsWith("world_border"));
		if (num != -1)
		{
			_mats[num].color = ((!dim) ? Color.white : Color.gray);
		}
	}

	public void Update()
	{
		for (int i = 0; i < _runningLife.Count; i++)
		{
			LifeBase lifeBase = _runningLife[i];
			if (!lifeBase.Update(Time.fixedDeltaTime))
			{
				_runningLife.RemoveAt(i);
				i--;
			}
		}
	}

	private Vector3 MeshFilterToPos(MeshFilter mf)
	{
		Mesh mesh = mf.mesh;
		if (mesh != null)
		{
			Vector3 center = mesh.bounds.center;
			center.Normalize();
			return center * 0.5f;
		}
		return Vector3.zero;
	}

	private void OnDestroy()
	{
		_runningLife.ForEach((LifeBase l) =>
		{
			l.Restore();
		});
		DeactivateBorderFlash();
	}
}
