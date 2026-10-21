using System.Collections.Generic;
using EB;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Common.PolygonManipulation;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using UnityEngine;

namespace Terrain
{
	public class TerrainBody
	{
		private bool _dirty;

		private Body _body;

		private List<Vertices> _polys;

		private AABB _aabb;

		public List<Vertices> Polys
		{
			get
			{
				return _polys;
			}
		}

		public TerrainBody(List<Vertices> polys, AABB aabb)
		{
			_polys = polys;
			_aabb = aabb;
			_dirty = true;
			CreateBody();
		}

		public void CreateBody()
		{
			if (!_dirty)
			{
				return;
			}
			if (_body != null)
			{
				_body.Dispose();
			}
			_body = new Body(FSWorldComponent.PhysicsWorld);
			_body.BodyType = BodyType.Static;
			List<Vertices> list = new List<Vertices>();
			foreach (Vertices poly in _polys)
			{
				if (poly.Count >= 3)
				{
					list.AddRange(CDTDecomposer.ConvexPartition(poly));
				}
			}
			foreach (Vertices item in list)
			{
				PolygonShape shape = new PolygonShape(item, 1f);
				Fixture fixture = _body.CreateFixture(shape);
				fixture.Friction = 1f;
			}
			_body.UserTag = "Terrain";
			_body.UserData = this;
			_body.CollisionCategories = Category.Cat2;
			_body.CollidesWith = Category.None;
			_dirty = false;
		}

		public void Clean()
		{
			_body.Dispose();
		}

		public void Bool(Vertices hole, AABB aabb)
		{
			if (!AABB.TestOverlap(_aabb, aabb))
			{
				return;
			}
			List<Vertices> list = new List<Vertices>();
			foreach (Vertices poly in _polys)
			{
				PolyClipError error;
				List<Vertices> collection = YuPengClipper.Difference(poly, hole, out error);
				if (error != PolyClipError.None)
				{
					EB.Debug.LogError(string.Format("YuPeng reported {0} while processing node with aabb {1}", error, Misc.AABBToString(ref _aabb)));
					foreach (FVector2 item in hole)
					{
						UnityEngine.Debug.Log("V: " + item);
					}
				}
				list.AddRange(collection);
			}
			_polys = list;
			_polys = Misc.SimplifyPolygons(_polys, 0.3f);
			_polys = Misc.MergeIdentical(_polys);
			_dirty = true;
		}

		public void DrawGizmos(Vector3 pos, Color color)
		{
			foreach (Vertices poly in _polys)
			{
				Misc.DrawVerts(pos, poly, color, Vector3.zero, true);
			}
		}
	}
}
