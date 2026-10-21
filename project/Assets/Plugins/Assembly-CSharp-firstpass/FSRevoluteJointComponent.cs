using System;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using UnityEngine;

[AddComponentMenu("FarseerUnity/Dynamics/Joints/Revolute Joint Component")]
public class FSRevoluteJointComponent : FSJointComponent
{
	protected RevoluteJoint joint;

	public bool LimitEnabled;

	public float LowerLimit = -90f;

	public float UpperLimit = 90f;

	public bool MotorEnabled;

	public float MaxMotorTorque = 400f;

	public float MotorSpeed;

	public Vector2 LocalAnchorB = Vector2.zero;

	public float Breakpoint = float.MaxValue;

	public override void InitJoint()
	{
		base.InitJoint();
		Vector3 input = BodyB.transform.TransformPoint(new Vector3(LocalAnchorB.x, LocalAnchorB.y, -5f));
		joint = JointFactory.CreateRevoluteJoint(FSWorldComponent.PhysicsWorld, BodyA.PhysicsBody, BodyB.PhysicsBody, BodyB.PhysicsBody.GetLocalPoint(FSHelper.Vector3ToFVector2(input)));
		joint.CollideConnected = CollideConnected;
		joint.LowerLimit = LowerLimit * ((float)Math.PI / 180f);
		joint.UpperLimit = UpperLimit * ((float)Math.PI / 180f);
		joint.LimitEnabled = LimitEnabled;
		joint.MaxMotorTorque = MaxMotorTorque;
		joint.MotorSpeed = MotorSpeed;
		joint.MotorEnabled = MotorEnabled;
		joint.Breakpoint = Breakpoint;
	}

	public override void OnDrawGizmos()
	{
		base.OnDrawGizmos();
		if (!(BodyA == null) && !(BodyB == null))
		{
			Vector3 vector = BodyB.transform.TransformPoint(new Vector3(LocalAnchorB.x, LocalAnchorB.y, -5f));
			Gizmos.color = Color.black;
			Gizmos.DrawSphere(vector, 0.3f);
			if (LimitEnabled)
			{
				float num = LowerLimit * ((float)Math.PI / 180f);
				float num2 = UpperLimit * ((float)Math.PI / 180f);
				float num3 = BodyB.transform.rotation.eulerAngles.z * ((float)Math.PI / 180f);
				num += num3;
				num2 += num3;
				Vector3 to = vector;
				to.x += Mathf.Cos(num) * 3f;
				to.y += Mathf.Sin(num) * 3f;
				Vector3 to2 = vector;
				to2.x += Mathf.Cos(num2) * 3f;
				to2.y += Mathf.Sin(num2) * 3f;
				Vector3 to3 = vector;
				to3.x += Mathf.Cos(num3) * 5f;
				to3.y += Mathf.Sin(num3) * 5f;
				Gizmos.color = Color.green;
				Gizmos.DrawLine(vector, to);
				Gizmos.color = Color.red;
				Gizmos.DrawLine(vector, to2);
				Gizmos.color = Color.white;
				Gizmos.DrawLine(vector, to3);
			}
		}
	}
}
