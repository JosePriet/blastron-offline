namespace EB.Replication
{
	public struct ViewId
	{
		public uint p;

		public uint n;

		public bool s;

		public override bool Equals(object other)
		{
			if (other is ViewId)
			{
				ViewId viewId = (ViewId)other;
				return viewId.p == p && viewId.n == n;
			}
			return false;
		}

		public int CompareTo(ViewId other)
		{
			if (p == other.p)
			{
				return n.CompareTo(other.n);
			}
			return p.CompareTo(other.p);
		}

		public override int GetHashCode()
		{
			return (int)(p ^ n);
		}

		public override string ToString()
		{
			return string.Format("[ViewId] p:{0}, n:{1}", p, n);
		}

		public static bool operator ==(ViewId a, ViewId b)
		{
			return a.p == b.p && a.n == b.n;
		}

		public static bool operator !=(ViewId a, ViewId b)
		{
			return a.p != b.p || a.n != b.n;
		}
	}
}
