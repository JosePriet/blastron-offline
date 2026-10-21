using System.Collections;
using System.Collections.Generic;
using EB;
using UnityEngine;

public class DebugCampaignUI : MonoBehaviour
{
	private const float hiResButtonSize = 70f;

	private const float loResButtonSize = 35f;

	private const float deviceOffsetRetina = 40f;

	private const float deviceOffset = 30f;

	private float _buttonSize;

	private Vector2 scrollPos;

	private List<SinglePlayerPrefab> _missions = new List<SinglePlayerPrefab>();

	private string[] _names;

	private int _selected;

	private IEnumerator Start()
	{
		yield return Assets.LoadBundle("singleplayer");
		Object[] prefabs = Assets.LoadAll("SinglePlayer_Prefabs", typeof(GameObject));
		Object[] array = prefabs;
		for (int i = 0; i < array.Length; i++)
		{
			GameObject go = (GameObject)array[i];
			SinglePlayerPrefab sp = go.GetComponent<SinglePlayerPrefab>();
			if (sp != null)
			{
				_missions.Add(sp);
			}
		}
		_missions.Sort((SinglePlayerPrefab p1, SinglePlayerPrefab p2) => string.Compare(p1.name, p2.name));
		_names = new string[_missions.Count];
		for (int i2 = 0; i2 < _missions.Count; i2++)
		{
			_names[i2] = _missions[i2].name;
		}
	}
}
