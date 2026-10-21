using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using UnityEngine;

[AddComponentMenu("FarseerUnity/Dynamics/Joints/Angle Joint Component")]
public class FSAngleJointComponent : FSJointComponent
{
	protected AngleJoint joint;

	public float BiasFactor = 0.2f;

	public float Softness;

	public override void InitJoint()
	{
		joint = JointFactory.CreateAngleJoint(FSWorldComponent.PhysicsWorld, BodyA.PhysicsBody, BodyB.PhysicsBody);
		joint.BiasFactor = BiasFactor;
		joint.Softness = Softness;
		joint.CollideConnected = CollideConnected;
		base.InitJoint();
	}

	public override void OnDrawGizmos()
	{
		if (!(BodyA == null) && !(BodyB == null))
		{
			Gizmos.color = Color.red;
			Gizmos.DrawLine(BodyA.transform.position, BodyB.transform.position);
			base.OnDrawGizmos();
		}
	}
}
