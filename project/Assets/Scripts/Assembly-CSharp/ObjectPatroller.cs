using System;
using EB;
using UnityEngine;

public class ObjectPatroller : MonoBehaviour
{
	[System.Serializable]
	public class Destination
	{
		public Transform point;

		public bool flipX;
	}

	public Destination[] points;

	public float speed;

	private int pointPrev;

	private int pointNext;

	private float lerpPos;

	private void Start()
	{
		if (points.Length > 1)
		{
			base.transform.position = points[0].point.transform.position;
		}
		else
		{
			EB.Debug.LogError("Missing points!");
		}
		pointPrev = 0;
		pointNext = 1;
	}

	private void Update()
	{
		lerpPos += UnityEngine.Time.deltaTime * speed;
		if (lerpPos > 1f)
		{
			lerpPos = 0f;
			pointPrev++;
			pointNext++;
			if (pointPrev >= points.Length)
			{
				pointPrev = 0;
			}
			if (pointNext >= points.Length)
			{
				pointNext = 0;
			}
			if (points[pointPrev].flipX)
			{
				Vector3 localScale = base.transform.localScale;
				localScale.x *= -1f;
				base.transform.localScale = localScale;
			}
		}
		base.transform.position = Vector3.Lerp(points[pointPrev].point.position, points[pointNext].point.position, lerpPos);
	}
}
