using System.Collections.Generic;
using EB;
using FarseerPhysics.Collision;
using Microsoft.Xna.Framework;
using UnityEngine;

public static class Navigation
{
	public class Node
	{
		private bool _INSERTED;

		public Node _parent;

		public float _g;

		public float _h;

		public int _x;

		public int _y;

		public bool _jump;

		public float Cost
		{
			get
			{
				return _g + _h;
			}
		}

		public Node Next { get; private set; }

		public static float CellSize { get; set; }

		public FVector2 Position
		{
			get
			{
				return new FVector2((float)_x * CellSize + CellSize * 0.5f, (float)_y * CellSize + CellSize * 0.5f);
			}
		}

		public Node(int x, int y, int g, bool jump)
		{
			_x = x;
			_y = y;
			_g = g;
			_jump = jump;
		}

		public Node(int x, int y, int g, bool jump, bool inserted)
		{
			_x = x;
			_y = y;
			_g = g;
			_jump = jump;
			_INSERTED = inserted;
		}

		public override bool Equals(object obj)
		{
			if (obj is Node)
			{
				return (Node)obj == this;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return _x ^ _y;
		}

		public override string ToString()
		{
			return string.Format("({0},{1}) {2}+{3}={4}", _x, _y, _g, _h, Cost);
		}

		public bool Contains(FVector2 p)
		{
			FVector2 fVector = new FVector2((float)_x * CellSize - CellSize * 0.5f, (float)_y * CellSize - CellSize * 0.5f);
			FVector2 max = fVector + new FVector2(CellSize, CellSize);
			return new AABB(fVector, max).Contains(ref p);
		}

		private int LengthInternal()
		{
			int num = 1;
			Node next = Next;
			while (!object.ReferenceEquals(next, null))
			{
				num++;
				next = next.Next;
			}
			return num;
		}

		public void Append(Node appendee)
		{
			appendee.Next = Next;
			Next = appendee;
		}

		public void Append(int ahead, Node appendee)
		{
			Node node = this;
			for (int i = 0; i < ahead; i++)
			{
				node = node.Next;
			}
			node.Append(appendee);
		}

		public static int Length(Node n)
		{
			if (!object.ReferenceEquals(n, null))
			{
				return n.LengthInternal();
			}
			return 0;
		}

		public static void LimitTo(Node p, int length)
		{
			for (int i = 0; i < length; i++)
			{
				if (object.ReferenceEquals(p, null))
				{
					break;
				}
				p = p.Next;
			}
			if (!object.ReferenceEquals(p, null))
			{
				p.Next = null;
			}
		}

		public static Node Bidirection(Node path)
		{
			Node parent = path._parent;
			while (!object.ReferenceEquals(parent, null))
			{
				parent.Next = path;
				path = parent;
				parent = path._parent;
			}
			return path;
		}

		public static void DrawGizmos(Node n, bool bigger, Color col)
		{
			if (object.ReferenceEquals(n, null))
			{
				return;
			}
			Node next = n.Next;
			float num = 1f;
			float num2 = ((!bigger) ? 1f : 1.01f);
			while (!object.ReferenceEquals(next, null))
			{
				Gizmos.color = col;
				if (next._INSERTED)
				{
					Gizmos.color = Color.red;
				}
				Gizmos.DrawWireCube(new Vector3((float)next._x * CellSize + CellSize * 0.5f, (float)next._y * CellSize + CellSize * 0.5f, 10f), Vector3.one * CellSize * 0.5f * num);
				next = next.Next;
				num *= num2;
			}
		}

		public static bool operator ==(Node a, Node b)
		{
			return a._x == b._x && a._y == b._y;
		}

		public static bool operator !=(Node a, Node b)
		{
			return !(a == b);
		}
	}

	public class NavWorld
	{
		public const int cCellFree = 0;

		public const int cCellTerrainBlocked = 2;

		public const int cCellStaticBlocked = 4;

		public const int cCellWalkable = 8;

		private const int halfSpanX = 5;

		private const int halfSpanY = 4;

		private const int spanX = 11;

		private const int spanY = 9;

		private static int[,] _successorDirs = new int[6, 4]
		{
			{ -1, 0, 10, 1 },
			{ -1, 1, 14, 1 },
			{ 1, 1, 14, 1 },
			{ 1, 0, 10, 1 },
			{ 1, -1, 24, 0 },
			{ -1, -1, 24, 0 }
		};

		private List<Node> JUMPPOINTS = new List<Node>();

		private int _dx;

		private int _dy;

		private float _cdim;

		private int[] _cells;

		public int Width
		{
			get
			{
				return _dx;
			}
		}

		public int Height
		{
			get
			{
				return _dy;
			}
		}

		public int[] NavCells
		{
			get
			{
				return _cells;
			}
		}

		public NavWorld(int x, int y, float cdim)
		{
			_dx = x;
			_dy = y;
			_cdim = cdim;
			_cells = new int[_dx * _dy];
			Node.CellSize = cdim;
			InitJumpCellDistances();
		}

		public NavWorld(int[] cells, int x, int y, float dim)
		{
			_dx = x;
			_dy = y;
			_cdim = dim;
			_cells = cells;
			Node.CellSize = _cdim;
			InitJumpCellDistances();
		}

		public void Reset()
		{
			JUMPPOINTS.Clear();
		}

		public void SetCell(int x, int y, int mask)
		{
			if (y < _dy)
			{
				_cells[x + y * _dx] |= mask;
			}
		}

		public void ClearCell(int x, int y, int mask)
		{
			if (y < _dy)
			{
				_cells[x + y * _dx] &= ~mask;
			}
		}

		public void Cell(FVector2 p, out int x, out int y)
		{
			x = Mathf.FloorToInt(p.X / _cdim);
			y = Mathf.FloorToInt(p.Y / _cdim);
		}

		private List<Node> JumpPointsFrom(Node n)
		{
			int num = Mathf.Max(n._x - 5, 1);
			int num2 = Mathf.Min(num + 11, _dx - 1);
			int num3 = Mathf.Max(n._y - 1, 0);
			int num4 = Mathf.Min(num3 + 4 + 1, _dy);
			List<Node> list = new List<Node>();
			for (int i = num3; i < num4; i++)
			{
				int num5 = Mathf.Abs(num3 + 4 - i);
				for (int j = num; j < num2; j++)
				{
					int num6 = Mathf.Abs(num + 5 - j);
					if (((num6 > 3 && num6 <= JumpCellsHorizontalMax) || (num5 < 3 && num6 < 3)) && IsCell(j, i, 8))
					{
						if (IsCellClear(j - 1, i) && j > n._x)
						{
							list.Add(new Node(j, i, 0, true));
						}
						else if (IsCellClear(j + 1, i) && j < n._x)
						{
							list.Add(new Node(j, i, 0, true));
						}
					}
				}
			}
			return list;
		}

		private FVector2 CellPos(int x, int y)
		{
			return new FVector2((float)x * Node.CellSize + Node.CellSize * 0.5f, (float)y * Node.CellSize - Node.CellSize * 0.5f);
		}

		private bool IsCell(int x, int y, int mask)
		{
			return (_cells[x + y * _dx] & mask) != 0;
		}

		private bool IsCellClear(int x, int y)
		{
			return _cells[x + y * _dx] == 0;
		}

		private bool IsCellInBounds(int x, int y)
		{
			return x >= 0 && x < _dx && y >= 0 && y < _dy;
		}

		public bool IsCellClearChecked(int x, int y)
		{
			if (IsCellInBounds(x, y))
			{
				return IsCellClear(x, y);
			}
			return true;
		}

		public void Clear(FVector2 c, float r)
		{
			float num = r * r;
			FVector2 fVector = new FVector2(c.X - r, c.Y - r);
			FVector2 fVector2 = new FVector2(c.X + r, c.Y + r);
			int num2 = ToCellMin(fVector.X, _dx);
			int num3 = ToCellMin(fVector.Y, _dy);
			int num4 = ToCellMax(fVector2.X, _dx);
			int num5 = ToCellMax(fVector2.Y, _dy);
			FVector2 value = default(FVector2);
			for (int i = num3; i < num5; i++)
			{
				value.Y = (float)i * Node.CellSize - Node.CellSize * 0.5f;
				for (int j = num2; j < num4; j++)
				{
					value.X = (float)j * Node.CellSize - Node.CellSize * 0.5f;
					if (FVector2.DistanceSquared(value, c) < num)
					{
						ClearCell(j, i, 10);
						if (i > 0 && IsCell(j, i - 1, 6))
						{
							SetCell(j, i, 8);
						}
					}
				}
			}
		}

		public int ToCellMin(float f, int num)
		{
			return Mathf.Min(Mathf.Max(0, Mathf.FloorToInt(f / Node.CellSize)), num - 1);
		}

		public int ToCellMax(float f, int num)
		{
			return Mathf.Max(0, Mathf.Min(Mathf.CeilToInt(f / Node.CellSize), num));
		}

		public FVector2 ToWorld(int x, int y)
		{
			return new FVector2((float)x * Node.CellSize + Node.CellSize * 0.5f, (float)y * Node.CellSize + Node.CellSize * 0.5f);
		}

		public void ToCell(FVector2 p, out int x, out int y)
		{
			x = Mathf.FloorToInt(p.X / Node.CellSize);
			y = Mathf.FloorToInt(p.Y / Node.CellSize);
		}

		public bool FreeAccessiblePosition(FVector2 p, FVector2 min, FVector2 max, out FVector2 pos)
		{
			int min2 = ToCellMin(min.X, _dx);
			int min3 = ToCellMin(min.Y, _dy);
			int max2 = ToCellMax(max.X, _dx);
			int max3 = ToCellMax(max.Y, _dy);
			for (int i = 0; i < 5; i++)
			{
				int x = Random.Range(min2, max2);
				int num = Random.Range(min3, max3);
				for (int j = 0; j < 15; j++)
				{
					int num2 = num - j;
					if (num2 <= 0)
					{
						break;
					}
					if (IsCell(x, num2, 8))
					{
						pos = ToWorld(x, num2);
						return true;
					}
				}
			}
			pos = FVector2.Zero;
			UnityEngine.Debug.Log(string.Format("Failed to find a place to flee using p={0}, min={1}, max={2}", p, min, max));
			return false;
		}

		public List<Node> Successors(Node n)
		{
			List<Node> list = new List<Node>();
			for (int i = 0; i < _successorDirs.GetLength(0); i++)
			{
				int num = n._x + _successorDirs[i, 0];
				int num2 = n._y + _successorDirs[i, 1];
				if (num < 0 || num > _dx - 1 || num2 < 0 || num2 > _dy - 1)
				{
					continue;
				}
				int num3 = _cells[num + num2 * _dx];
				if (!IsCell(num, num2, 6))
				{
					bool flag = (num3 & 8) != 0;
					if (_successorDirs[i, 3] != 1 || flag)
					{
						int g = _successorDirs[i, 2];
						list.Add(new Node(num, num2, g, false));
					}
				}
			}
			if (n._x >= 0 && n._x < _dx && n._y >= 0 && n._y < _dy && !IsCellClear(n._x, n._y))
			{
				List<Node> collection = JumpPointsFrom(n);
				JUMPPOINTS.AddRange(collection);
				list.AddRange(collection);
			}
			return list;
		}

		public void DrawGizmos()
		{
			float cellSize = Node.CellSize;
			for (int i = 0; i < _dy; i++)
			{
				for (int j = 0; j < _dx; j++)
				{
					int num = _cells[j + i * _dx];
					if (num != 0)
					{
						if ((num & 8) != 0)
						{
							Gizmos.color = Color.cyan;
						}
						else if ((num & 2) != 0 && (num & 4) != 0)
						{
							Gizmos.color = Color.magenta;
						}
						else if ((num & 2) != 0)
						{
							Gizmos.color = Color.green;
						}
						else if ((num & 4) != 0)
						{
							Gizmos.color = Color.red;
						}
						Gizmos.DrawWireCube(new Vector3((float)j * cellSize + cellSize * 0.5f, (float)i * cellSize + cellSize * 0.5f, 10f), Vector3.one * Node.CellSize);
					}
				}
			}
			Gizmos.color = new Color(1f, 0.5f, 0f);
			foreach (Node jUMPPOINT in JUMPPOINTS)
			{
				Gizmos.DrawWireCube(new Vector3((float)jUMPPOINT._x * cellSize + cellSize * 0.5f, (float)jUMPPOINT._y * cellSize + cellSize * 0.5f, 10f), Vector3.one * Node.CellSize * 0.8f);
			}
		}

		public int HCost(int sx, int sy, int ex, int ey)
		{
			return (Mathf.Abs(ex - sx) + Mathf.Abs(ey - sy)) * 10;
		}
	}

	public enum PathingResult
	{
		kPathFound = 0,
		kPathIncomplete = 1,
		kNoPath = 2
	}

	private static List<Node> _open;

	private static List<Node> _closed;

	public static float JumpHorizontalMax { get; private set; }

	public static float JumpVerticalMax { get; private set; }

	public static int JumpCellsHorizontalMax { get; private set; }

	public static int JumpCellsVerticalMax { get; private set; }

	static Navigation()
	{
		_open = new List<Node>();
		_closed = new List<Node>();
	}

	private static int JumpHeightInCells()
	{
		return (int)Mathf.Max(0f, Tuning.Instance.Player_JumpHeight * 0.1f / Node.CellSize - 1f);
	}

	private static void Reset()
	{
		_open.Clear();
		_closed.Clear();
	}

	public static Node Search(FVector2 start, FVector2 end, NavWorld navWorld, out PathingResult ePathResult)
	{
		ePathResult = PathingResult.kNoPath;
		navWorld.Reset();
		int x;
		int y;
		navWorld.Cell(start, out x, out y);
		int x2;
		int y2;
		navWorld.Cell(end, out x2, out y2);
		if (x == x2 && y == y2)
		{
			return null;
		}
		Reset();
		Node item = new Node(x, y, 0, false);
		_open.Add(item);
		while (_open.Count > 0)
		{
			item = _open[0];
			_open.RemoveAt(0);
			_closed.Add(item);
			int num = Mathf.Abs(x2 - item._x);
			int num2 = Mathf.Abs(y2 - item._y);
			if (num < 2 && num2 < 3)
			{
				ePathResult = PathingResult.kPathFound;
				return Node.Bidirection(item);
			}
			List<Node> list = navWorld.Successors(item);
			Node succNode;
			foreach (Node item2 in list)
			{
				succNode = item2;
				if (_closed.Exists((Node n) => n == succNode))
				{
					continue;
				}
				float num3 = item._g + succNode._g;
				float h = navWorld.HCost(succNode._x, succNode._y, x2, y2);
				Node objA = _open.Find((Node n) => n == succNode);
				if (object.ReferenceEquals(objA, null))
				{
					succNode._g = num3;
					succNode._h = h;
					succNode._parent = item;
					_open.Add(succNode);
					_open.Sort((Node a, Node b) => a.Cost.CompareTo(b.Cost));
				}
				else if (num3 < succNode._g)
				{
					succNode._parent = item;
					succNode._g = num3;
					_open.Sort((Node a, Node b) => a.Cost.CompareTo(b.Cost));
				}
			}
		}
		ePathResult = PathingResult.kPathIncomplete;
		return null;
	}

	private static void DumpOpen()
	{
		UnityEngine.Debug.Log("Open: ");
		foreach (Node item in _open)
		{
			UnityEngine.Debug.Log("o: " + item);
		}
	}

	public static void InitJumpCellDistances()
	{
		float cellDim = LevelData.Instance.CellDim;
		float num = 0f - FSWorldComponent.PhysicsWorld.Gravity.Y;
		float player_RunSpeed = Tuning.Instance.Player_RunSpeed;
		float num2 = Misc.JumpSpeedRequiredToAttainHeight(Tuning.Instance.Player_JumpHeight * 0.1f, num);
		float num3 = Mathf.Sqrt(num2 * num2 + player_RunSpeed * player_RunSpeed);
		float num4 = Mathf.Atan(num2 / player_RunSpeed);
		float num5 = Mathf.Cos(num4);
		JumpHorizontalMax = num3 * num3 * Mathf.Sin(2f * num4) / num;
		float num6 = JumpHorizontalMax * 0.5f;
		JumpVerticalMax = num6 * (num2 / player_RunSpeed) - num * num6 * num6 / (2f * (num3 * num3 * num5 * num5));
		JumpCellsHorizontalMax = Mathf.CeilToInt(JumpHorizontalMax / cellDim);
		JumpCellsVerticalMax = Mathf.CeilToInt(JumpVerticalMax / cellDim);
		JumpHorizontalMax -= cellDim * 0.25f;
		EB.Debug.Log("cell jumps : " + JumpCellsHorizontalMax + " " + JumpCellsVerticalMax);
	}
}
