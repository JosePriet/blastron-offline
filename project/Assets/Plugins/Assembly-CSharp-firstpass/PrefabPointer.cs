using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PrefabPointer : MonoBehaviour
{
	[SerializeField]
	private string m_GUIDSerialized = Guid.Empty.ToString();

	private Guid m_GUID;

	private GameObject m_Prefab;

	private static IEnumerable<UnityEngine.Object> s_PrefabPointers;

	public void Awake()
	{
		BuildPrefabPointers(false);
		BuildGUIDIfNecessary();
	}

	public static void BuildPrefabPointers(bool ForceRebuild)
	{
		if (!ForceRebuild && s_PrefabPointers != null)
		{
			return;
		}
		IEnumerable<UnityEngine.Object> first = new List<UnityEngine.Object>(Resources.FindObjectsOfTypeAll(typeof(PrefabPointer)));
		IEnumerable<UnityEngine.Object> second = new List<UnityEngine.Object>(UnityEngine.Object.FindObjectsOfType(typeof(PrefabPointer)));
		s_PrefabPointers = first.Except(second);
		foreach (UnityEngine.Object s_PrefabPointer in s_PrefabPointers)
		{
			PrefabPointer prefabPointer = s_PrefabPointer as PrefabPointer;
			prefabPointer.BuildGUIDIfNecessary();
		}
	}

	public static void BuildPrefabPointers()
	{
		BuildPrefabPointers(false);
	}

	public GameObject GetPrefab()
	{
		if (null == m_Prefab)
		{
			BuildPrefabPointers(false);
			foreach (UnityEngine.Object s_PrefabPointer in s_PrefabPointers)
			{
				PrefabPointer prefabPointer = s_PrefabPointer as PrefabPointer;
				if (Is(prefabPointer))
				{
					m_Prefab = prefabPointer.gameObject;
					break;
				}
			}
		}
		return m_Prefab;
	}

	public bool Is(PrefabPointer TestPrefabPointer)
	{
		if (null == TestPrefabPointer)
		{
			return false;
		}
		BuildGUIDIfNecessary();
		TestPrefabPointer.BuildGUIDIfNecessary();
		return m_GUID == TestPrefabPointer.m_GUID;
	}

	public void GenerateGUID()
	{
		m_GUID = Guid.NewGuid();
		m_GUIDSerialized = m_GUID.ToString();
	}

	public string GetGUIDAsString()
	{
		BuildGUIDIfNecessary();
		return m_GUID.ToString();
	}

	public bool IsGUIDEmpty()
	{
		BuildGUIDIfNecessary();
		return m_GUID == Guid.Empty;
	}

	private void BuildGUIDIfNecessary()
	{
		if (Guid.Empty == m_GUID)
		{
			m_GUID = new Guid(m_GUIDSerialized);
		}
	}
}
