using System;
using EB.Net;

namespace EB.Sparx
{
	public class NetworkWS : Network
	{
		private enum PacketMask
		{
			None = 0,
			Broadcast = 128,
			Player = 64,
			Channel = 15
		}

		private WebSocket _socket;

		private Deferred _deferred;

		private Buffer _sendBuffer;

		public override NetworkStats Stats
		{
			get
			{
				return default(NetworkStats);
			}
		}

		public override bool IsLocal
		{
			get
			{
				return false;
			}
		}

		public NetworkWS()
		{
			_deferred = new Deferred(4);
			_sendBuffer = new Buffer(65536);
		}

		public override void Dispose()
		{
			_deferred.Dispose();
			if (_socket != null)
			{
				_socket.Dispose();
			}
			base.Dispose();
		}

		public override void Update()
		{
			_deferred.Dispatch();
		}

		public override void Connect(string url, uint connId)
		{
			Debug.Log("Connecting to " + url);
			byte[] bytes = BitConverter.GetBytes(connId);
			_socket = new WebSocket();
			_socket.OnData += OnSocketReceive;
			_socket.OnConnect += OnSocketConnect;
			_socket.OnError += OnSocketError;
			_socket.ConnectAsync(new Uri(url), "io.sparx.game", bytes);
		}

		public override void Disconnect(bool now)
		{
			if (_socket != null)
			{
				_socket.Dispose();
				_socket = null;
				DispatchDisconnect(string.Empty);
			}
		}

		public override void SendTo(uint playerId, Packet packet)
		{
			_sendBuffer.Reset();
			byte b = (byte)((Channel)0xF & packet.Channel);
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
			_socket.SendBinary(_sendBuffer);
		}

		public override void Broadcast(Packet packet)
		{
			_sendBuffer.Reset();
			byte b = (byte)((Channel)0xF & packet.Channel);
			b |= 0x80;
			_sendBuffer.WriteByte(b);
			_sendBuffer.WriteBuffer(packet.Data);
			_socket.SendBinary(_sendBuffer);
		}

		private void OnSocketError(string error)
		{
			Debug.LogError("Socket error!: " + error);
			_deferred.Defer(new Action<string>(base.DispatchError), error);
		}

		private void OnSocketConnect()
		{
			Debug.Log("NetworkWS: Connected");
			_deferred.Defer(new Action(base.DispatchConnect));
		}

		private void OnSocketReceive(byte[] data)
		{
			try
			{
				Buffer buffer = new Buffer(data, false);
				byte b = buffer.ReadByte();
				byte channel = (byte)(b & 0xF);
				uint num = 0u;
				if ((b & 0x40) != 0)
				{
					num = buffer.ReadUInt32LE();
				}
				Buffer buffer2 = buffer.Remaining();
				Packet packet = new Packet((Channel)channel, buffer2);
				_deferred.Defer(new Action<uint, Packet>(base.DispatchReceive), num, packet);
			}
			catch (Exception ex)
			{
				_deferred.Defer(new Action<string>(base.DispatchError), "error occured decoding packet : " + ex.ToString());
			}
		}
	}
}
