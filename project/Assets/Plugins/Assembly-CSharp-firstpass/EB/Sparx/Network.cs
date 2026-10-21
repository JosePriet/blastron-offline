using System;

namespace EB.Sparx
{
	public abstract class Network : IDisposable
	{
		public const uint HostId = 0u;

		public const uint BroadcastId = uint.MaxValue;

		public const uint NpcId = 10u;

		public const uint NpcIdLast = 20u;

		public abstract NetworkStats Stats { get; }

		public abstract bool IsLocal { get; }

		public event Action OnConnect;

		public event Action<string> OnDisconnect;

		public event Action<string> OnError;

		public event Action<uint, Packet> OnReceive;

		public abstract void Connect(string url, uint connId);

		public abstract void Disconnect(bool now);

		public abstract void SendTo(uint playerId, Packet packet);

		public abstract void Broadcast(Packet packet);

		public abstract void Update();

		public virtual void Dispose()
		{
			OnConnect = null;
			OnDisconnect = null;
			OnError = null;
			OnReceive = null;
		}

		protected void DispatchConnect()
		{
			if (OnConnect != null)
			{
				OnConnect();
			}
		}

		protected void DispatchDisconnect(string reason)
		{
			if (OnDisconnect != null)
			{
				OnDisconnect(reason);
			}
		}

		protected void DispatchError(string error)
		{
			if (OnError != null)
			{
				OnError(error);
			}
		}

		protected void DispatchReceive(uint playerId, Packet packet)
		{
			if (OnReceive != null)
			{
				OnReceive(playerId, packet);
			}
		}
	}
}
