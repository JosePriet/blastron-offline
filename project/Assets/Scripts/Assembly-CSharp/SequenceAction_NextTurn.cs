using System.Collections.Generic;
using EB;
using EB.Replication;
using EB.Sequence;
using EB.Sequence.Runtime;
using EB.Sparx;
using UnityEngine;

[MenuItem(Path = "Actions/Next Turn")]
public class SequenceAction_NextTurn : EB.Sequence.Runtime.Action
{
	private Game _game;

	private List<uint> _order;

	private float _remaining;

	[Variable(ExpectedType = typeof(uint))]
	public Variable PlayerId = Variable.Null;

	[Variable(ExpectedType = typeof(uint), Direction = Direction.Out)]
	public Variable NextId = Variable.Null;

	[Variable(ExpectedType = typeof(float))]
	public Variable TurnLength = Variable.Null;

	[Variable(ExpectedType = typeof(float))]
	public Variable TurnLength2Player = Variable.Null;

	[Variable(ExpectedType = typeof(float))]
	public Variable TurnLengthAI = Variable.Null;

	[Variable(ExpectedType = typeof(float))]
	public Variable TruncateTime = Variable.Null;

	[Variable(ExpectedType = typeof(GameObject))]
	public Variable YourTurnSound = Variable.Null;

	[Trigger]
	public Trigger TurnEnded = new Trigger();

	[Trigger]
	public Trigger RoundEnded = new Trigger();

	public override void Init()
	{
		base.Init();
		_game = Hub.Instance.GameManager.Game;
		_order = new List<uint>(_game.Players.Count);
		uint sortOrder = GameAttributes.GetSortOrder();
		List<uint> list = new List<uint>(_game.Players.Count);
		foreach (EB.Sparx.Player player in _game.Players)
		{
			list.Add(player.PlayerId);
		}
		list.Sort((uint a, uint b) =>
		{
			a ^= sortOrder;
			b ^= sortOrder;
			return a.CompareTo(b);
		});
		int num = -1;
		while (list.Count > 0)
		{
			uint num2 = list[0];
			list.RemoveAt(0);
			int teamId = Scoring.Instance.GetTeamId(num2);
			if (teamId != num)
			{
				_order.Add(num2);
				num = teamId;
			}
			else
			{
				list.Add(num2);
			}
		}
		GameHUD.Instance.SetPlayerOrder(_order);
	}

	[Entry]
	public void StartGame()
	{
		uint value = PlayerId.GetValue<uint>();
		uint nextId = GetNextId(value, false);
		NextId.Value = nextId;
		TurnEnded.Invoke();
		EB.Replication.Manager.RPC("TurnHudVisible", RPCMode.All);
	}

	public override bool Update()
	{
		_remaining -= UnityEngine.Time.deltaTime;
		if (_remaining <= 0f)
		{
			uint value = PlayerId.GetValue<uint>();
			uint nextId = GetNextId(value, true);
			NextId.Value = nextId;
			EB.Replication.Manager.RPC("EndTurn", RPCMode.All, value, nextId);
			int num = _order.IndexOf(value);
			int num2 = _order.IndexOf(nextId);
			if (num2 <= num)
			{
				RoundEnded.Invoke();
			}
			TurnEnded.Invoke();
			return false;
		}
		return true;
	}

	[Entry]
	public void NextTurn()
	{
		if (_game == null)
		{
			EB.Debug.LogError("Error: no game!");
			return;
		}
		uint value = PlayerId.GetValue<uint>();
		uint nextId = GetNextId(value, false);
		PlayerId.Value = nextId;
		Player player = LevelData.Instance.GetPlayer(nextId);
		if ((bool)player && player.IsAI)
		{
			_remaining = TurnLengthAI.GetValue<float>();
		}
		else
		{
			_remaining = TurnLength.GetValue<float>();
			if (EB.Replication.Manager.Players.Count <= 2)
			{
				float value2 = TurnLength2Player.GetValue<float>();
				if (value2 > 0f)
				{
					_remaining = value2;
				}
			}
		}
		GameObject value3 = YourTurnSound.GetValue<GameObject>();
		if (value3 != null && player != null && nextId == EB.Replication.Manager.LocalPlayerId)
		{
			AudioEvent component = value3.GetComponent<AudioEvent>();
			AudioBridge.Instance.Play(component);
		}
		EB.Replication.Manager.RPC("NextTurn", RPCMode.All, nextId, _remaining);
		EB.Sequence.Component.Activate(null, nextId, typeof(SequenceEvent_BeginTurn));
		Activate();
	}

	[Entry]
	public void Truncate()
	{
		float value = TruncateTime.GetValue<float>();
		if (_remaining >= value)
		{
			_remaining = value;
			EB.Replication.Manager.RPC("SetTurnTimer", RPCMode.All, _remaining);
		}
	}

	[Entry]
	public void EndTurn()
	{
		_remaining = 0f;
	}

	private uint GetNextId(uint current, bool updateTurnMade)
	{
		int num = _order.IndexOf(current);
		int num2 = 0;
		if (num >= 0)
		{
			num2 = num + 1;
		}
		Scoring.ScoringMode mode = Scoring.Instance.Mode;
		for (int i = 0; i < _order.Count; i++)
		{
			num = (num2 + i) % _order.Count;
			uint num3 = _order[num];
			bool flag = true;
			if (mode == Scoring.ScoringMode.Order)
			{
				List<Scoring.DeadPlayer> deathOrder = Scoring.Instance.GetDeathOrder();
				foreach (Scoring.DeadPlayer item in deathOrder)
				{
					if (item.playerId == num3)
					{
						EB.Debug.Log("this player is dead");
						flag = false;
						break;
					}
				}
			}
			if (_game.GetPlayer(num3) != null && flag)
			{
				return num3;
			}
		}
		return 0u;
	}

	public override void Serialize(BitStream bs)
	{
		bs.Serialize(ref _remaining);
	}
}
