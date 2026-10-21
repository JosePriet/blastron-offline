using System;
using System.Collections.Generic;
using FarseerPhysics.Collision;
using Microsoft.Xna.Framework;

public class QuadTree<T>
{
	public int MaxBucket;

	public int MaxDepth;

	public List<Element<T>> Nodes;

	public AABB Span;

	public QuadTree<T>[] SubTrees;

	public bool IsPartitioned
	{
		get
		{
			return SubTrees != null;
		}
	}

	public QuadTree(AABB span, int maxbucket, int maxdepth)
	{
		Span = span;
		Nodes = new List<Element<T>>();
		MaxBucket = maxbucket;
		MaxDepth = maxdepth;
	}

	private int Partition(AABB span, AABB test)
	{
		if (span.Q1.Contains(ref test))
		{
			return 1;
		}
		if (span.Q2.Contains(ref test))
		{
			return 2;
		}
		if (span.Q3.Contains(ref test))
		{
			return 3;
		}
		if (span.Q4.Contains(ref test))
		{
			return 4;
		}
		return 0;
	}

	public void AddNode(Element<T> node)
	{
		if (!IsPartitioned)
		{
			if (Nodes.Count >= MaxBucket && MaxDepth > 0)
			{
				Nodes.Add(node);
				SubTrees = new QuadTree<T>[4];
				SubTrees[0] = new QuadTree<T>(Span.Q1, MaxBucket, MaxDepth - 1);
				SubTrees[1] = new QuadTree<T>(Span.Q2, MaxBucket, MaxDepth - 1);
				SubTrees[2] = new QuadTree<T>(Span.Q3, MaxBucket, MaxDepth - 1);
				SubTrees[3] = new QuadTree<T>(Span.Q4, MaxBucket, MaxDepth - 1);
				List<Element<T>> list = new List<Element<T>>();
				foreach (Element<T> node2 in Nodes)
				{
					switch (Partition(Span, node2.Span))
					{
					case 1:
						SubTrees[0].AddNode(node2);
						break;
					case 2:
						SubTrees[1].AddNode(node2);
						break;
					case 3:
						SubTrees[2].AddNode(node2);
						break;
					case 4:
						SubTrees[3].AddNode(node2);
						break;
					default:
						node2.Parent = this;
						list.Add(node2);
						break;
					}
				}
				Nodes = list;
			}
			else
			{
				node.Parent = this;
				Nodes.Add(node);
			}
		}
		else
		{
			switch (Partition(Span, node.Span))
			{
			case 1:
				SubTrees[0].AddNode(node);
				break;
			case 2:
				SubTrees[1].AddNode(node);
				break;
			case 3:
				SubTrees[2].AddNode(node);
				break;
			case 4:
				SubTrees[3].AddNode(node);
				break;
			default:
				node.Parent = this;
				Nodes.Add(node);
				break;
			}
		}
	}

	public static bool RayCastAABB(AABB aabb, FVector2 p1, FVector2 p2)
	{
		AABB b = default(AABB);
		FVector2.Min(ref p1, ref p2, out b.LowerBound);
		FVector2.Max(ref p1, ref p2, out b.UpperBound);
		if (!AABB.TestOverlap(aabb, b))
		{
			return false;
		}
		FVector2 fVector = p2 - p1;
		FVector2 value = p1;
		FVector2 value2 = new FVector2(0f - fVector.Y, fVector.X);
		if ((double)value2.Length() == 0.0)
		{
			return true;
		}
		value2.Normalize();
		float num = FVector2.Dot(value, value2);
		FVector2[] vertices = aabb.GetVertices();
		float value3 = FVector2.Dot(vertices[0], value2) - num;
		for (int i = 1; i < 4; i++)
		{
			float value4 = FVector2.Dot(vertices[i], value2) - num;
			if (Math.Sign(value4) != Math.Sign(value3))
			{
				return true;
			}
		}
		return false;
	}

	public void QueryAABB(Func<Element<T>, bool> callback, ref AABB searchR)
	{
		Stack<QuadTree<T>> stack = new Stack<QuadTree<T>>();
		stack.Push(this);
		while (stack.Count > 0)
		{
			QuadTree<T> quadTree = stack.Pop();
			if (!AABB.TestOverlap(ref searchR, ref quadTree.Span))
			{
				continue;
			}
			foreach (Element<T> node in quadTree.Nodes)
			{
				if (AABB.TestOverlap(ref searchR, ref node.Span) && !callback(node))
				{
					return;
				}
			}
			if (quadTree.IsPartitioned)
			{
				QuadTree<T>[] subTrees = quadTree.SubTrees;
				foreach (QuadTree<T> t in subTrees)
				{
					stack.Push(t);
				}
			}
		}
	}

	public void RayCast(Func<RayCastInput, Element<T>, float> callback, ref RayCastInput input)
	{
		Stack<QuadTree<T>> stack = new Stack<QuadTree<T>>();
		stack.Push(this);
		float num = input.MaxFraction;
		FVector2 point = input.Point1;
		FVector2 p = point + (input.Point2 - input.Point1) * num;
		RayCastInput arg = default(RayCastInput);
		while (stack.Count > 0)
		{
			QuadTree<T> quadTree = stack.Pop();
			if (!RayCastAABB(quadTree.Span, point, p))
			{
				continue;
			}
			foreach (Element<T> node in quadTree.Nodes)
			{
				if (RayCastAABB(node.Span, point, p))
				{
					arg.Point1 = input.Point1;
					arg.Point2 = input.Point2;
					arg.MaxFraction = num;
					float num2 = callback(arg, node);
					if (num2 == 0f)
					{
						return;
					}
					if (!(num2 <= 0f))
					{
						num = num2;
						p = point + (input.Point2 - input.Point1) * num;
					}
				}
			}
			if (IsPartitioned)
			{
				QuadTree<T>[] subTrees = quadTree.SubTrees;
				foreach (QuadTree<T> t in subTrees)
				{
					stack.Push(t);
				}
			}
		}
	}

	public void GetAllNodesR(ref List<Element<T>> nodes)
	{
		nodes.AddRange(Nodes);
		if (IsPartitioned)
		{
			QuadTree<T>[] subTrees = SubTrees;
			foreach (QuadTree<T> quadTree in subTrees)
			{
				quadTree.GetAllNodesR(ref nodes);
			}
		}
	}

	public void RemoveNode(Element<T> node)
	{
		node.Parent.Nodes.Remove(node);
	}

	public void Reconstruct()
	{
		List<Element<T>> nodes = new List<Element<T>>();
		GetAllNodesR(ref nodes);
		Clear();
		nodes.ForEach(AddNode);
	}

	public void Clear()
	{
		Nodes.Clear();
		SubTrees = null;
	}
}
