using System;
using EB;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using UnityEngine;

[AddComponentMenu("FarseerUnity/Dynamics/Body Component")]
public class FSBodyComponent : MonoBehaviour
{
	protected Body body;

	public BodyType Type = BodyType.Dynamic;

	public bool Replicated = true;

	public float KillBelowY = -100f;

	protected bool initialized;

	public Body PhysicsBody
	{
		get
		{
			if (!initialized)
			{
				Start();
			}
			return body;
		}
	}

	private void Awake()
	{
		if (Replicated && (Type != BodyType.Static || base.gameObject.tag == "FSStaticDestructible"))
		{
			ReplicationView replicationView = base.gameObject.GetComponent<ReplicationView>();
			if (replicationView == null)
			{
				replicationView = base.gameObject.AddComponent<ReplicationView>();
			}
			replicationView.observed = this;
			replicationView.stateSynchronization = NetworkStateSynchronization.Unreliable;
		}
	}

	public virtual void Start()
	{
		if (initialized)
		{
			return;
		}
		initialized = true;
		body = new Body(FSWorldComponent.PhysicsWorld);
		FSShapeComponent[] componentsInChildren = GetComponentsInChildren<FSShapeComponent>();
		FSShapeComponent[] array = componentsInChildren;
		foreach (FSShapeComponent fSShapeComponent in array)
		{
			if (fSShapeComponent.GetShape() != null)
			{
				Fixture fixture = body.CreateFixture(fSShapeComponent.GetShape());
				fixture.Friction = fSShapeComponent.Friction;
				fixture.Restitution = fSShapeComponent.Restitution;
				if (fSShapeComponent.tag.Length > 0)
				{
					fixture.UserTag = fSShapeComponent.tag;
				}
				if (fSShapeComponent.CollisionFilter == CollisionGroupDef.Manually)
				{
					fixture.CollisionCategories = fSShapeComponent.BelongsTo;
					fixture.CollidesWith = fSShapeComponent.CollidesWith;
				}
				else if (fSShapeComponent.CollisionFilter == CollisionGroupDef.PresetFile && fSShapeComponent.CollisionGroup != null)
				{
					fixture.CollisionCategories = fSShapeComponent.CollisionGroup.BelongsTo;
					fixture.CollidesWith = fSShapeComponent.CollisionGroup.CollidesWith;
				}
			}
		}
		if (componentsInChildren.Length < 1)
		{
			FSShapeComponent component = GetComponent<FSShapeComponent>();
			if (component != null)
			{
				Fixture fixture2 = body.CreateFixture(component.GetShape());
				fixture2.Friction = component.Friction;
				fixture2.Restitution = component.Restitution;
				if (component.tag.Length > 0)
				{
					fixture2.UserTag = component.tag;
				}
				if (component.CollisionFilter == CollisionGroupDef.Manually)
				{
					fixture2.CollisionCategories = component.BelongsTo;
					fixture2.CollidesWith = component.CollidesWith;
				}
				else if (component.CollisionFilter == CollisionGroupDef.PresetFile && component.CollisionGroup != null)
				{
					fixture2.CollidesWith = component.CollisionGroup.CollidesWith;
					fixture2.CollisionCategories = component.CollisionGroup.BelongsTo;
				}
			}
		}
		if (base.tag == "FSSideBarrier")
		{
			body.CollisionCategories = Category.Cat7;
			body.CollidesWith = Category.None;
		}
		else if (base.tag == "FSStatic" || base.tag == "FSStaticDestructible")
		{
			body.CollisionCategories = Category.Cat2;
			body.CollidesWith = Category.None;
		}
		body.BodyType = Type;
		body.Position = new FVector2(base.transform.position.x, base.transform.position.y);
		body.Rotation = base.transform.rotation.eulerAngles.z * ((float)Math.PI / 180f);
		if (base.tag.Length > 0)
		{
			body.UserTag = base.tag;
		}
		body.UserFSBodyComponent = this;
		UnityEngine.Object.Destroy(base.collider);
	}

	private void Update()
	{
		if (body != null)
		{
			Vector3 position = base.transform.position;
			position.x = body.Position.X;
			position.y = body.Position.Y;
			Vector3 eulerAngles = base.transform.rotation.eulerAngles;
			eulerAngles.z = body.Rotation * 57.29578f;
			base.transform.position = position;
			base.transform.rotation = Quaternion.Euler(eulerAngles);
			if (base.transform.position.y < KillBelowY)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
	}

	protected virtual void OnDestroy()
	{
		FSWorldComponent.PhysicsWorld.RemoveBody(PhysicsBody);
	}

	private void OnSerializeView(BitStream stream)
	{
		if (body == null)
		{
			return;
		}
		bool data = body.Awake;
		stream.Serialize(ref data);
		if (stream.isReading)
		{
			body.Awake = data;
		}
		if (data)
		{
			FVector2 position = body.Position;
			float value = body.Rotation;
			FVector2 linearVelocity = body.LinearVelocity;
			stream.SerializeFixed16(ref position.X, 8);
			stream.SerializeFixed16(ref position.Y, 8);
			stream.SerializeFixed16(ref linearVelocity.X, 8);
			stream.SerializeFixed16(ref linearVelocity.Y, 8);
			if (!body.FixedRotation)
			{
				stream.SerializeFixed16(ref value, 8);
			}
			if (stream.isReading)
			{
				body.Position = position;
				body.Rotation = value;
				body.LinearVelocity = linearVelocity;
			}
		}
	}

	[RPC]
	private void ApplyHit(object hitInfo)
	{
		SendMessage("DoApplyHit", hitInfo);
	}
}
