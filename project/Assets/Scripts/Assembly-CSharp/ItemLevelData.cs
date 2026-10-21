using System;
using EB;
using UnityEngine;

[EB.Serializable(Tag = "ild")]
public class ItemLevelData : MonoBehaviour
{
	[System.Serializable]
	[EB.Serializable(Tag = "ilv")]
	public class Level
	{
		[EB.Serializable(Tag = "u")]
		public int uses;

		[EB.Serializable(Tag = "m")]
		public float pointsMultiplier;

		public Level()
		{
		}

		public Level(int uses, float pointsMultiplier)
		{
			this.uses = uses;
			this.pointsMultiplier = pointsMultiplier;
		}
	}

	[EB.Serializable(Tag = "lvs")]
	public Level[] _levels;
}
