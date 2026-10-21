using System;
using System.Collections;
using System.Collections.Generic;
using EB.ENet;

namespace EB.Sparx
{
	public class NetworkENet : Network
	{
		private enum PacketMask
		{
			None = 0,
			Broadcast = 128,
			Player = 64
		}

		private enum DisconnectReason
		{
			Normal = 1,
			Now = 2
		}

		private Host _host;

		private Peer _peer;

		private Buffer _sendBuffer;

		private Tracker _out;

		private Tracker _in;

		private DateTime _dT;

		private Hmac _hmac;

		private static bool _initialized;

		public override NetworkStats Stats
		{
			get
			{
				return new NetworkStats
				{
					bandwidthIn = (int)_in.rate,
					bandwidthOut = (int)_out.rate
				};
			}
		}

		public override bool IsLocal
		{
			get
			{
				return false;
			}
		}

		public NetworkENet()
		{
			if (!_initialized)
			{
				_initialized = true;
				Plugin.Initialize();
			}
			_sendBuffer = new Buffer(65536);
			_dT = DateTime.Now;
		}

		public override void Dispose()
		{
			if (_peer != null)
			{
				_peer.Dispose();
				_peer = null;
			}
			if (_host != null)
			{
				_host.Dispose();
				_host = null;
			}
			base.Dispose();
		}

		public override void Update()
		{
			if (_host != null)
			{
				Event netEvent = default(Event);
				int num;
				while (true)
				{
					num = Plugin.SerivceHost(_host, out netEvent, 0u);
					if (num <= 0)
					{
						break;
					}
					switch (netEvent.type)
					{
					case EventType.EVENT_TYPE_CONNECT:
						Debug.Log("Got connect!");
						DispatchConnect();
						break;
					case EventType.EVENT_TYPE_DISCONNECT:
						Debug.Log("Got disconnect!" + netEvent.channelID + " " + netEvent.data);
						DispatchDisconnect(string.Empty);
						break;
					case EventType.EVENT_TYPE_RECEIVE:
					{
						EB.ENet.Packet packet = netEvent.Packet;
						Buffer buffer = new Buffer(packet.Data, false);
						_in.Add(buffer.Capacity);
						if (_hmac == null || buffer.Verify(_hmac))
						{
							byte b = buffer.ReadByte();
							uint playerId = 0u;
							if ((b & 0x40) != 0)
							{
								playerId = buffer.ReadUInt32LE();
							}
							Buffer buffer2 = buffer.Remaining();
							Packet packet2 = new Packet((Channel)netEvent.channelID, buffer2);
							DispatchReceive(playerId, packet2);
						}
						else
						{
							Debug.LogError("Failed to verify packet!!!!!");
						}
						packet.Dispose();
						break;
					}
					}
				}
				if (num < 0)
				{
					DispatchError(Localizer.GetString("ID_SPARX_ERROR_LOST_CONNECT"));
				}
			}
			DateTime now = DateTime.Now;
			float dT = (float)(now - _dT).TotalSeconds;
			_in.Update(dT);
			_out.Update(dT);
			_dT = now;
		}

		public override void Connect(string url, uint connId)
		{
			Debug.Log("Connecting to " + url);
			Uri uri = new Uri(url);
			_host = Plugin.CreateHost(default(Address), 1u, 4u, 0u, 0u);
			Hashtable obj = QueryString.Parse(uri.Query);
			if (Dot.Integer("compressed", obj, 0) == 1)
			{
				Debug.Log("Using compression");
				Plugin.Compress(_host);
			}
			string value = Dot.String("signed", obj, null);
			if (!string.IsNullOrEmpty(value))
			{
				List<byte> list = new List<byte>();
				list.AddRange(Hub.Instance.Config.ApiKey.Value);
				list.AddRange(Encoding.GetBytes(connId.ToString()));
				_hmac = Hmac.MD5(list.ToArray());
				Debug.Log("Using signed packets");
			}
			Address addr = default(Address);
			Plugin.Lookup(uri.Host, out addr);
			addr.port = (ushort)uri.Port;
			_peer = Plugin.Connect(_host, addr, 4u, connId);
		}

		public override void Disconnect(bool now)
		{
			if (_peer != null)
			{
				if (now)
				{
					Debug.Log("Disconnect NOW!");
					Plugin.PeerDisconnectNow(_peer, 2u);
					DispatchDisconnect(string.Empty);
				}
				else
				{
					Plugin.PeerDisconnect(_peer, 1u);
				}
			}
		}

		public override void SendTo(uint playerId, Packet packet)
		{
			_sendBuffer.Reset();
			byte b = 0;
			if (playerId != 0)
			{
				b |= 0x40;
				_sendBuffer.WriteByte(b);
				_sendBuffer.WriteUInt32LE(playerId);
			}
			else
			{
				_sendBuffer.WriteByte(b);
			}
			_sendBuffer.WriteBuffer(packet.Data);
			if (_hmac != null)
			{
				_sendBuffer.Sign(_hmac);
			}
			ArraySegment<byte> arraySegment = _sendBuffer.ToArraySegment(false);
			PacketFlag flags = PacketFlag.PACKET_FLAG_NONE;
			if (packet.Channel != Channel.Game_UnReliable)
			{
				flags = PacketFlag.PACKET_FLAG_RELIABLE;
			}
			_out.Add(arraySegment.Count);
			EB.ENet.Packet packet2 = Plugin.CreatePacket(arraySegment.Array, arraySegment.Count, flags);
			Plugin.Send(_peer, (int)packet.Channel, packet2);
			packet2.Dispose();
		}

		public override void Broadcast(Packet packet)
		{
			_sendBuffer.Reset();
			byte value = 128;
			_sendBuffer.WriteByte(value);
			_sendBuffer.WriteBuffer(packet.Data);
			if (_hmac != null)
			{
				_sendBuffer.Sign(_hmac);
			}
			ArraySegment<byte> arraySegment = _sendBuffer.ToArraySegment(false);
			PacketFlag flags = PacketFlag.PACKET_FLAG_NONE;
			if (packet.Channel != Channel.Game_UnReliable)
			{
				flags = PacketFlag.PACKET_FLAG_RELIABLE;
			}
			_out.Add(arraySegment.Count);
			EB.ENet.Packet packet2 = Plugin.CreatePacket(arraySegment.Array, arraySegment.Count, flags);
			Plugin.Send(_peer, (int)packet.Channel, packet2);
			packet2.Dispose();
		}
	}
}
