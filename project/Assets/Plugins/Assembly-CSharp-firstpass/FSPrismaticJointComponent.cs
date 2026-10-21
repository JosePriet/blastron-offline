using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using UnityEngine;

[AddComponentMenu("FarseerUnity/Dynamics/Joints/Prismatic Joint Component")]
public class FSPrismaticJointComponent : FSJointComponent
{
	protected PrismaticJoint joint;

	public float Frequency = 60f;

	public override void InitJoint()
	{
		base.InitJoint();
		float f = Mathf.Atan2(BodyB.PhysicsBody.Position.Y - BodyA.PhysicsBody.Position.Y, BodyB.PhysicsBody.Position.X - BodyA.PhysicsBody.Position.X);
		joint = JointFactory.CreatePrismaticJoint(axis: new FVector2(Mathf.Cos(f), Mathf.Sin(f)), world: FSWorldComponent.PhysicsWorld, bodyA: BodyA.PhysicsBody, bodyB: BodyB.PhysicsBody, localanchorB: FVector2.Zero);
		joint.CollideConnected = CollideConnected;
	}

	public override void Update()
	{
		base.Update();
	}

	public override void OnDrawGizmos()
	{
		if (!(BodyA == null) && !(BodyB == null))
		{
			Gizmos.color = Color.cyan;
			Gizmos.DrawLine(BodyA.transform.position, BodyB.transform.position);
		}
	}
}
