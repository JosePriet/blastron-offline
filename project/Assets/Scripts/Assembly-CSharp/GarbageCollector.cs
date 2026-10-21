using System;
using UnityEngine;

internal class GarbageCollector : MonoBehaviour
{
	public int frameFreq = 30;

	private void Update()
	{
		if (Setup.Instance._gameStarted && Time.frameCount % frameFreq == 0)
		{
			GC.Collect();
		}
	}
}
