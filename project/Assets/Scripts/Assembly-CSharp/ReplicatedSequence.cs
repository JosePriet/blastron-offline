using System;
using System.Collections.Generic;
using EB;
using EB.Replication;
using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

public class ReplicatedSequence : Sequence
{
	private const string _RPCName = "Activate";

	private ReplicationView _view;

	private Dictionary<int, int> _digests = new Dictionary<int, int>();

	private List<int> _active = new List<int>();

	private EB.Buffer _buffer = new EB.Buffer(16384);

	private void Awake()
	{
		Manager.RegisterRPC("Activate", new EB.Action<uint, object, string>(_ActivateRPC));
		_view = GetComponent<ReplicationView>() ?? base.gameObject.AddComponent<ReplicationView>();
		_view.stateSynchronization = NetworkStateSynchronization.ReliableDeltaCompressed;
		_view.migrationSynchronization = NetworkMigrationSynchronization.Host;
		_view.updateInterval = 0.5f;
		_view.observed = this;
		AllocateNodes();
	}

	protected override void Start()
	{
		if (_view.isMine)
		{
			StartSequence();
		}
	}

	protected override void DestroyMe()
	{
		Manager.Destroy(base.gameObject);
	}

	private void OnSerializeView(BitStream stream)
	{
		_buffer.Reset();
		if (stream.isWriting)
		{
			foreach (KeyValuePair<int, Node> allNode in _allNodes)
			{
				int key = allNode.Key;
				Node value = allNode.Value;
				short data = (short)key;
				bool flag = false;
				if (value is Operation)
				{
					Operation operation = (Operation)value;
					if (operation.IsActive)
					{
						data *= -1;
						flag = true;
					}
				}
				BitStream bitStream = new BitStream(_buffer, true);
				value.Serialize(bitStream);
				EB.Buffer data2 = bitStream.Slice();
				int num = data2.Digest();
				int value2 = 0;
				if (!_digests.TryGetValue(key, out value2) || value2 != num || flag)
				{
					stream.Serialize(ref data);
					stream.Serialize(ref data2);
					_digests[key] = num;
				}
			}
			return;
		}
		_active.Clear();
		while (stream.DataAvailable)
		{
			short data3 = 0;
			stream.Serialize(ref data3);
			if (data3 < 0)
			{
				data3 *= -1;
				_active.Add(data3);
			}
			byte[] data4 = null;
			stream.Serialize(ref data4);
			Node node = GetNode(data3);
			if (node != null)
			{
				BitStream bs = new BitStream(data4);
				node.Serialize(bs);
			}
			else
			{
				EB.Debug.LogError("Panic!: can't find node to serialize!!!!! id: " + data3);
			}
		}
	}

	private void OnTransferedOwnership()
	{
		if (!_view.isMine)
		{
			return;
		}
		EB.Debug.LogError("Replicated Sequence Transferred Ownership");
		LinkNodes();
		foreach (int item in _active)
		{
			Operation operation = (Operation)GetNode(item);
			operation.Activate();
		}
		ActivateVariables();
	}

	public static void NetworkActivate(object target, Type eventType)
	{
		uint localPlayerId = Manager.LocalPlayerId;
		Manager.RPC("Activate", RPCMode.Server, localPlayerId, target, eventType.Name);
	}

	private static void _ActivateRPC(uint fromId, object target, string type)
	{
		Type typeFromName = Utils.GetTypeFromName(type);
		if (typeFromName == null)
		{
			EB.Debug.LogError("Failed to get type " + typeFromName);
		}
		EB.Sequence.Component.Activate(null, target, typeFromName);
	}
}
