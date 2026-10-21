using System;
using EB;
using UnityEngine;

public class HoFData : MonoBehaviour
{
	[System.Serializable]
	public class HoFBracket
	{
		public string _name;

		public int _max;

		public float _scoreMultiplier = 0.0002f;

		public int _differentialBonus = 3;

		public int[] _bracketBonus = new int[4] { 25, 20, 16, 10 };

		public float _rivetMultiplier = 1f;

		public int Calculate(int place, int score, int[] hofPoints)
		{
			int num = hofPoints.Length;
			float num2 = 0f;
			for (int i = 0; i < num; i++)
			{
				if (i != place)
				{
					num2 += (float)hofPoints[i];
				}
			}
			if (num > 1)
			{
				num2 /= (float)(num - 1);
			}
			float num3 = ((!((float)hofPoints[place] > num2)) ? _differentialBonus : (-_differentialBonus));
			return Mathf.FloorToInt((float)_bracketBonus[place] + (float)score * _scoreMultiplier + num3 + 0.5f);
		}
	}

	public HoFBracket[] _brackets = new HoFBracket[5];

	private static HoFData _instance;

	private static string _masterSequencePath = "Prefab_Sequences/sequence_hof_master";

	private int[] _testScores = new int[4] { 11619, 7748, 4804, 3947 };

	private int[] _testHoF = new int[4] { 44, 55, 33, 76 };

	public static HoFData Instance
	{
		get
		{
			if (_instance == null)
			{
				GameObject gameObject = Assets.Load("hof/hof") as GameObject;
				_instance = gameObject.GetComponent<HoFData>();
			}
			return _instance;
		}
		private set
		{
			_instance = value;
		}
	}

	public static void CreateHoFMasterSeq()
	{
		UnityEngine.Object obj = Assets.Load(_masterSequencePath);
		if (obj != null)
		{
			GameObject target = UnityEngine.Object.Instantiate(obj) as GameObject;
			UnityEngine.Object.DontDestroyOnLoad(target);
		}
	}

	public int Level(int hofPoints)
	{
		for (int i = 0; i < _brackets.Length; i++)
		{
			if (hofPoints <= _brackets[i]._max)
			{
				return i;
			}
		}
		return _brackets.Length - 1;
	}

	public int Points(int bracket, int place, int score, int[] hofPoints)
	{
		return _brackets[bracket].Calculate(place, score, hofPoints);
	}

	public int ToAlmostNextLevel(int level, int points)
	{
		if (level < _brackets.Length - 1)
		{
			return _brackets[level]._max - points;
		}
		return 0;
	}

	public float RivetMultiplier(int bracket)
	{
		if (bracket < _brackets.Length)
		{
			return _brackets[bracket]._rivetMultiplier;
		}
		return 1f;
	}

	public void TestBracket()
	{
		int num = 1;
		for (int i = 0; i < 4; i++)
		{
			UnityEngine.Debug.Log("ACE: " + _brackets[num].Calculate(i, _testScores[i], _testHoF));
		}
	}
}
