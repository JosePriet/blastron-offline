using FarseerPhysics.Collision;
using FarseerPhysics.Controllers;
using Microsoft.Xna.Framework;
using UnityEngine;

public class FSBuoyancyComponent : MonoBehaviour
{
	public Vector2 StartPoint;

	public Vector2 EndPoint;

	protected AABB aabb;

	public float Density = 2f;

	public float LinearDragCoef = 0.987f;

	public float RotationalDragCoef = 0.987f;

	protected BuoyancyController buoyancyController;

	private void Start()
	{
		Vector3 position = base.transform.position;
		FVector2 min = new FVector2(position.x + StartPoint.x, position.y + StartPoint.y);
		FVector2 max = new FVector2(position.x + EndPoint.x, position.y + EndPoint.y);
		aabb = new AABB(min, max);
		buoyancyController = new BuoyancyController(aabb, Density, LinearDragCoef, RotationalDragCoef, FSWorldComponent.PhysicsWorld.Gravity);
		FSWorldComponent.PhysicsWorld.AddController(buoyancyController);
	}

	private void Update()
	{
	}

	private void OnDrawGizmos()
	{
		Vector3 position = base.transform.position;
		Vector3 vector = position;
		Vector3 vector2 = position;
		Vector3 vector3 = position;
		Vector3 vector4 = position;
		Gizmos.color = Color.red;
		vector.x += StartPoint.x;
		vector.y += StartPoint.y;
		vector2.x += StartPoint.x;
		vector2.y += EndPoint.y;
		vector3.x += EndPoint.x;
		vector3.y += EndPoint.y;
		vector4.x += EndPoint.x;
		vector4.y += StartPoint.y;
		Gizmos.DrawLine(vector, vector2);
		Gizmos.DrawLine(vector2, vector3);
		Gizmos.DrawLine(vector3, vector4);
		Gizmos.DrawLine(vector4, vector);
	}
}
