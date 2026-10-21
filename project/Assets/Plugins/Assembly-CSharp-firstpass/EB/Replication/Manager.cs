using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using EB.Collections;
using EB.Sparx;
using Microsoft.Xna.Framework;
using UnityEngine;

namespace EB.Replication
{
	public class Manager
	{
		private class AllocationInfo
		{
			private uint _p;

			private uint _n;

			private uint _m;

			private bool _s;

			public AllocationInfo(uint p, uint n, uint m, bool s)
			{
				_p = p;
				_n = n;
				_m = n + m;
				_s = s;
			}

			public ViewId Allocate()
			{
				if (_n == _m)
				{
					throw new Exception("Ran out of view ids: " + ToString());
				}
				ViewId result = new ViewId
				{
					p = _p,
					n = _n,
					s = _s
				};
				_n++;
				return result;
			}

			public void ClearSceneFlag()
			{
				_s = false;
			}

			public override string ToString()
			{
				return string.Format("[AllocationInfo: p{0}, n{1}, m{2}, s{3}]", _p, _n, _m, _s);
			}
		}

		private interface IRpcCaller
		{
			void Invoke(object[] args);
		}

		private class DelegateRpc : IRpcCaller
		{
			private Delegate _cb;

			public DelegateRpc(Delegate cb)
			{
				_cb = cb;
			}

			public void Invoke(object[] args)
			{
				if (_cb != null)
				{
					try
					{
						_cb.DynamicInvoke(args);
					}
					catch
					{
					}
				}
			}
		}

		private class MethodRpc : IRpcCaller
		{
			private object _t;

			private string _m;

			public MethodRpc(object target, string method)
			{
				_t = target;
				_m = method;
			}

			public void Invoke(object[] args)
			{
				if (_t != null)
				{
					try
					{
						_t.GetType().InvokeMember(_m, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, _t, args);
					}
					catch (Exception ex)
					{
						Debug.LogError("Failed to call " + _m + " " + ex);
					}
				}
			}
		}

		private enum VarArgsType
		{
			Null = 0,
			Boolean = 1,
			Byte = 2,
			Short = 3,
			UShort = 4,
			Int = 5,
			UInt = 6,
			Long = 7,
			ULong = 8,
			Single = 9,
			Double = 10,
			Object = 11,
			String = 12,
			Vector2 = 13,
			Vector3 = 14,
			Vector4 = 15,
			Quaternion = 16,
			ViewId = 17,
			FVector2 = 18,
			Serializable = 19
		}

		private const string LoadLevelRPC = "0";

		private const string LoadLevelCompleteRPC = "1";

		private const string InstantiateRPC = "2";

		private const string TransferSceneOwnershipRPC = "4";

		private const string TransferOwnershipRPC = "5";

		private const string DestroyRPC = "6";

		private const string BroadcastRPC = "7";

		private const uint MaxNetworkViewsPerObject = 10u;

		private const uint MaxNetworkViewsPerScene = 1000u;

		private const int MaxPacketSize = 512;

		private static Dictionary<ViewId, View> _viewMap;

		private static Dictionary<string, IRpcCaller> _rpc;

		private static Game _game;

		private static EB.Collections.Stack<AllocationInfo> _allocations;

		private static uint _localP;

		private static uint _localN;

		private static List<uint> _myIds;

		private static byte _viewCmd;

		private static byte _rpcCmd;

		private static byte _viewRpcCmd;

		private static DateTime _lastUpdate;

		private static Dictionary<uint, List<Buffer>> _reliable;

		private static Dictionary<uint, List<Buffer>> _unreliable;

		private static Dictionary<int, UnityEngine.Object> _objMap;

		private static Buffer _buffer;

		private static Buffer _tmp;

		private static bool _needsFlush;

		private static bool _pauseViewUpdates;

		private static Dictionary<byte, Type> _serializablesIn;

		private static Dictionary<Type, byte> _serializablesOut;

		private static byte _serializeNext;

		private static Hashtable _doneLoading;

		public static int UpdateRate { get; set; }

		public static bool IsHost
		{
			get
			{
				return _game == null || _game.IsHost;
			}
		}

		public static uint LocalPlayerId
		{
			get
			{
				return _localP;
			}
		}

		public static Player LocalPlayer
		{
			get
			{
				return (_game == null) ? null : _game.LocalPlayer;
			}
		}

		public static int LocalPlayerIndex
		{
			get
			{
				return (_game != null) ? _game.LocalPlayer.Index : 0;
			}
		}

		public static bool IsConnected
		{
			get
			{
				return _game != null;
			}
		}

		public static List<Player> Players
		{
			get
			{
				List<Player> list = null;
				if (_game != null)
				{
					list = _game.Players;
				}
				else
				{
					list = new List<Player>();
					list.Add(new Player(new Id(), 0u));
				}
				return list;
			}
		}

		public static bool IsLocalGame
		{
			get
			{
				return _game == null || _game.Network.IsLocal;
			}
		}

		public static bool IsLoadingLevel { get; private set; }

		static Manager()
		{
			_viewMap = new Dictionary<ViewId, View>();
			_rpc = new Dictionary<string, IRpcCaller>();
			_game = null;
			_allocations = new EB.Collections.Stack<AllocationInfo>();
			_myIds = new List<uint>();
			_viewCmd = 0;
			_rpcCmd = 1;
			_viewRpcCmd = 2;
			_reliable = new Dictionary<uint, List<Buffer>>();
			_unreliable = new Dictionary<uint, List<Buffer>>();
			_objMap = new Dictionary<int, UnityEngine.Object>();
			_buffer = new Buffer(262144);
			_tmp = new Buffer(16384);
			_needsFlush = false;
			_pauseViewUpdates = false;
			_serializablesIn = new Dictionary<byte, Type>();
			_serializablesOut = new Dictionary<Type, byte>();
			_serializeNext = 19;
			_doneLoading = new Hashtable();
			UpdateRate = 15;
			RegisterRPC("0", new Action<uint, uint, string>(_LoadLevelRPC));
			RegisterRPC("1", new Action<uint>(_LoadLevelCompleteRPC));
			RegisterRPC("2", new Action<uint, uint, UnityEngine.Object, Vector3, Quaternion>(_InstantiateRPC));
			RegisterRPC("4", new Action<uint, uint>(_TransferSceneOwnershipRPC));
			RegisterRPC("5", new Action<ViewId, uint, uint>(_TransferOwnershipRPC));
			RegisterRPC("6", new Action<uint, uint>(_DestroyRPC));
			RegisterRPC("7", new Action<string>(_BroadcastMessageRPC));
		}

		public static View GetView(ViewId id)
		{
			View value;
			_viewMap.TryGetValue(id, out value);
			return value;
		}

		public static GameObject GetObject(ViewId id)
		{
			View view = GetView(id);
			return (!view) ? null : view.gameObject;
		}

		public static void PauseViewUpdates(bool pause)
		{
			_pauseViewUpdates = pause;
		}

		public static void SetGame(Game game)
		{
			_game = game;
			_localP = game.LocalPlayer.PlayerId;
			_localN = 1u;
			_lastUpdate = DateTime.Now;
			_myIds.Clear();
			_myIds.Add(_localP);
			for (uint num = 10u; num <= 20; num++)
			{
				_myIds.Add(num);
			}
			_buffer.Reset();
		}

		public static bool IsMine(ViewId id)
		{
			return _myIds.Contains(id.p) || id == default(ViewId);
		}

		public static Player GetPlayer(uint p)
		{
			if (_game != null)
			{
				return _game.GetPlayer(p);
			}
			return null;
		}

		public static void ClearGame()
		{
			_game = null;
		}

		public static void RegisterSerializable(Type type)
		{
			_serializablesIn[_serializeNext] = type;
			_serializablesOut[type] = _serializeNext;
			_serializeNext++;
		}

		public static ViewId Register(View view)
		{
			ViewId viewId = AllocateViewId();
			_viewMap[viewId] = view;
			return viewId;
		}

		public static void Unregister(View view)
		{
			_viewMap.Remove(view.viewId);
		}

		public static void RegisterRPC(string name, Delegate function)
		{
			_rpc[name] = new DelegateRpc(function);
		}

		public static void RegisterPrefab(UnityEngine.Object obj)
		{
			_objMap[Hash.StringHash(obj.name)] = obj;
		}

		public static void RegisterResource(string path)
		{
			RegisterPrefab(Assets.Load(path));
		}

		public static void RegisterResourceFolder(string folder)
		{
			UnityEngine.Object[] array = Assets.LoadAll(folder, typeof(UnityEngine.Object));
			foreach (UnityEngine.Object obj in array)
			{
				RegisterPrefab(obj);
			}
		}

		public static void RegisterRPCs(object obj)
		{
			Type type = obj.GetType();
			MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			MethodInfo[] array = methods;
			foreach (MethodInfo methodInfo in array)
			{
				if (methodInfo.GetCustomAttributes(typeof(RPC), true).Length > 0)
				{
					_rpc[methodInfo.Name] = new MethodRpc(obj, methodInfo.Name);
				}
			}
		}

		public static void TransferSceneOwnership()
		{
			uint localN = _localN;
			_localN += 1000u;
			RPC("4", RPCMode.All, _localP, localN);
		}

		public static ViewId TransferOwnershipTo(View view, uint owner)
		{
			if (!IsLocalGame)
			{
				Debug.LogError("Cant call this silly function in a online game!");
				return view.viewId;
			}
			ViewId viewId = view.viewId;
			_viewMap.Remove(viewId);
			viewId.p = owner;
			_viewMap.Add(viewId, view);
			return viewId;
		}

		public static void TransferOwnership(ViewId viewId)
		{
			uint localN = _localN;
			_localN++;
			RPC("5", RPCMode.AllBuffered, viewId, _localP, localN);
		}

		private static void _DestroyRPC(uint p, uint n)
		{
			ViewId viewId = new ViewId
			{
				n = n,
				p = p
			};
			View value;
			if (_viewMap.TryGetValue(viewId, out value))
			{
				UnityEngine.Object.Destroy(value.gameObject);
			}
			else
			{
				Debug.LogError("Failed to destroy object " + viewId);
			}
		}

		private static void _TransferOwnershipRPC(ViewId viewId, uint newOwner, uint nextId)
		{
			View value;
			if (!_viewMap.TryGetValue(viewId, out value))
			{
				Debug.LogError("Transfer ownership failed, can't find object for view id " + viewId);
				return;
			}
			Push(newOwner, nextId, 1u, viewId.s);
			value.TransferOwnership();
			Pop();
		}

		private static void _TransferSceneOwnershipRPC(uint newOwner, uint nextId)
		{
			List<View> list = new List<View>();
			foreach (KeyValuePair<ViewId, View> item in _viewMap)
			{
				if (item.Key.s)
				{
					list.Add(item.Value);
				}
			}
			list.Sort((View a, View b) => a.viewId.CompareTo(b.viewId));
			Push(newOwner, nextId, 1000u, true);
			foreach (View item2 in list)
			{
				item2.TransferOwnership();
			}
			Pop();
		}

		public static void Update()
		{
			if (_game == null)
			{
				return;
			}
			float num = 1f / (float)UpdateRate;
			double totalSeconds = (DateTime.Now - _lastUpdate).TotalSeconds;
			if (totalSeconds < (double)num)
			{
				if (_needsFlush)
				{
					Flush();
				}
				return;
			}
			if (!IsLocalGame && !_pauseViewUpdates)
			{
				SerializeViews();
			}
			_lastUpdate = DateTime.Now;
			Flush();
		}

		public static void OnPlayerLeft(Game game, Player player)
		{
			if (game != _game)
			{
				return;
			}
			uint playerId = player.PlayerId;
			bool isHost = IsHost;
			List<View> list = new List<View>(16);
			List<View> list2 = new List<View>(16);
			foreach (KeyValuePair<ViewId, View> item in _viewMap)
			{
				if (item.Key.p != playerId)
				{
					continue;
				}
				switch (item.Value.migrationSynchronization)
				{
				case NetworkMigrationSynchronization.Host:
					if (isHost)
					{
						list.Add(item.Value);
					}
					break;
				case NetworkMigrationSynchronization.Destroy:
					list2.Add(item.Value);
					break;
				}
			}
			foreach (View item2 in list2)
			{
				Debug.Log("Migration: Destroying " + item2.name);
				UnityEngine.Object.Destroy(item2.gameObject);
			}
			foreach (View item3 in list)
			{
				Debug.Log("Migration: Transfering " + item3.name);
				TransferOwnership(item3.viewId);
			}
			Flush();
		}

		public static void Receive(uint fromId, Buffer buffer)
		{
			try
			{
				BitStream bitStream = new BitStream(buffer, false);
				while (bitStream.DataAvailable)
				{
					byte data = 0;
					bitStream.Serialize(ref data);
					if (data == _rpcCmd)
					{
						ReadRPC(fromId, bitStream);
					}
					else if (data == _viewCmd)
					{
						ReadViewUpdate(fromId, bitStream);
					}
					else
					{
						if (data != _viewRpcCmd)
						{
							throw new ArgumentException("Unknow command " + data);
						}
						ReadViewRPC(fromId, bitStream);
					}
					bitStream.ResetBoolFlag();
				}
			}
			catch (Exception ex)
			{
				ArraySegment<byte> arraySegment = buffer.ToArraySegment(true);
				Debug.LogError("Invalid packet: " + Encoding.ToHexString(arraySegment.Array, arraySegment.Offset, arraySegment.Count, 1));
				throw ex;
			}
		}

		private static void Flush()
		{
			Flush(_reliable, Channel.Game_Reliable);
			Flush(_unreliable, Channel.Game_UnReliable);
			_buffer.Reset();
			_needsFlush = false;
		}

		private static void Send(uint playerId, Channel channel, Buffer buffer)
		{
			if (playerId == uint.MaxValue)
			{
				_game.Broadcast(channel, buffer);
			}
			else
			{
				_game.SendTo(playerId, channel, buffer);
			}
		}

		private static void Flush(Dictionary<uint, List<Buffer>> dic, Channel channel)
		{
			foreach (KeyValuePair<uint, List<Buffer>> item in dic)
			{
				uint key = item.Key;
				List<Buffer> value = item.Value;
				_tmp.Reset();
				foreach (Buffer item2 in value)
				{
					if (item2.Length > 512)
					{
						Send(key, channel, item2);
						continue;
					}
					if (_tmp.Length + item2.Length > 512)
					{
						Send(key, channel, _tmp);
						_tmp.Reset();
					}
					_tmp.WriteBuffer(item2);
				}
				if (_tmp.Length > 0)
				{
					Send(key, channel, _tmp);
					_tmp.Reset();
				}
				value.Clear();
			}
		}

		private static void SerializeViews()
		{
			foreach (KeyValuePair<ViewId, View> item in _viewMap)
			{
				ViewId key = item.Key;
				View value = item.Value;
				if (key.p != _localP || !value.enabled || !value.gameObject.active || !(value.observed != null) || value.stateSynchronization == NetworkStateSynchronization.Off)
				{
					continue;
				}
				_tmp.Reset();
				BitStream bitStream = new BitStream(_buffer, true);
				BitStream bitStream2 = new BitStream(_tmp, true);
				bitStream.Serialize(ref _viewCmd);
				bitStream.SerializeUInt24(ref key.n);
				value.Serialize(bitStream2);
				Buffer data = bitStream2.Slice();
				if (data.Length <= 0)
				{
					continue;
				}
				bitStream.Serialize(ref data);
				if (value.stateSynchronization == NetworkStateSynchronization.ReliableDeltaCompressed)
				{
					int num = data.Digest();
					if (num != value.lastHash)
					{
						value.lastHash = num;
						GetPacketList(uint.MaxValue, _reliable).Add(bitStream.Slice());
					}
				}
				else
				{
					GetPacketList(uint.MaxValue, _unreliable).Add(bitStream.Slice());
				}
			}
		}

		private static List<Buffer> GetPacketList(uint playerId, Dictionary<uint, List<Buffer>> dic)
		{
			List<Buffer> value;
			if (!dic.TryGetValue(playerId, out value))
			{
				value = new List<Buffer>();
				dic.Add(playerId, value);
			}
			return value;
		}

		public static void RPC(string name, RPCMode mode, params object[] args)
		{
			uint localN = _localN;
			_localN += 10u;
			if (IsLocalGame)
			{
				if (mode != RPCMode.Others && mode != RPCMode.OthersBuffered)
				{
					CallRPC(_localP, localN, name, args);
				}
				return;
			}
			switch (mode)
			{
			case RPCMode.All:
			case RPCMode.AllBuffered:
			{
				Buffer buffer = WriteRPC(uint.MaxValue, localN, name, args);
				Receive(_localP, buffer.Clone());
				break;
			}
			case RPCMode.Others:
			case RPCMode.OthersBuffered:
				WriteRPC(uint.MaxValue, localN, name, args);
				break;
			case RPCMode.Server:
				if (_game != null)
				{
					Player hostPlayer = _game.HostPlayer;
					if (hostPlayer.PlayerId == _localP)
					{
						CallRPC(_localP, localN, name, args);
					}
					else
					{
						WriteRPC(hostPlayer.PlayerId, localN, name, args);
					}
				}
				break;
			}
			if (mode != RPCMode.AllBuffered && mode != RPCMode.OthersBuffered)
			{
				_needsFlush = true;
			}
		}

		public static void ViewRPC(string name, ViewId id, RPCMode mode, params object[] args)
		{
			uint localN = _localN;
			_localN += 10u;
			if (IsLocalGame)
			{
				if (mode != RPCMode.Others && mode != RPCMode.OthersBuffered)
				{
					CallViewRPC(id.p, localN, name, id, args);
				}
				return;
			}
			switch (mode)
			{
			case RPCMode.All:
			case RPCMode.AllBuffered:
			{
				Buffer buffer = WriteViewRPC(uint.MaxValue, localN, id, name, args);
				Receive(_localP, buffer.Clone());
				break;
			}
			case RPCMode.Others:
			case RPCMode.OthersBuffered:
				WriteViewRPC(uint.MaxValue, localN, id, name, args);
				break;
			case RPCMode.Server:
				if (_game != null)
				{
					Player hostPlayer = _game.HostPlayer;
					if (hostPlayer.PlayerId == _localP)
					{
						CallViewRPC(_localP, localN, name, id, args);
					}
					else
					{
						WriteViewRPC(hostPlayer.PlayerId, localN, id, name, args);
					}
				}
				break;
			}
			if (mode != RPCMode.AllBuffered && mode != RPCMode.OthersBuffered)
			{
				_needsFlush = true;
			}
		}

		private static void ReadViewUpdate(uint fromId, BitStream bs)
		{
			ViewId viewId = new ViewId
			{
				p = fromId
			};
			bs.SerializeUInt24(ref viewId.n);
			byte[] data = null;
			bs.Serialize(ref data);
			View value;
			if (_viewMap.TryGetValue(viewId, out value))
			{
				BitStream bs2 = new BitStream(data);
				try
				{
					value.Serialize(bs2);
				}
				catch (Exception ex)
				{
					Debug.LogError("Exception while deserializing view {0}, buffer {1}, {2}, ex: {3}", viewId, data.Length, Encoding.ToHexString(data, 1), ex);
				}
			}
		}

		private static void ReadRPC(uint fromId, BitStream bs)
		{
			string data = string.Empty;
			bs.Serialize(ref data);
			uint data2 = 0u;
			bs.Serialize(ref data2);
			object[] args = null;
			SerializeVarAgs(bs, ref args);
			CallRPC(fromId, data2, data, args);
		}

		private static void ReadViewRPC(uint fromId, BitStream bs)
		{
			string data = string.Empty;
			bs.Serialize(ref data);
			ViewId data2 = default(ViewId);
			bs.Serialize(ref data2);
			uint data3 = 0u;
			bs.Serialize(ref data3);
			object[] args = null;
			SerializeVarAgs(bs, ref args);
			CallViewRPC(fromId, data3, data, data2, args);
		}

		private static Buffer WriteViewRPC(uint playerId, uint nextId, ViewId id, string name, object[] args)
		{
			BitStream bitStream = new BitStream(_buffer, true);
			bitStream.Serialize(ref _viewRpcCmd);
			bitStream.Serialize(ref name);
			bitStream.Serialize(ref id);
			bitStream.Serialize(ref nextId);
			SerializeVarAgs(bitStream, ref args);
			Buffer buffer = bitStream.Slice();
			GetPacketList(playerId, _reliable).Add(buffer);
			return buffer;
		}

		private static Buffer WriteRPC(uint playerId, uint nextId, string name, object[] args)
		{
			BitStream bitStream = new BitStream(_buffer, true);
			bitStream.Serialize(ref _rpcCmd);
			bitStream.Serialize(ref name);
			bitStream.Serialize(ref nextId);
			SerializeVarAgs(bitStream, ref args);
			Buffer buffer = bitStream.Slice();
			GetPacketList(playerId, _reliable).Add(buffer);
			return buffer;
		}

		private static void CallRPC(uint ownerId, uint nextId, string name, object[] args)
		{
			Push(ownerId, nextId, 10u, false);
			IRpcCaller value = null;
			if (_rpc.TryGetValue(name, out value))
			{
				value.Invoke(args);
			}
			else
			{
				Debug.LogError("Failed to call RPC : " + name);
			}
			Pop();
		}

		private static void CallViewRPC(uint ownerId, uint nextId, string name, ViewId Id, object[] args)
		{
			View value;
			if (!_viewMap.TryGetValue(Id, out value))
			{
				Debug.LogError("Failed to call RPC : " + name + " on view " + Id);
				return;
			}
			Push(ownerId, nextId, 10u, false);
			Component observed = value.observed;
			if (observed != null)
			{
				try
				{
					observed.GetType().InvokeMember(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, observed, args);
				}
				catch (MissingMethodException)
				{
					Debug.LogWarning("Missing method " + name + " on " + observed.name);
				}
				catch (Exception ex2)
				{
					Debug.LogError("Exception: on " + observed.name + " " + ex2.ToString());
				}
			}
			Pop();
		}

		private static void SerializeVarAgs(BitStream bs, ref object[] args)
		{
			byte value = 0;
			if (bs.isReading)
			{
				byte data = 0;
				bs.Serialize(ref data);
				args = new object[data];
				for (int i = 0; i < data; i++)
				{
					bs.Serialize(ref value);
					switch (value)
					{
					case 0:
						args[i] = null;
						continue;
					case 1:
					{
						bool data2 = false;
						bs.Serialize(ref data2);
						args[i] = data2;
						continue;
					}
					case 2:
					{
						byte data18 = 0;
						bs.Serialize(ref data18);
						args[i] = data18;
						continue;
					}
					case 3:
					{
						short data17 = 0;
						bs.Serialize(ref data17);
						args[i] = data17;
						continue;
					}
					case 4:
					{
						ushort data16 = 0;
						bs.Serialize(ref data16);
						args[i] = data16;
						continue;
					}
					case 5:
					{
						int data15 = 0;
						bs.Serialize(ref data15);
						args[i] = data15;
						continue;
					}
					case 6:
					{
						uint data14 = 0u;
						bs.Serialize(ref data14);
						args[i] = data14;
						continue;
					}
					case 7:
					{
						long data13 = 0L;
						bs.Serialize(ref data13);
						args[i] = data13;
						continue;
					}
					case 8:
					{
						ulong data12 = 0uL;
						bs.Serialize(ref data12);
						args[i] = data12;
						continue;
					}
					case 9:
					{
						float data11 = 0f;
						bs.Serialize(ref data11);
						args[i] = data11;
						continue;
					}
					case 10:
					{
						double data10 = 0.0;
						bs.Serialize(ref data10);
						args[i] = data10;
						continue;
					}
					case 12:
					{
						string data9 = string.Empty;
						bs.Serialize(ref data9);
						args[i] = data9;
						continue;
					}
					case 11:
					{
						int data8 = 0;
						bs.Serialize(ref data8);
						args[i] = GetObjectFromHashId(data8);
						continue;
					}
					case 13:
					{
						Vector2 data7 = Vector2.zero;
						bs.Serialize(ref data7);
						args[i] = data7;
						continue;
					}
					case 14:
					{
						Vector3 data6 = Vector3.zero;
						bs.Serialize(ref data6);
						args[i] = data6;
						continue;
					}
					case 15:
					{
						Vector4 data5 = Vector4.zero;
						bs.Serialize(ref data5);
						args[i] = data5;
						continue;
					}
					case 16:
					{
						Quaternion data4 = Quaternion.identity;
						bs.Serialize(ref data4);
						args[i] = data4;
						continue;
					}
					case 17:
					{
						ViewId data3 = default(ViewId);
						bs.Serialize(ref data3);
						args[i] = data3;
						continue;
					}
					case 18:
					{
						FVector2 fVector = default(FVector2);
						bs.Serialize(ref fVector.X);
						bs.Serialize(ref fVector.Y);
						args[i] = fVector;
						continue;
					}
					}
					Type value2;
					if (_serializablesIn.TryGetValue(value, out value2))
					{
						ISerializable serializable = (ISerializable)Activator.CreateInstance(value2);
						serializable.Serialize(bs);
						args[i] = serializable;
						continue;
					}
					throw new ArgumentException("Unknown var arg type: " + value);
				}
				return;
			}
			if (args.Length > 255)
			{
				throw new ArgumentException("Too many args!");
			}
			byte data19 = (byte)args.Length;
			bs.Serialize(ref data19);
			for (int j = 0; j < data19; j++)
			{
				object obj = args[j];
				if (obj == null)
				{
					value = 0;
					bs.Serialize(ref value);
					continue;
				}
				if (obj is bool)
				{
					bool data20 = (bool)obj;
					value = 1;
					bs.Serialize(ref value);
					bs.Serialize(ref data20);
					continue;
				}
				if (obj is byte)
				{
					byte data21 = (byte)obj;
					value = 2;
					bs.Serialize(ref value);
					bs.Serialize(ref data21);
					continue;
				}
				if (obj is ushort)
				{
					ushort data22 = (ushort)obj;
					value = 4;
					bs.Serialize(ref value);
					bs.Serialize(ref data22);
					continue;
				}
				if (obj is short)
				{
					short data23 = (short)obj;
					value = 3;
					bs.Serialize(ref value);
					bs.Serialize(ref data23);
					continue;
				}
				if (obj is int)
				{
					int data24 = (int)obj;
					value = 5;
					bs.Serialize(ref value);
					bs.Serialize(ref data24);
					continue;
				}
				if (obj is uint)
				{
					uint data25 = (uint)obj;
					value = 6;
					bs.Serialize(ref value);
					bs.Serialize(ref data25);
					continue;
				}
				if (obj is long)
				{
					long data26 = (long)obj;
					value = 7;
					bs.Serialize(ref value);
					bs.Serialize(ref data26);
					continue;
				}
				if (obj is ulong)
				{
					ulong data27 = (ulong)obj;
					value = 8;
					bs.Serialize(ref value);
					bs.Serialize(ref data27);
					continue;
				}
				if (obj is float)
				{
					float data28 = (float)obj;
					value = 9;
					bs.Serialize(ref value);
					bs.Serialize(ref data28);
					continue;
				}
				if (obj is double)
				{
					double data29 = (double)obj;
					value = 10;
					bs.Serialize(ref value);
					bs.Serialize(ref data29);
					continue;
				}
				if (obj is string)
				{
					string data30 = (string)obj;
					value = 12;
					bs.Serialize(ref value);
					bs.Serialize(ref data30);
					continue;
				}
				if (obj is UnityEngine.Object)
				{
					int data31 = Hash.StringHash(((UnityEngine.Object)obj).name);
					if (GetObjectFromHashId(data31) == null)
					{
						Debug.LogError("Cant serialize object " + ((UnityEngine.Object)obj).name + " Because is hasnt been register");
					}
					value = 11;
					bs.Serialize(ref value);
					bs.Serialize(ref data31);
					continue;
				}
				if (obj is Vector2)
				{
					Vector2 data32 = (Vector2)obj;
					value = 13;
					bs.Serialize(ref value);
					bs.Serialize(ref data32);
					continue;
				}
				if (obj is Vector3)
				{
					Vector3 data33 = (Vector3)obj;
					value = 14;
					bs.Serialize(ref value);
					bs.Serialize(ref data33);
					continue;
				}
				if (obj is Vector4)
				{
					Vector4 data34 = (Vector4)obj;
					value = 15;
					bs.Serialize(ref value);
					bs.Serialize(ref data34);
					continue;
				}
				if (obj is Quaternion)
				{
					Quaternion data35 = (Quaternion)obj;
					value = 16;
					bs.Serialize(ref value);
					bs.Serialize(ref data35);
					continue;
				}
				if (obj is ViewId)
				{
					ViewId data36 = (ViewId)obj;
					value = 17;
					bs.Serialize(ref value);
					bs.Serialize(ref data36);
					continue;
				}
				if (obj is FVector2)
				{
					FVector2 fVector2 = (FVector2)obj;
					value = 18;
					bs.Serialize(ref value);
					bs.Serialize(ref fVector2.X);
					bs.Serialize(ref fVector2.Y);
					continue;
				}
				if (obj is ISerializable)
				{
					ISerializable serializable2 = (ISerializable)obj;
					if (!_serializablesOut.TryGetValue(serializable2.GetType(), out value))
					{
						throw new ArgumentException("Unregister serializable type " + serializable2.GetType());
					}
					bs.Serialize(ref value);
					serializable2.Serialize(bs);
					continue;
				}
				throw new ArgumentException("Unknown var arg type: " + obj.GetType());
			}
		}

		private static UnityEngine.Object GetObjectFromHashId(int hashId)
		{
			UnityEngine.Object value;
			if (_objMap.TryGetValue(hashId, out value))
			{
				return value;
			}
			Debug.LogError("Cant find object for hash " + hashId);
			return null;
		}

		public static void LoadLevel(string name)
		{
			uint localN = _localN;
			_localN += 1000u;
			RPC("0", RPCMode.All, _localP, localN, name);
		}

		public static void Destroy(GameObject go)
		{
			if (go != null)
			{
				View component = go.GetComponent<View>();
				if ((bool)component)
				{
					RPC("6", RPCMode.All, component.viewId.p, component.viewId.n);
				}
				else
				{
					Debug.LogError("Can't call destroy on a object that doesn't have a network view");
				}
			}
		}

		public static UnityEngine.Object Instantiate(UnityEngine.Object prefab, Vector3 position, Quaternion rotation)
		{
			uint localN = _localN;
			_localN += 10u;
			if (GetObjectFromHashId(Hash.StringHash(prefab.name)) == null)
			{
				Debug.LogError("Instantiate: Prefab not registered " + prefab.name);
				return null;
			}
			RPC("2", RPCMode.Others, _localP, localN, prefab, position, rotation);
			return _Instantiate(_localP, localN, prefab, position, rotation);
		}

		public static UnityEngine.Object InstantiateAs(uint connId, UnityEngine.Object prefab, Vector3 position, Quaternion rotation)
		{
			uint localN = _localN;
			_localN += 10u;
			RPC("2", RPCMode.Others, connId, localN, prefab, position, rotation);
			return _Instantiate(connId, localN, prefab, position, rotation);
		}

		private static ViewId AllocateViewId()
		{
			if (_allocations.Count == 0)
			{
				throw new Exception("Failed to allocate view Id!");
			}
			return _allocations.Peek().Allocate();
		}

		private static void Push(uint p, uint n, uint m, bool s)
		{
			_allocations.Push(new AllocationInfo(p, n, m, s));
		}

		private static void Pop()
		{
			_allocations.Pop();
		}

		private static UnityEngine.Object _Instantiate(uint ownerId, uint nextId, UnityEngine.Object prefab, Vector3 position, Quaternion rotation)
		{
			Push(ownerId, nextId, 10u, false);
			UnityEngine.Object result = null;
			if (prefab != null)
			{
				result = UnityEngine.Object.Instantiate(prefab, position, rotation);
			}
			else
			{
				Debug.LogError("Network instantiate prefab is null! ");
			}
			Pop();
			return result;
		}

		private static void _InstantiateRPC(uint ownerId, uint nextId, UnityEngine.Object prefab, Vector3 position, Quaternion rotation)
		{
			_Instantiate(ownerId, nextId, prefab, position, rotation);
		}

		private static void _LoadLevelRPC(uint ownerId, uint nextId, string name)
		{
			Coroutines.Run(_LoadLevelCoroutine(ownerId, nextId, name));
		}

		private static IEnumerator _LoadLevelCoroutine(uint ownerId, uint nextId, string name)
		{
			IsLoadingLevel = true;
			_pauseViewUpdates = true;
			yield return 1;
			Push(ownerId, nextId, 1000u, true);
			yield return Assets.ForceLoadLevel(name);
			yield return new WaitForFixedUpdate();
			yield return new WaitForFixedUpdate();
			_allocations.Peek().ClearSceneFlag();
			RPC("1", RPCMode.All, _localP);
			while (true)
			{
				bool allLoaded = true;
				if (_game != null && !_game.Network.IsLocal)
				{
					foreach (Player player in _game.Players)
					{
						if (!_doneLoading.ContainsKey(player.PlayerId))
						{
							allLoaded = false;
						}
					}
				}
				if (_game == null)
				{
					Debug.LogWarning("Game is null, aborting wait for level load");
					break;
				}
				if (allLoaded)
				{
					break;
				}
				yield return 1;
			}
			yield return new WaitForFixedUpdate();
			_doneLoading.Clear();
			if (IsHost || _game == null)
			{
				RPC("7", RPCMode.All, "OnNetworkLoadedLevel");
			}
			Pop();
			IsLoadingLevel = false;
			_pauseViewUpdates = false;
		}

		private static void _BroadcastMessageRPC(string name)
		{
			UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType(typeof(GameObject));
			for (int i = 0; i < array.Length; i++)
			{
				GameObject gameObject = (GameObject)array[i];
				gameObject.SendMessage(name, SendMessageOptions.DontRequireReceiver);
			}
		}

		private static void _LoadLevelCompleteRPC(uint playerId)
		{
			_doneLoading[playerId] = true;
		}
	}
}
