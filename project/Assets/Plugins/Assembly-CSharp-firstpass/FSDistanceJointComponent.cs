using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using UnityEngine;

[AddComponentMenu("FarseerUnity/Dynamics/Joints/Distance Joint Component")]
public class FSDistanceJointComponent : FSJointComponent
{
	protected DistanceJoint joint;

	public float Frequency = 60f;

	public override void InitJoint()
	{
		base.InitJoint();
		joint = JointFactory.CreateDistanceJoint(FSWorldComponent.PhysicsWorld, BodyA.PhysicsBody, BodyB.PhysicsBody, FVector2.Zero, FVector2.Zero);
		joint.CollideConnected = CollideConnected;
		joint.Frequency = Frequency;
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
