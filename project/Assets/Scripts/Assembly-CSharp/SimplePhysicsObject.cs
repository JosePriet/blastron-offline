using System;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using UnityEngine;

public class SimplePhysicsObject : MonoBehaviour
{
	public enum ShapeType
	{
		kBox = 0,
		kCircle = 1
	}

	public Vector2 size;

	public bool modulateByTransformScale;

	public float mass = 1f;

	public BodyType bodyType = BodyType.Dynamic;

	public float restitution;

	public float friction;

	public ShapeType _eShapeType;

	private Body _body;

	public bool _killAtFloor = true;

	public float RenderZ { get; set; }

	public Body PhysicsBody
	{
		get
		{
			return _body;
		}
	}

	private void Awake()
	{
		if (modulateByTransformScale)
		{
			size.x *= base.transform.localScale.x;
			size.y *= base.transform.localScale.y;
		}
		switch (_eShapeType)
		{
		case ShapeType.kBox:
			_body = BodyFactory.CreateRectangle(FSWorldComponent.PhysicsWorld, size.x, size.y, 1f);
			break;
		case ShapeType.kCircle:
			_body = BodyFactory.CreateCircle(FSWorldComponent.PhysicsWorld, size.x, 1f);
			break;
		}
		_body.Mass = mass;
		_body.BodyType = bodyType;
		_body.Position = FSHelper.Vector3ToFVector2(base.transform.position);
		_body.Rotation = base.transform.eulerAngles.z * ((float)Math.PI / 180f);
		_body.Friction = friction;
		_body.Restitution = restitution;
		if (_killAtFloor)
		{
			base.gameObject.AddComponent<ObjectKiller>();
		}
		_body.UserData = base.gameObject;
	}

	private void OnDestroy()
	{
		if (_body != null && FSWorldComponent.IsValid)
		{
			FSWorldComponent.PhysicsWorld.RemoveBody(_body);
			_body = null;
		}
	}

	private void LateUpdate()
	{
		if (_body != null)
		{
			Vector3 position = base.transform.position;
			position.x = _body.Position.X;
			position.y = _body.Position.Y;
			position.z = RenderZ;
			Vector3 eulerAngles = base.transform.rotation.eulerAngles;
			eulerAngles.z = _body.Rotation * 57.29578f;
			base.transform.position = position;
			base.transform.rotation = Quaternion.Euler(eulerAngles);
		}
	}

	private void SetZ(float z)
	{
		RenderZ = z;
	}
}
