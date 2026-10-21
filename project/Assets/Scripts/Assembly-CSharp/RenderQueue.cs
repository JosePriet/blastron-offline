using UnityEngine;

[ExecuteInEditMode]
public class RenderQueue : MonoBehaviour
{
	public enum Queue
	{
		Background = 1000,
		Geometry = 2000,
		AlphaTest = 2450,
		Transparent = 3000,
		Overlay = 4000
	}

	public Queue BaseQueue = Queue.Geometry;

	public int Offset;

	private void Awake()
	{
		UpdateQueue();
	}

	private void UpdateQueue()
	{
		if ((bool)base.renderer)
		{
			Material sharedMaterial = base.renderer.sharedMaterial;
			if ((bool)sharedMaterial)
			{
				sharedMaterial.renderQueue = (int)(BaseQueue + Offset);
			}
		}
	}
}
