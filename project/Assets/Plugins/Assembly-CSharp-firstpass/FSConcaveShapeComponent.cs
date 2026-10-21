using System.Collections.Generic;
using FarseerPhysics.Dynamics;
using UnityEngine;

[AddComponentMenu("FarseerUnity/Collision/Concave Shape Component")]
public class FSConcaveShapeComponent : MonoBehaviour
{
	public float Density = 1f;

	public float Restitution = 0.5f;

	public float Friction = 0.75f;

	[HideInInspector]
	public CollisionGroupDef CollisionFilter;

	public FSCollisionGroup CollisionGroup;

	public Category BelongsTo = Category.Cat1;

	public bool BelongsToFold;

	public Category CollidesWith = Category.All;

	public bool CollidesWithFold;

	[HideInInspector]
	public Vector3[,] ConvertedVertices;

	[HideInInspector]
	public FSShapePointInput PointInput = FSShapePointInput.Transform;

	[HideInInspector]
	public Transform[] TransformPoints;

	[HideInInspector]
	public Vector2[] ConcavePoints;

	public virtual void OnDrawGizmos()
	{
		if (PointInput != FSShapePointInput.Transform)
		{
			return;
		}
		List<Transform> list = new List<Transform>(TransformPoints);
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Transform child = base.transform.GetChild(i);
			if (list.Contains(child))
			{
				Gizmos.color = Color.magenta;
				Gizmos.DrawSphere(child.position, 0.13f);
			}
			else
			{
				Gizmos.color = Color.white;
				Gizmos.DrawWireSphere(child.position, 0.1f);
			}
		}
		Transform transform = null;
		if (TransformPoints == null || TransformPoints.Length <= 2)
		{
			return;
		}
		transform = TransformPoints[TransformPoints.Length - 1];
		for (int j = 0; j < TransformPoints.Length; j++)
		{
			if (transform != null && TransformPoints[j] != null)
			{
				Gizmos.color = Color.magenta;
				Gizmos.DrawLine(transform.position, TransformPoints[j].position);
				Vector3 position = transform.position;
				position += (TransformPoints[j].position - transform.position) / 2f;
				Gizmos.color = Color.white;
				GizmosHelper.DrawString(position, j.ToString());
			}
			transform = TransformPoints[j];
		}
	}
}
