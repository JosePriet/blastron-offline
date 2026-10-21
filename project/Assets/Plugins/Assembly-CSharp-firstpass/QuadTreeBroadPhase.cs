using System;
using System.Collections.Generic;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

public class QuadTreeBroadPhase : IBroadPhase
{
	private const int TreeUpdateThresh = 10000;

	private int _currID;

	private Dictionary<int, Element<FixtureProxy>> _idRegister;

	private List<Element<FixtureProxy>> _moveBuffer;

	private List<Pair> _pairBuffer;

	private QuadTree<FixtureProxy> _quadTree;

	private int _treeMoveNum;

	public int ProxyCount
	{
		get
		{
			return _idRegister.Count;
		}
	}

	public QuadTreeBroadPhase(AABB span)
	{
		_quadTree = new QuadTree<FixtureProxy>(span, 5, 10);
		_idRegister = new Dictionary<int, Element<FixtureProxy>>();
		_moveBuffer = new List<Element<FixtureProxy>>();
		_pairBuffer = new List<Pair>();
	}

	public void GetFatAABB(int proxyID, out AABB aabb)
	{
		if (_idRegister.ContainsKey(proxyID))
		{
			aabb = _idRegister[proxyID].Span;
			return;
		}
		throw new KeyNotFoundException("proxyID not found in register");
	}

	public void UpdatePairs(BroadphaseDelegate callback)
	{
		_pairBuffer.Clear();
		Element<FixtureProxy> qtnode;
		foreach (Element<FixtureProxy> item in _moveBuffer)
		{
			qtnode = item;
			Query((int proxyID) => PairBufferQueryCallback(proxyID, qtnode.Value.ProxyId), ref qtnode.Span);
		}
		_moveBuffer.Clear();
		_pairBuffer.Sort();
		int num = 0;
		while (num < _pairBuffer.Count)
		{
			Pair pair = _pairBuffer[num];
			FixtureProxy proxyA = GetProxy(pair.ProxyIdA);
			FixtureProxy proxyB = GetProxy(pair.ProxyIdB);
			callback(ref proxyA, ref proxyB);
			for (num++; num < _pairBuffer.Count && _pairBuffer[num].ProxyIdA == pair.ProxyIdA && _pairBuffer[num].ProxyIdB == pair.ProxyIdB; num++)
			{
			}
		}
	}

	public bool TestOverlap(int proxyIdA, int proxyIdB)
	{
		AABB aabb;
		GetFatAABB(proxyIdA, out aabb);
		AABB aabb2;
		GetFatAABB(proxyIdB, out aabb2);
		return AABB.TestOverlap(ref aabb, ref aabb2);
	}

	public int AddProxy(ref FixtureProxy proxy)
	{
		int num = (proxy.ProxyId = _currID++);
		AABB span = Fatten(ref proxy.AABB);
		Element<FixtureProxy> element = new Element<FixtureProxy>(proxy, span);
		_idRegister.Add(num, element);
		_quadTree.AddNode(element);
		return num;
	}

	public void RemoveProxy(int proxyId)
	{
		if (_idRegister.ContainsKey(proxyId))
		{
			Element<FixtureProxy> element = _idRegister[proxyId];
			UnbufferMove(element);
			_idRegister.Remove(proxyId);
			_quadTree.RemoveNode(element);
			return;
		}
		throw new KeyNotFoundException("proxyID not found in register");
	}

	public void MoveProxy(int proxyId, ref AABB aabb, FVector2 displacement)
	{
		AABB aabb2;
		GetFatAABB(proxyId, out aabb2);
		if (!aabb2.Contains(ref aabb))
		{
			AABB aABB = aabb;
			FVector2 fVector = new FVector2(0.1f, 0.1f);
			aABB.LowerBound -= fVector;
			aABB.UpperBound += fVector;
			FVector2 fVector2 = 2f * displacement;
			if (fVector2.X < 0f)
			{
				aABB.LowerBound.X += fVector2.X;
			}
			else
			{
				aABB.UpperBound.X += fVector2.X;
			}
			if (fVector2.Y < 0f)
			{
				aABB.LowerBound.Y += fVector2.Y;
			}
			else
			{
				aABB.UpperBound.Y += fVector2.Y;
			}
			Element<FixtureProxy> element = _idRegister[proxyId];
			element.Value.AABB = aABB;
			element.Span = aABB;
			ReinsertNode(element);
			BufferMove(element);
		}
	}

	public FixtureProxy GetProxy(int proxyId)
	{
		if (_idRegister.ContainsKey(proxyId))
		{
			return _idRegister[proxyId].Value;
		}
		throw new KeyNotFoundException("proxyID not found in register");
	}

	public void TouchProxy(int proxyId)
	{
		if (_idRegister.ContainsKey(proxyId))
		{
			BufferMove(_idRegister[proxyId]);
			return;
		}
		throw new KeyNotFoundException("proxyID not found in register");
	}

	public void Query(Func<int, bool> callback, ref AABB query)
	{
		_quadTree.QueryAABB(TransformPredicate(callback), ref query);
	}

	public void RayCast(Func<RayCastInput, int, float> callback, ref RayCastInput input)
	{
		_quadTree.RayCast(TransformRayCallback(callback), ref input);
	}

	private AABB Fatten(ref AABB aabb)
	{
		FVector2 fVector = new FVector2(0.1f, 0.1f);
		return new AABB(aabb.LowerBound - fVector, aabb.UpperBound + fVector);
	}

	private Func<Element<FixtureProxy>, bool> TransformPredicate(Func<int, bool> idPredicate)
	{
		return (Element<FixtureProxy> qtnode) => idPredicate(qtnode.Value.ProxyId);
	}

	private Func<RayCastInput, Element<FixtureProxy>, float> TransformRayCallback(Func<RayCastInput, int, float> callback)
	{
		return (RayCastInput input, Element<FixtureProxy> qtnode) => callback(input, qtnode.Value.ProxyId);
	}

	private bool PairBufferQueryCallback(int proxyID, int baseID)
	{
		if (proxyID == baseID)
		{
			return true;
		}
		Pair item = new Pair
		{
			ProxyIdA = Math.Min(proxyID, baseID),
			ProxyIdB = Math.Max(proxyID, baseID)
		};
		_pairBuffer.Add(item);
		return true;
	}

	private void ReconstructTree()
	{
		_quadTree.Clear();
		foreach (Element<FixtureProxy> value in _idRegister.Values)
		{
			_quadTree.AddNode(value);
		}
	}

	private void ReinsertNode(Element<FixtureProxy> qtnode)
	{
		_quadTree.RemoveNode(qtnode);
		_quadTree.AddNode(qtnode);
		if (++_treeMoveNum > 10000)
		{
			ReconstructTree();
			_treeMoveNum = 0;
		}
	}

	private void BufferMove(Element<FixtureProxy> proxy)
	{
		_moveBuffer.Add(proxy);
	}

	private void UnbufferMove(Element<FixtureProxy> proxy)
	{
		_moveBuffer.Remove(proxy);
	}
}
