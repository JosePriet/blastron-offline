using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using UnityEngine;

[AddComponentMenu("FarseerUnity/Dynamics/Joints/Pulley Joint Component")]
public class FSPulleyJointComponent : FSJointComponent
{
	protected PulleyJoint joint;

	public Transform GroundAnchorA;

	public Transform GroundAnchorB;

	public Vector2 LocalAnchorA = Vector2.zero;

	public Vector2 LocalAnchorB = Vector2.zero;

	public float ForceRatio = 1f;

	public override void InitJoint()
	{
		base.InitJoint();
		Vector3 input = BodyA.transform.TransformPoint(new Vector3(LocalAnchorA.x, LocalAnchorA.y, -5f));
		Vector3 input2 = BodyB.transform.TransformPoint(new Vector3(LocalAnchorB.x, LocalAnchorB.y, -5f));
		joint = JointFactory.CreatePulleyJoint(FSWorldComponent.PhysicsWorld, BodyA.PhysicsBody, BodyB.PhysicsBody, FSHelper.Vector3ToFVector2(GroundAnchorA.position), FSHelper.Vector3ToFVector2(GroundAnchorB.position), BodyA.PhysicsBody.GetLocalPoint(FSHelper.Vector3ToFVector2(input)), BodyB.PhysicsBody.GetLocalPoint(FSHelper.Vector3ToFVector2(input2)), ForceRatio);
		joint.CollideConnected = CollideConnected;
	}

	public override void OnDrawGizmos()
	{
		if (BodyA != null && BodyB != null && GroundAnchorA != null && GroundAnchorB != null)
		{
			Vector3 vector = BodyA.transform.TransformPoint(new Vector3(LocalAnchorA.x, LocalAnchorA.y, -5f));
			Vector3 to = BodyB.transform.TransformPoint(new Vector3(LocalAnchorB.x, LocalAnchorB.y, -5f));
			Gizmos.color = Color.red;
			Gizmos.DrawLine(vector, GroundAnchorA.position);
			Gizmos.DrawLine(GroundAnchorA.position, GroundAnchorB.position);
			Gizmos.DrawLine(GroundAnchorB.position, to);
		}
		if (GroundAnchorA != null)
		{
			Gizmos.DrawIcon(GroundAnchorA.position, "Shape");
		}
		if (GroundAnchorB != null)
		{
			Gizmos.DrawIcon(GroundAnchorB.position, "Shape");
		}
		base.OnDrawGizmos();
	}
}
