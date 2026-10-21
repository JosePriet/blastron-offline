using System;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using UnityEngine;

[AddComponentMenu("FarseerUnity/Debug/Mouse Test Component")]
public class FSMouseTest : MonoBehaviour
{
	protected FixedMouseJoint mouseJoint;

	public static float MouseXWorldPhys;

	public static float MouseYWorldPhys;

	protected FVector2 mousePVec = default(FVector2);

	public virtual void Update()
	{
		UpdateMouseWorld();
		MouseDrag();
	}

	public virtual void UpdateMouseWorld()
	{
		Vector3 vector = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		MouseXWorldPhys = vector.x;
		MouseYWorldPhys = vector.y;
	}

	public virtual Body GetBodyAtMouse()
	{
		return GetBodyAtMouse(false);
	}

	public virtual Body GetBodyAtMouse(bool includeStatic)
	{
		mousePVec.X = MouseXWorldPhys;
		mousePVec.Y = MouseYWorldPhys;
		FVector2 min = new FVector2(MouseXWorldPhys - 0.001f, MouseYWorldPhys - 0.001f);
		FVector2 max = new FVector2(MouseXWorldPhys + 0.001f, MouseYWorldPhys + 0.001f);
		AABB aabb = new AABB(min, max);
		Body body = null;
		Func<Fixture, bool> callback = (Fixture fixture0) =>
		{
			Shape shape = fixture0.Shape;
			if (fixture0.Body.BodyType != BodyType.Static || includeStatic)
			{
				FarseerPhysics.Common.Transform transform;
				fixture0.Body.GetTransform(out transform);
				if (shape.TestPoint(ref transform, ref mousePVec))
				{
					body = fixture0.Body;
					return false;
				}
			}
			return true;
		};
		FSWorldComponent.PhysicsWorld.QueryAABB(callback, ref aabb);
		return body;
	}

	public virtual void MouseDrag()
	{
		if (Input.GetMouseButtonDown(0) && mouseJoint == null)
		{
			Body bodyAtMouse = GetBodyAtMouse();
			if (bodyAtMouse != null)
			{
				mouseJoint = JointFactory.CreateFixedMouseJoint(target: new FVector2(MouseXWorldPhys, MouseYWorldPhys), world: FSWorldComponent.PhysicsWorld, body: bodyAtMouse);
				mouseJoint.CollideConnected = true;
				mouseJoint.MaxForce = 300f * bodyAtMouse.Mass;
				bodyAtMouse.Awake = true;
			}
		}
		if (Input.GetMouseButtonUp(0) && mouseJoint != null)
		{
			FSWorldComponent.PhysicsWorld.RemoveJoint(mouseJoint);
			mouseJoint = null;
		}
		if (mouseJoint != null)
		{
			FVector2 worldAnchorB = new FVector2(MouseXWorldPhys, MouseYWorldPhys);
			mouseJoint.WorldAnchorB = worldAnchorB;
		}
	}
}
