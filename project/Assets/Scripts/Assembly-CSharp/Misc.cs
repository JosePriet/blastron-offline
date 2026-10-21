using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using EB;
using EB.Replication;
using EB.Sparx;
using FarseerPhysics.Collision;
using FarseerPhysics.Common;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using UnityEngine;

public static class Misc
{
	public enum Winding
	{
		kCCW = 0,
		kCW = 1
	}

	public struct HitInfo : EB.Replication.ISerializable
	{
		public float _damage;

		public float _mag;

		public FVector2 _pos;

		public FVector2 _dir;

		public uint _attackerID;

		public short _points;

		public short _bonusPoints;

		public ushort _rivetsBoost;

		public ushort _XpBoost;

		public ushort _effectType;

		public ushort _effectAmount;

		public ushort _effectVisualType;

		public ushort _effectApplicationInterval;

		public ushort _effectDurationSecs;

		public bool _bullet;

		public int _weaponHash;

		public int _weaponLevel;

		public FVector2 Twat
		{
			get
			{
				return _dir * _mag;
			}
		}

		public void Serialize(BitStream bs)
		{
			bs.Serialize(ref _damage);
			bs.Serialize(ref _mag);
			bs.Serialize(ref _pos);
			bs.Serialize(ref _dir);
			bs.Serialize(ref _attackerID);
			bs.Serialize(ref _points);
			bs.Serialize(ref _bonusPoints);
			bs.Serialize(ref _rivetsBoost);
			bs.Serialize(ref _XpBoost);
			bs.Serialize(ref _effectType);
			bs.Serialize(ref _effectAmount);
			bs.Serialize(ref _effectVisualType);
			bs.Serialize(ref _effectApplicationInterval);
			bs.Serialize(ref _effectDurationSecs);
			bs.Serialize(ref _bullet);
			bs.Serialize(ref _weaponHash);
			bs.Serialize(ref _weaponLevel);
		}

		public override string ToString()
		{
			return string.Format("[HitInfo damage:{0} mag:{1} pos:{2} dir:{3} attk:{4} pts:{5} weapon:{6}]", _damage, _mag, _pos, _dir, _attackerID, _points, _weaponHash);
		}
	}

	public struct DamageInfo
	{
		public bool dead;

		public float damage;

		public uint attackerID;

		public bool bullet;

		public int weaponHash;

		public int weaponLevel;
	}

	public class DeferredHit
	{
		public HitInfo _hitInfo;

		public FVector2 _linearImpulse;

		public View _receiver;

		public Fixture _fixture;
	}

	[StructLayout((LayoutKind)0, Size = 1)]
	public struct WeaponInstantiationParms
	{
	}

	public struct IntermediateTimer
	{
		private DateTime _start;

		private DateTime _intermediate;

		public void Start()
		{
			_start = DateTime.Now;
			_intermediate = _start;
		}

		public double Interval()
		{
			DateTime now = DateTime.Now;
			TimeSpan timeSpan = now - _intermediate;
			_intermediate = now;
			return timeSpan.TotalSeconds;
		}

		public double Total()
		{
			return (DateTime.Now - _start).TotalSeconds;
		}
	}

	public enum VertexSense
	{
		kConvex = 0,
		kReflex = 1,
		kCollinear = 2,
		kDoubleBack = 3
	}

	public enum PointSegResult
	{
		kLeft = 0,
		kRight = 1,
		kCollinearFwd = 2,
		kCollinearBkwd = 3
	}

	public class Timer
	{
		public delegate void Callback();

		private float _period;

		private float _time;

		private Callback _fnCallback;

		public bool Active { get; set; }

		public Timer(Callback fnCallback)
		{
			_fnCallback = fnCallback;
		}

		public void Start(float period)
		{
			Active = true;
			_time = 0f;
			_period = period;
		}

		public void Reset()
		{
			_time = 0f;
		}

		public void Update()
		{
			if (!Active)
			{
				return;
			}
			_time += UnityEngine.Time.smoothDeltaTime;
			if (_time > _period)
			{
				_time -= _period;
				if (_fnCallback != null)
				{
					_fnCallback();
				}
			}
		}

		public void Stop()
		{
			Active = false;
		}
	}

	public delegate void FixtureRangeQueryCallback(Fixture f);

	public const Category CollisionCategory_Player = Category.Cat1;

	public const Category CollisionCategory_Terrain = Category.Cat2;

	public const Category CollisionCategory_PhysicsDamage = Category.Cat3;

	public const Category CollisionCategory_Projectile = Category.Cat5;

	public const Category CollisionCategory_Projectile2 = Category.Cat6;

	public const Category CollisionCategory_SideBarrier = Category.Cat7;

	public const Category CollisionCategory_Crate = Category.Cat9;

	public const Category CollisionCategory_Loot = Category.Cat10;

	public const float PlayerZ = 0f;

	public const float TerrainZ = 10f;

	public const float LootZ = -20f;

	public const float ProjectileZ = -10f;

	public const float BeamZ = -30f;

	public const float PlayerHUDZ = -60f;

	private const float dly = 20f;

	private static List<FVector2> _DEBUG_RAYHITS = new List<FVector2>();

	public static bool IgnoreDynamicBodiesInRayCast { private get; set; }

	public static bool isChampionship
	{
		get
		{
			return GameAttributes.GetChampionshipMode(GameAttributes.GetValue(GameAttributes.AttributeType.Championship)) == GameAttributes.ChampionshipType.kOn;
		}
	}

	public static bool isSP
	{
		get
		{
			return GameAttributes.GetMode(GameAttributes.GetValue(GameAttributes.AttributeType.Mode)) == GameAttributes.ModeType.SP;
		}
	}

	public static bool IsPhone()
	{
		return false;
	}

	public static bool IsTablet()
	{
		return false;
	}

	public static bool IsRetina()
	{
		return false;
	}

	public static bool IsSlow()
	{
		return false;
	}

	public static Hashtable CreatePlayerAttributes()
	{
		Hashtable hashtable = new Hashtable();
		CharacterManager.Instance.SerializeRecipe(hashtable);
		hashtable["name"] = Hub.Instance.LoginManager.LocalUser.Name;
		hashtable["level"] = Profile.Instance.Level;
		hashtable["hofp"] = Profile.Instance.HoFPoints;
		int num = 5;
		hashtable["power"] = num;
		hashtable["wifi"] = ((Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork) ? 1 : 0);
		return hashtable;
	}

	public static Hashtable CreateDefaultSearchAttributes()
	{
		Hashtable hashtable = new Hashtable();
		hashtable[GameAttributes.AttributeType.Version] = GameAttributes.GetDefault(GameAttributes.AttributeType.Version);
		return hashtable;
	}

	public static Hashtable CreateDefaultAttributes()
	{
		return GameAttributes.CreateDefault(GameAttributes.ModeType.MP);
	}

	public static Hashtable CreateAIPlayerAttributes(string name)
	{
		Hashtable hashtable = new Hashtable();
		CharacterManager.Instance.SerializeRecipe(hashtable, CharacterDB.GetRandomRecipe());
		hashtable["name"] = name;
		hashtable["level"] = Profile.Instance.Level;
		hashtable["ai"] = true;
		return hashtable;
	}

	public static void DisplayGameAttributes(Game game, float buttonSize)
	{
		bool isHost = game.LocalPlayer.IsHost;
		foreach (DictionaryEntry attribute2 in game.Attributes)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(string.Concat(attribute2.Key, ":"), GUILayout.Width(100f));
			GUI.enabled = isHost;
			GameAttributes.AttributeType attribute = GameAttributes.GetAttribute(attribute2.Key.ToString());
			string[] values = GameAttributes.GetValues(attribute);
			string value = attribute2.Value.ToString();
			if (attribute == GameAttributes.AttributeType.Mode)
			{
				GUI.enabled = false;
			}
			int num = Array.IndexOf(values, value);
			int num2 = GUILayout.SelectionGrid(num, values, 4, GUILayout.MinHeight(buttonSize * (float)(values.Length / 4 + 1)));
			if (isHost && num != num2)
			{
				game.SetAttributes(new Hashtable { 
				{
					attribute2.Key,
					values[num2]
				} });
				break;
			}
			GUI.enabled = true;
			GUILayout.EndHorizontal();
		}
	}

	public static Mesh MakeQuadMesh(float xmin, float ymin, float xmax, float ymax)
	{
		Mesh mesh = new Mesh();
		mesh.vertices = new Vector3[4]
		{
			new Vector3(xmin, ymin, 0f),
			new Vector3(xmin, ymax, 0f),
			new Vector3(xmax, ymax, 0f),
			new Vector3(xmax, ymin, 0f)
		};
		mesh.uv = new Vector2[4]
		{
			new Vector2(0f, 0f),
			new Vector2(0f, 1f),
			new Vector2(1f, 1f),
			new Vector2(1f, 0f)
		};
		mesh.triangles = new int[6] { 0, 1, 3, 1, 2, 3 };
		return mesh;
	}

	public static Mesh MakeCircleMesh(float radius, int segs)
	{
		Mesh mesh = new Mesh();
		Vector3[] array = new Vector3[segs + 1];
		Vector2[] array2 = new Vector2[segs + 1];
		int[] array3 = new int[segs * 3];
		float f = 360f / (float)segs * ((float)Math.PI / 180f);
		float num = Mathf.Sin(f);
		float num2 = Mathf.Cos(f);
		float num3 = 1f;
		float num4 = 0f;
		int i;
		for (i = 0; i < segs; i++)
		{
			array[i] = new Vector3(num3 * radius, num4 * radius, 0f);
			array2[i] = new Vector2(num3 * radius + 0.5f, num4 * radius + 0.5f);
			float num5 = num3 * num2 - num4 * num;
			num4 = num3 * num + num4 * num2;
			num3 = num5;
			int num6 = i * 3;
			array3[num6] = (i + 1) % segs;
			array3[num6 + 1] = i % segs;
			array3[num6 + 2] = segs;
		}
		array[i] = Vector3.zero;
		array2[i] = new Vector2(0.5f, 0.5f);
		mesh.vertices = array;
		mesh.triangles = array3;
		mesh.uv = array2;
		return mesh;
	}

	public static GameObject[] GetObjects(GameObject obj, string name)
	{
		List<GameObject> list = new List<GameObject>();
		if (obj == null)
		{
			return list.ToArray();
		}
		if (obj.name.Contains(name))
		{
			list.Add(obj);
		}
		foreach (UnityEngine.Transform item in obj.transform)
		{
			list.AddRange(GetObjects(item.gameObject, name));
		}
		return list.ToArray();
	}

	public static GameObject GetObject(GameObject obj, string name)
	{
		return GetObject(obj, name, false);
	}

	public static GameObject GetObjectExactMatch(GameObject obj, string name)
	{
		return GetObject(obj, name, true);
	}

	public static GameObject GetObject(GameObject obj, string name, bool bExactMatch)
	{
		if (obj == null)
		{
			return null;
		}
		if (!bExactMatch)
		{
			if (obj.name.Contains(name))
			{
				return obj;
			}
		}
		else if (obj.name == name)
		{
			return obj;
		}
		foreach (UnityEngine.Transform item in obj.transform)
		{
			GameObject gameObject = GetObject(item.gameObject, name, bExactMatch);
			if (gameObject != null)
			{
				return gameObject;
			}
		}
		return null;
	}

	public static T FindOrAdd<T>(GameObject root) where T : Component
	{
		T component = root.GetComponent<T>();
		if (!component)
		{
			return root.AddComponent<T>();
		}
		return component;
	}

	public static T FindInTreeOrAdd<T>(GameObject root) where T : Component
	{
		T componentInChildren = root.GetComponentInChildren<T>();
		if (!componentInChildren)
		{
			return root.AddComponent<T>();
		}
		return componentInChildren;
	}

	public static AABB ComputeAABB(Vertices vertices)
	{
		FVector2 fVector = vertices[0];
		FVector2 fVector2 = fVector;
		for (int i = 1; i < vertices.Count; i++)
		{
			fVector = FVector2.Min(fVector, vertices[i]);
			fVector2 = FVector2.Max(fVector2, vertices[i]);
		}
		return new AABB(fVector, fVector2);
	}

	public static void DrawVerts(UnityEngine.Transform t, Vertices v, Color color, Vector3 offset, bool alternate)
	{
		if (v == null)
		{
			return;
		}
		for (int i = 0; i < v.Count; i++)
		{
			Vector3 vector = t.TransformPoint(v[i].ToVector3()) + offset;
			Vector3 vector2 = t.TransformPoint(v[(i + 1) % v.Count].ToVector3()) + offset;
			if (alternate)
			{
				if (i % 2 == 0)
				{
					UnityEngine.Debug.DrawLine(vector, vector2, color);
				}
				else
				{
					UnityEngine.Debug.DrawLine(vector + Vector3.back * 5f, vector2 + Vector3.back * 5f, Color.black);
				}
			}
			else
			{
				UnityEngine.Debug.DrawLine(vector, vector2, color);
			}
		}
	}

	public static void DrawVerts(UnityEngine.Transform t, Vertices v, Color colorstart, Color colorend, Vector3 offset, bool alternate)
	{
		if (v == null)
		{
			return;
		}
		Color color = colorstart;
		Color color2 = colorend / v.Count;
		for (int i = 0; i < v.Count; i++)
		{
			Vector3 start = t.TransformPoint(v[i].ToVector3()) + offset;
			Vector3 end = t.TransformPoint(v[(i + 1) % v.Count].ToVector3()) + offset;
			if (alternate)
			{
				if (i % 2 == 0)
				{
					UnityEngine.Debug.DrawLine(start, end, color);
				}
				else
				{
					UnityEngine.Debug.DrawLine(start, end, Color.black);
				}
			}
			else
			{
				UnityEngine.Debug.DrawLine(start, end, color);
			}
			color += color2;
		}
	}

	public static void DrawVerts(Vector3 pos, Vertices v, Color color, Vector3 offset, bool alternate)
	{
		if (v == null)
		{
			return;
		}
		Color color2 = color * 0.5f;
		Vector3 back = Vector3.back;
		for (int i = 0; i < v.Count; i++)
		{
			Vector3 vector = v[i].ToVector3() + pos + offset;
			Vector3 vector2 = v[(i + 1) % v.Count].ToVector3() + pos + offset;
			if (alternate)
			{
				if (i % 2 == 0)
				{
					UnityEngine.Debug.DrawLine(vector + back, vector2 + back, color);
				}
				else
				{
					UnityEngine.Debug.DrawLine(vector + back, vector2 + back, color2);
				}
				back.z -= 0.1f;
			}
			else
			{
				UnityEngine.Debug.DrawLine(vector, vector2, color);
			}
		}
	}

	public static bool RayCastForClosest(Vector3 start, Vector3 dir, out Vector3 hitPoint, out Vector3 hitNormal, out Fixture hitFixture, Body ignoreBody)
	{
		FVector2 hitPoint2;
		FVector2 hitNormal2;
		bool result = RayCastForClosest(FSHelper.Vector3ToFVector2(start), FSHelper.Vector3ToFVector2(dir), out hitPoint2, out hitNormal2, out hitFixture, ignoreBody);
		hitPoint = hitPoint2.ToVector3();
		hitNormal = hitNormal2.ToVector3();
		return result;
	}

	public static bool RayCastForClosest(FVector2 start, FVector2 dir, out FVector2 hitPoint, out FVector2 hitNormal, out Fixture hitFixture, Body ignoreBody)
	{
		World physicsWorld = FSWorldComponent.PhysicsWorld;
		bool hit = false;
		FVector2 anonHitPoint = FVector2.Zero;
		FVector2 anonHitNormal = FVector2.Zero;
		Fixture anonFixture = null;
		physicsWorld.RayCast((Fixture fixture, FVector2 point, FVector2 normal, float fraction) =>
		{
			if (fixture.Body != ignoreBody && fixture.Body.UserTag != "NoRay")
			{
				hit = true;
				anonHitPoint = point;
				anonHitNormal = normal;
				anonFixture = fixture;
				return fraction;
			}
			return -1f;
		}, start, start + dir);
		hitNormal = anonHitNormal;
		hitPoint = anonHitPoint;
		hitFixture = anonFixture;
		return hit;
	}

	public static bool RayCast(Vector3 start, Vector3 dir, out Vector3 hitPoint, out Vector3 hitNormal, out Fixture hitFixture, Body ignoreBody)
	{
		FVector2 hitPoint2;
		FVector2 hitNormal2;
		bool result = RayCast(FSHelper.Vector3ToFVector2(start), FSHelper.Vector3ToFVector2(dir), out hitPoint2, out hitNormal2, out hitFixture, ignoreBody);
		hitPoint = hitPoint2.ToVector3();
		hitNormal = hitNormal2.ToVector3();
		return result;
	}

	public static bool RayCast(FVector2 start, FVector2 dir, Body ignoreBody)
	{
		FVector2 hitPoint;
		FVector2 hitNormal;
		Fixture hitFixture;
		return RayCast(start, dir, out hitPoint, out hitNormal, out hitFixture, ignoreBody);
	}

	public static bool RayCast(FVector2 start, FVector2 end, Category castAgainstCategories)
	{
		World physicsWorld = FSWorldComponent.PhysicsWorld;
		bool hit = false;
		physicsWorld.RayCast((Fixture fixture, FVector2 hitPoint, FVector2 hitNormal, float fraction) =>
		{
			if ((fixture.CollisionCategories & castAgainstCategories) != 0)
			{
				hit = true;
				return 0f;
			}
			return -1f;
		}, start, end);
		return hit;
	}

	public static bool RayCast(FVector2 start, FVector2 dir, out FVector2 hitPoint, out FVector2 hitNormal, out Fixture hitFixture, Body ignoreBody)
	{
		World physicsWorld = FSWorldComponent.PhysicsWorld;
		bool hit = false;
		FVector2 anonHitPoint = FVector2.Zero;
		FVector2 anonHitNormal = FVector2.Zero;
		Fixture anonFixture = null;
		physicsWorld.RayCast((Fixture fixture, FVector2 point, FVector2 normal, float fraction) =>
		{
			if (fixture.Body.BodyType == BodyType.Dynamic && IgnoreDynamicBodiesInRayCast)
			{
				return -1f;
			}
			if (fixture.Body != ignoreBody && fixture.Body.UserTag != "NoRay")
			{
				hit = true;
				anonHitPoint = point;
				anonHitNormal = normal;
				anonFixture = fixture;
				return 0f;
			}
			return -1f;
		}, start, start + dir);
		hitNormal = anonHitNormal;
		hitPoint = anonHitPoint;
		hitFixture = anonFixture;
		return hit;
	}

	public static bool TestPoint(FVector2 p, Fixture ignoreFixture)
	{
		Fixture fixture = FSWorldComponent.PhysicsWorld.TestPoint(p);
		return fixture != null && fixture != ignoreFixture;
	}

	public static void DrawRaycastGizmos()
	{
		Gizmos.color = Color.magenta;
		Color color = (Color.white - Color.magenta) / _DEBUG_RAYHITS.Count;
		foreach (FVector2 dEBUG_RAYHIT in _DEBUG_RAYHITS)
		{
			Gizmos.DrawSphere(dEBUG_RAYHIT.ToVector3(), 0.5f);
			Gizmos.color += color;
		}
	}

	public static List<Vector3> FromMesh(Mesh m, bool bForceWinding, Winding eWinding = Winding.kCCW)
	{
		List<Vector3> list = new List<Vector3>(m.vertexCount);
		Vector3[] vertices = m.vertices;
		for (int i = 0; i < vertices.Length; i++)
		{
			Vector3 vector = vertices[i];
			list.Add(new Vector3(vector.x, 0f, vector.z));
		}
		if (bForceWinding && ClassifyContour(list) != eWinding)
		{
			list.Reverse();
		}
		return list;
	}

	public static Winding ClassifyContour(List<Vector3> vtx)
	{
		float num = 0f;
		int index = vtx.Count - 1;
		for (int i = 0; i < vtx.Count; i++)
		{
			Vector3 vector = vtx[index];
			Vector3 vector2 = vtx[i];
			num += (vector2.x - vector.x) * (vector2.z + vector.z);
			index = i;
		}
		return (!(num < 0f)) ? Winding.kCW : Winding.kCCW;
	}

	public static FVector2 ClosestPointOnAABB(FVector2 p, AABB aabb, FVector2 aabbPos)
	{
		aabb.LowerBound += aabbPos;
		aabb.UpperBound += aabbPos;
		if (p.X < aabb.LowerBound.X)
		{
			p.X = aabb.LowerBound.X;
		}
		else if (p.X > aabb.UpperBound.X)
		{
			p.X = aabb.UpperBound.X;
		}
		if (p.Y < aabb.LowerBound.Y)
		{
			p.Y = aabb.LowerBound.Y;
		}
		else if (p.Y > aabb.UpperBound.Y)
		{
			p.Y = aabb.UpperBound.Y;
		}
		return p;
	}

	public static FVector2 ClosestPointOnAABB(FVector2 p, AABB aabb)
	{
		if (p.X < aabb.LowerBound.X)
		{
			p.X = aabb.LowerBound.X;
		}
		else if (p.X > aabb.UpperBound.X)
		{
			p.X = aabb.UpperBound.X;
		}
		if (p.Y < aabb.LowerBound.Y)
		{
			p.Y = aabb.LowerBound.Y;
		}
		else if (p.Y > aabb.UpperBound.Y)
		{
			p.Y = aabb.UpperBound.Y;
		}
		return p;
	}

	public static Body DiscoverBody(GameObject gameObject)
	{
		Body result = null;
		FSBodyComponent component = gameObject.GetComponent<FSBodyComponent>();
		if (component != null)
		{
			result = component.PhysicsBody;
		}
		else
		{
			SimplePhysicsObject component2 = gameObject.GetComponent<SimplePhysicsObject>();
			if (component2 != null)
			{
				result = component2.PhysicsBody;
			}
			else
			{
				EB.Debug.LogError("I don't know how to get the Physics Body in object " + gameObject.name);
			}
		}
		return result;
	}

	public static Quaternion DirectionToQuat(Vector3 dir)
	{
		return Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * 57.29578f);
	}

	private static void ComputeAABB(out AABB aabb, Vertices poly)
	{
		FVector2 fVector = poly[0];
		FVector2 fVector2 = fVector;
		for (int i = 1; i < poly.Count; i++)
		{
			FVector2 value = poly[i];
			fVector = FVector2.Min(fVector, value);
			fVector2 = FVector2.Max(fVector2, value);
		}
		aabb.LowerBound = fVector;
		aabb.UpperBound = fVector2;
	}

	public static string AABBToString(ref AABB aabb)
	{
		return string.Format("Lower : {0} Upper {1}", aabb.LowerBound, aabb.UpperBound);
	}

	public static List<Vertices> SimplifyPolygons(List<Vertices> inPolys, float distanceTolerance)
	{
		List<Vertices> list = new List<Vertices>();
		foreach (Vertices inPoly in inPolys)
		{
			Vertices vertices = SimplifyTools.DouglasPeuckerSimplify(inPoly, distanceTolerance);
			if (vertices.Count >= 3)
			{
				list.Add(vertices);
			}
		}
		return list;
	}

	public static List<Vertices> MergeIdentical(List<Vertices> inPolys)
	{
		List<Vertices> list = new List<Vertices>();
		foreach (Vertices inPoly in inPolys)
		{
			list.Add(SimplifyTools.MergeIdenticalPoints(inPoly));
		}
		return list;
	}

	public static VertexSense ClassifyVertex(FVector2 v0, FVector2 v1, FVector2 v2)
	{
		switch (PointSegTest(v2, v0, v1))
		{
		case PointSegResult.kRight:
			return VertexSense.kReflex;
		case PointSegResult.kLeft:
			return VertexSense.kConvex;
		case PointSegResult.kCollinearBkwd:
			return VertexSense.kDoubleBack;
		default:
			return VertexSense.kCollinear;
		}
	}

	public static PointSegResult PointSegTest(FVector2 point, FVector2 segA, FVector2 segB)
	{
		FVector2 value = new FVector2(segB.Y - segA.Y, segA.X - segB.X);
		FVector2 value2 = new FVector2(point.X - segA.X, point.Y - segA.Y);
		float num = FVector2.Dot(value, value2);
		if (num > 0f)
		{
			return PointSegResult.kRight;
		}
		if (num < 0f)
		{
			return PointSegResult.kLeft;
		}
		if (FVector2.Dot(segB - segA, point - segB) < 0f)
		{
			return PointSegResult.kCollinearBkwd;
		}
		return PointSegResult.kCollinearFwd;
	}

	public static bool EBGLineIntersect(FVector2 a0, FVector2 a1, FVector2 b0, FVector2 b1, out float ta, out float tb)
	{
		ta = (tb = -1f);
		if (a0 == b0 || a0 == b1 || a1 == b0 || a1 == b1)
		{
			return false;
		}
		float x = a0.X;
		float y = a0.Y;
		float x2 = a1.X;
		float y2 = a1.Y;
		float x3 = b0.X;
		float y3 = b0.Y;
		float x4 = b1.X;
		float y4 = b1.Y;
		if (Mathf.Max(x, x2) < Mathf.Min(x3, x4) || Mathf.Max(x3, x4) < Mathf.Min(x, x2))
		{
			return false;
		}
		if (Mathf.Max(y, y2) < Mathf.Min(y3, y4) || Mathf.Max(y3, y4) < Mathf.Min(y, y2))
		{
			return false;
		}
		ta = (x4 - x3) * (y - y3) - (y4 - y3) * (x - x3);
		tb = (x2 - x) * (y - y3) - (y2 - y) * (x - x3);
		float num = (y4 - y3) * (x2 - x) - (x4 - x3) * (y2 - y);
		if (Mathf.Abs(num) < float.Epsilon)
		{
			return false;
		}
		ta /= num;
		tb /= num;
		if (0f < ta && ta < 1f && 0f < tb && tb < 1f)
		{
			return true;
		}
		return false;
	}

	public static void DrawPanelAboveWorld(Vector3 pos, float width, string title, List<string> texts)
	{
		float num = (float)(texts.Count + 2) * 20f;
		float num2 = num * 0.5f;
		Vector3 pos2 = Camera.main.WorldToScreenPoint(pos);
		pos2.y += num2;
		DrawPanelAtScreen(pos2, width, title, texts);
	}

	public static void DrawPanelAtScreen(Vector3 pos, float width, string title, List<string> texts)
	{
		float num = width * 0.5f;
		float num2 = (float)(texts.Count + 2) * 20f;
		float num3 = num2 * 0.5f;
		GUI.BeginGroup(new Rect(pos.x - num, (float)Screen.height - (pos.y + num3), width, num2));
		float num4 = 20f;
		GUI.Box(new Rect(0f, 0f, width, num2), title);
		foreach (string text in texts)
		{
			GUI.Label(new Rect(0f, num4 += 20f, width, 20f), text);
		}
		GUI.EndGroup();
	}

	public static string RandomString(string[] strings)
	{
		return strings[UnityEngine.Random.Range(0, strings.Length)];
	}

	public static void DrawFatAABB(Body b)
	{
		if (b.FixtureList == null)
		{
			return;
		}
		IBroadPhase broadPhase = FSWorldComponent.PhysicsWorld.ContactManager.BroadPhase;
		foreach (Fixture fixture in b.FixtureList)
		{
			for (int i = 0; i < fixture.ProxyCount; i++)
			{
				FixtureProxy fixtureProxy = fixture.Proxies[i];
				AABB aabb;
				broadPhase.GetFatAABB(fixtureProxy.ProxyId, out aabb);
				Vector3 center = new Vector3(aabb.Center.X, aabb.Center.Y, 0f);
				Vector3 size = new Vector3(aabb.Extents.X, aabb.Extents.Y, 0.5f) * 2f;
				Gizmos.DrawWireCube(center, size);
			}
		}
	}

	public static AABB GetFatAABB(Fixture f)
	{
		IBroadPhase broadPhase = FSWorldComponent.PhysicsWorld.ContactManager.BroadPhase;
		AABB aabb = default(AABB);
		aabb.UpperBound = new FVector2(float.MinValue, float.MinValue);
		aabb.LowerBound = new FVector2(float.MaxValue, float.MaxValue);
		for (int i = 0; i < f.ProxyCount; i++)
		{
			FixtureProxy fixtureProxy = f.Proxies[i];
			AABB aabb2;
			broadPhase.GetFatAABB(fixtureProxy.ProxyId, out aabb2);
			aabb.Combine(ref aabb, ref aabb2);
		}
		return aabb;
	}

	public static void CameraViewLimits(Camera c, out Vector3 min, out Vector3 max)
	{
		Vector3 vector = new Vector3(c.orthographicSize * c.aspect, c.orthographicSize, 0f);
		min = c.transform.position - vector;
		max = c.transform.position + vector;
	}

	public static bool CanDamage(Fixture target, Fixture hitter)
	{
		return target.UserTag == "EV" || ((target.Body.BodyType != BodyType.Static || target.Body.UserTag == "FSStaticDestructible") && target != hitter && target.Body.UserTag != "Terrain" && target.Body.UserTag != "FSStatic" && target.UserTag != "NoHit");
	}

	public static Vector3 TopOfScreenIntersectionVP(Vector3 worldPos, Vector3 dir)
	{
		Vector3 vector = Camera.mainCamera.WorldToViewportPoint(worldPos);
		float num = 1f - vector.y;
		dir.y = Mathf.Abs(dir.y);
		float x;
		if (dir.y != 0f)
		{
			x = Mathf.Clamp01(vector.x + dir.x * num / dir.y);
		}
		else
		{
			x = ((!(dir.x > 0f)) ? 0f : 1f);
		}
		return new Vector3(x, 1f, 0f);
	}

	public static Vector3 TopOfScreenIntersectionWorld(Vector3 worldPos, Vector3 dir)
	{
		return Camera.mainCamera.ViewportToWorldPoint(TopOfScreenIntersectionVP(worldPos, dir));
	}

	public static float JumpSpeedRequiredToAttainHeight(float height, float g)
	{
		return Mathf.Sqrt(2f * height * g);
	}

	public static Vector3 CatmullRom(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, float t)
	{
		float num = t * t;
		float num2 = num * t;
		return 0.5f * (2f * v1 + (v2 - v0) * t + (2f * v0 - 5f * v1 + 4f * v2 - v3) * num + (3f * v1 - 3f * v2 + v3 - v0) * num2);
	}

	public static void CurrentlyPlayingAnims(Animation animationComponent, List<string> currentlyPlaying)
	{
		currentlyPlaying.Clear();
		if (!(animationComponent != null))
		{
			return;
		}
		foreach (AnimationState item in animationComponent)
		{
			if (animationComponent.IsPlaying(item.name))
			{
				currentlyPlaying.Add(item.name);
			}
		}
	}

	public static int CurrentlyPlayingAnimCount(Animation animationComponent)
	{
		int num = 0;
		if (animationComponent != null)
		{
			foreach (AnimationState item in animationComponent)
			{
				if (animationComponent.IsPlaying(item.name))
				{
					num++;
				}
			}
		}
		return num;
	}

	public static float H0(float t)
	{
		return 2f * t * t * t - 3f * t * t + 1f;
	}
}
