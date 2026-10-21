using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Poly2Tri.Triangulation.Delaunay;

namespace Poly2Tri.Triangulation
{
	public abstract class TriangulationContext
	{
		public readonly List<TriangulationPoint> Points = new List<TriangulationPoint>(200);

		public readonly List<DelaunayTriangle> Triangles = new List<DelaunayTriangle>();

		public TriangulationMode TriangulationMode { get; protected set; }

		public Triangulatable Triangulatable { get; private set; }

		public bool WaitUntilNotified { get; private set; }

		public bool Terminated { get; set; }

		public int StepCount { get; private set; }

		public virtual bool IsDebugEnabled { get; protected set; }

		public TriangulationContext()
		{
			Terminated = false;
		}

		public void Done()
		{
			StepCount++;
		}

		public virtual void PrepareTriangulation(Triangulatable t)
		{
			Triangulatable = t;
			TriangulationMode = t.TriangulationMode;
			t.PrepareTriangulation(this);
		}

		public abstract TriangulationConstraint NewConstraint(TriangulationPoint a, TriangulationPoint b);

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void Update(string message)
		{
		}

		public virtual void Clear()
		{
			Points.Clear();
			Terminated = false;
			StepCount = 0;
		}
	}
}
