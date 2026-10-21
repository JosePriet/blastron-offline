using System;

namespace EB.ENet
{
	public struct Event
	{
		public EventType type;

		public IntPtr peer;

		public byte channelID;

		public uint data;

		public IntPtr packet;

		public Packet Packet
		{
			get
			{
				if (packet != IntPtr.Zero)
				{
					return new Packet(packet);
				}
				return null;
			}
		}
	}
}
