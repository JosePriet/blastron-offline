using System;
using System.Collections.Generic;
using System.Reflection;
using EB.Sequence.Runtime;
using EB.Sequence.Serialization;
using UnityEngine;

namespace EB.Sequence
{
	public class Component : MonoBehaviour
	{
		private class LinkFinder
		{
			private int _id;

			public LinkFinder(int id)
			{
				_id = id;
			}

			public bool Find(Link link)
			{
				return link.inId == _id || link.outId == _id;
			}
		}

		[HideInInspector]
		public int NextId = 1;

		public List<EB.Sequence.Serialization.Node> Nodes = new List<EB.Sequence.Serialization.Node>();

		public List<Link> Links = new List<Link>();

		protected List<Operation> _activeOperations = new List<Operation>();

		protected Dictionary<int, EB.Sequence.Runtime.Node> _allNodes = new Dictionary<int, EB.Sequence.Runtime.Node>();

		private List<Operation> _removeList = new List<Operation>();

		private bool _kill;

		private static int _numNodes;

		public EB.Sequence.Serialization.Node FindById(int id)
		{
			foreach (EB.Sequence.Serialization.Node node in Nodes)
			{
				if (node.id == id)
				{
					return node;
				}
			}
			return null;
		}

		public void Clear()
		{
			Nodes.Clear();
			Links.Clear();
			NextId = 1;
		}

		public EB.Sequence.Serialization.Node AddNode(Type type)
		{
			EB.Sequence.Serialization.Node node = Utils.CreateSerializationNode(type, NextId++);
			Nodes.Add(node);
			return node;
		}

		public EB.Sequence.Serialization.Node AddNode(EB.Sequence.Serialization.Node node)
		{
			node.id = NextId++;
			Nodes.Add(node);
			return node;
		}

		public void RemoveNode(int nodeId)
		{
			EB.Sequence.Serialization.Node node = FindById(nodeId);
			if (node != null)
			{
				Nodes.Remove(node);
				Links.RemoveAll(new LinkFinder(nodeId).Find);
			}
		}

		protected virtual void OnDestroy()
		{
			EndSequence();
		}

		public Link AddLink(EB.Sequence.Serialization.Node outNode, string outName, EB.Sequence.Serialization.Node inNode, string inName)
		{
			if (Utils.ValidateLink(outNode, outName, inNode, inName) == Utils.ValidateLinkResult.Ok)
			{
				return AddLink(outNode.id, outName, inNode.id, inName);
			}
			return null;
		}

		private Link AddLink(int outId, string outName, int inId, string inName)
		{
			if (FindLink(outId, outName, inId, inName) != null)
			{
				return null;
			}
			Link link = new Link();
			link.outId = outId;
			link.outName = outName;
			link.inId = inId;
			link.inName = inName;
			Links.Add(link);
			return link;
		}

		public Link FindLink(int outId, string outName, int inId, string inName)
		{
			foreach (Link link in Links)
			{
				if (link.outId == outId && link.inId == inId && link.outName == outName && link.inName == inName)
				{
					return link;
				}
			}
			return null;
		}

		public void RmvLink(Link link)
		{
			Links.Remove(link);
		}

		private static string GoName(GameObject go)
		{
			return (!(go != null)) ? "null" : go.name;
		}

		public static void Activate(GameObject instigator, object target, Type eventType)
		{
			EB.Sequence.Runtime.Event.Activate(instigator, target, eventType);
		}

		public void AddToUpdate(Operation node)
		{
			if (!_activeOperations.Contains(node))
			{
				_activeOperations.Add(node);
			}
		}

		public void VariableChanged(Variable variable)
		{
			foreach (Link link in Links)
			{
				if (link.outId == variable.Id)
				{
					EB.Sequence.Runtime.Node node = GetNode(link.inId);
					if (node != null)
					{
						node.OnInputChanged();
					}
				}
			}
		}

		public EB.Sequence.Runtime.Node GetNode(int id)
		{
			EB.Sequence.Runtime.Node value;
			if (_allNodes.TryGetValue(id, out value))
			{
				return value;
			}
			return null;
		}

		protected void Update()
		{
			if (_activeOperations.Count > 0)
			{
				for (int i = 0; i < _activeOperations.Count; i++)
				{
					bool flag = false;
					Operation operation = _activeOperations[i];
					if (!operation.Update())
					{
						_removeList.Add(operation);
					}
				}
				if (_removeList.Count > 0)
				{
					foreach (Operation remove in _removeList)
					{
						remove.IsActive = false;
						_activeOperations.Remove(remove);
					}
					_removeList.Clear();
				}
			}
			if (_kill)
			{
				EndSequence();
				DestroyMe();
			}
		}

		protected virtual void DestroyMe()
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}

		public void KillSequence()
		{
			if (!_kill)
			{
				_kill = true;
				Activate(base.gameObject, base.gameObject, typeof(SequenceEvent_SequenceKilled));
			}
		}

		protected void AllocateNodes()
		{
			if (_allNodes.Count > 0)
			{
				return;
			}
			foreach (EB.Sequence.Serialization.Node node in Nodes)
			{
				_allNodes[node.id] = Utils.CreateRuntimeNode(node, this);
			}
			_numNodes += _allNodes.Count;
		}

		public virtual void StartSequence()
		{
			AllocateNodes();
			LinkNodes();
			ActivateVariables();
			Activate(base.gameObject, base.gameObject, typeof(SequenceEvent_SequenceStarted));
		}

		protected void ActivateVariables()
		{
			foreach (EB.Sequence.Runtime.Node value in _allNodes.Values)
			{
				if (value is Variable)
				{
					VariableChanged((Variable)value);
				}
			}
		}

		protected void LinkNodes()
		{
			foreach (Link link in Links)
			{
				EB.Sequence.Runtime.Node node = GetNode(link.inId);
				EB.Sequence.Runtime.Node node2 = GetNode(link.outId);
				if (node == null)
				{
					Debug.LogError("Invalid Link: inNode is null " + link.inId + " " + base.name);
					continue;
				}
				if (node2 == null)
				{
					Debug.LogError("Invalid Link: outNode is null " + link.outId + " " + base.name);
					continue;
				}
				LinkFieldInfo linkFieldInfo = Utils.ParseField(link.inName);
				LinkFieldInfo linkFieldInfo2 = Utils.ParseField(link.outName);
				if (node2 is Variable && (link.outName == "Value" || link.outName == string.Empty))
				{
					FieldInfo field = node.GetType().GetField(linkFieldInfo.field);
					if (field != null)
					{
						if (field.FieldType.IsArray)
						{
							List<Variable> list = new List<Variable>();
							Variable[] array = (Variable[])field.GetValue(node);
							if (array != null)
							{
								Variable[] array2 = array;
								foreach (Variable item in array2)
								{
									list.Add(item);
								}
							}
							if (linkFieldInfo.index == -1)
							{
								list.Add((Variable)node2);
							}
							else
							{
								while (list.Count <= linkFieldInfo.index)
								{
									list.Add(Variable.Null);
								}
								list[linkFieldInfo.index] = (Variable)node2;
							}
							field.SetValue(node, list.ToArray());
						}
						else
						{
							field.SetValue(node, node2);
						}
					}
					else
					{
						Debug.LogWarning("Failed to find variable link: " + link.inName);
					}
					continue;
				}
				FieldInfo field2 = node2.GetType().GetField(linkFieldInfo2.field);
				if (field2 != null)
				{
					Trigger trigger = null;
					if (field2.FieldType.IsArray)
					{
						List<Trigger> list2 = new List<Trigger>();
						Trigger[] array3 = (Trigger[])field2.GetValue(node2);
						if (array3 != null)
						{
							Trigger[] array4 = array3;
							foreach (Trigger item2 in array4)
							{
								list2.Add(item2);
							}
						}
						if (linkFieldInfo2.index == -1)
						{
							trigger = new Trigger();
							list2.Add(trigger);
						}
						else
						{
							while (list2.Count <= linkFieldInfo2.index)
							{
								list2.Add(new Trigger());
							}
							trigger = list2[linkFieldInfo2.index];
						}
						field2.SetValue(node2, list2.ToArray());
					}
					else
					{
						trigger = (Trigger)field2.GetValue(node2);
					}
					MethodInfo method = node.GetType().GetMethod(link.inName);
					if (method != null)
					{
						Delegate obj = Delegate.CreateDelegate(typeof(Action), node, method, false);
						if (obj != null)
						{
							trigger.trigger += (Action)obj;
							continue;
						}
						Debug.LogError("Failed to create delegate for function {0} on type {1}", link.inName, node.GetType());
					}
					else
					{
						Debug.LogError("Failed to get methodInfo for function {0} on type {1}", link.inName, node.GetType());
					}
				}
				else
				{
					Debug.LogWarning("Failed to find link: " + link.outName);
				}
			}
		}

		public virtual void EndSequence()
		{
			_numNodes -= _allNodes.Count;
			foreach (EB.Sequence.Runtime.Node value in _allNodes.Values)
			{
				value.Dispose();
			}
			_allNodes.Clear();
			_activeOperations.Clear();
		}
	}
}
