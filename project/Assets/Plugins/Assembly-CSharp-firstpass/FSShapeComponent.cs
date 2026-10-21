using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using UnityEngine;

[AddComponentMenu("FarseerUnity/Collision/Basic Shape Component")]
public class FSShapeComponent : MonoBehaviour
{
	public ShapeType SType = ShapeType.Polygon;

	public bool UseUnityCollider = true;

	public float Density = 1f;

	public float Restitution = 0.5f;

	public float Friction = 0.75f;

	public UnityEngine.Transform[] PolygonPoints;

	[HideInInspector]
	public CollisionGroupDef CollisionFilter;

	[HideInInspector]
	public FSCollisionGroup CollisionGroup;

	[HideInInspector]
	public Category BelongsTo = Category.Cat1;

	[HideInInspector]
	public bool BelongsToFold;

	[HideInInspector]
	public Category CollidesWith = Category.All;

	[HideInInspector]
	public bool CollidesWithFold;

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		if (base.transform.childCount > 0)
		{
			for (int i = 0; i < base.transform.childCount; i++)
			{
				Gizmos.DrawWireSphere(base.transform.GetChild(i).position, 0.15f);
			}
		}
		if (SType == ShapeType.Circle)
		{
			SphereCollider component = GetComponent<SphereCollider>();
			if (component != null)
			{
				Gizmos.color = Color.yellow;
				Gizmos.DrawWireSphere(base.transform.position, component.radius * base.transform.lossyScale.x);
			}
		}
		if (PolygonPoints != null && PolygonPoints.Length >= 3)
		{
			Color yellow = Color.yellow;
			for (int j = 1; j < PolygonPoints.Length; j++)
			{
				Gizmos.color = yellow;
				Gizmos.DrawLine(PolygonPoints[j - 1].position, PolygonPoints[j].position);
				yellow.r += 0.05f;
				yellow.g += 0.05f;
				yellow.b += 0.05f;
			}
			Gizmos.DrawLine(PolygonPoints[PolygonPoints.Length - 1].position, PolygonPoints[0].position);
		}
	}

	public Shape GetShape()
	{
		if (SType == ShapeType.Polygon)
		{
			return GetPolyShape();
		}
		if (SType == ShapeType.Circle)
		{
			SphereCollider component = GetComponent<SphereCollider>();
			if (component == null)
			{
				return null;
			}
			return new CircleShape(component.radius * base.transform.lossyScale.x, Density);
		}
		return null;
	}

	protected PolygonShape GetPolyShape()
	{
		bool flag = GetComponent<FSBodyComponent>() != null;
		List<FVector2> list = new List<FVector2>();
		if (UseUnityCollider)
		{
			BoxCollider component = GetComponent<BoxCollider>();
			if (component == null)
			{
				return null;
			}
			Vector3 lossyScale = base.transform.lossyScale;
			Vector3 vector = new Vector3((0f - component.size.x) * 0.5f, (0f - component.size.y) * 0.5f);
			Vector3 vector2 = new Vector3(component.size.x * 0.5f, (0f - component.size.y) * 0.5f);
			Vector3 vector3 = new Vector3(component.size.x * 0.5f, component.size.y * 0.5f);
			Vector3 vector4 = new Vector3((0f - component.size.x) * 0.5f, component.size.y * 0.5f);
			vector.Scale(lossyScale);
			vector2.Scale(lossyScale);
			vector3.Scale(lossyScale);
			vector4.Scale(lossyScale);
			Vector3 input = ((!flag) ? FSHelper.LocalTranslatedVec3(vector, base.transform) : vector);
			Vector3 input2 = ((!flag) ? FSHelper.LocalTranslatedVec3(vector2, base.transform) : vector2);
			Vector3 input3 = ((!flag) ? FSHelper.LocalTranslatedVec3(vector3, base.transform) : vector3);
			Vector3 input4 = ((!flag) ? FSHelper.LocalTranslatedVec3(vector4, base.transform) : vector4);
			list.Add(FSHelper.Vector3ToFVector2(input));
			list.Add(FSHelper.Vector3ToFVector2(input2));
			list.Add(FSHelper.Vector3ToFVector2(input3));
			list.Add(FSHelper.Vector3ToFVector2(input4));
		}
		else
		{
			if (PolygonPoints.Length < 3)
			{
				return null;
			}
			for (int i = 0; i < PolygonPoints.Length; i++)
			{
				if (!flag)
				{
					Vector3 input5 = base.transform.parent.InverseTransformPoint(PolygonPoints[i].position);
					list.Add(FSHelper.Vector3ToFVector2(FSHelper.LocalTranslatedVec3(input5, base.transform.parent)));
				}
				else
				{
					list.Add(FSHelper.Vector3ToFVector2(PolygonPoints[i].localPosition));
				}
			}
		}
		return new PolygonShape(new Vertices(list.ToArray()), Density);
	}
}
