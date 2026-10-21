using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using UnityEngine;

[AddComponentMenu("FarseerUnity/Physics World Component")]
public class FSWorldComponent : MonoBehaviour
{
	protected static FSWorldComponent instance;

	private World world;

	public Vector2 Gravity = new Vector2(0f, -9.8f);

	public bool Paused { get; set; }

	public static bool IsValid
	{
		get
		{
			return instance != null;
		}
	}

	public static World PhysicsWorld
	{
		get
		{
			return instance.world;
		}
	}

	private void Awake()
	{
		instance = this;
		world = new World(new FVector2(Gravity.x, Gravity.y));
	}

	private void FixedUpdate()
	{
		if (!Paused)
		{
			world.Step(Time.fixedDeltaTime);
		}
	}

	public static void Destroy()
	{
		Object.Destroy(instance);
	}
}
