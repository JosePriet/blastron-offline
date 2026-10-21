using System;
using System.Runtime.InteropServices;

namespace EB.ENet
{
	public class Host : IDisposable
	{
		public IntPtr Handle { get; private set; }

		public int Socket
		{
			get
			{
				return Marshal.ReadInt32(Handle, 0);
			}
		}

		public uint BindIp
		{
			get
			{
				return (uint)Marshal.ReadInt32(Handle, 4);
			}
		}

		public ushort BindPort
		{
			get
			{
				return (ushort)Marshal.ReadInt16(Handle, 8);
			}
		}

		public uint IncomingBandwidth
		{
			get
			{
				return (uint)Marshal.ReadInt32(Handle, 12);
			}
		}

		public uint OutgoingBandwidth
		{
			get
			{
				return (uint)Marshal.ReadInt32(Handle, 16);
			}
		}

		public uint BandwidthThrottleEpoch
		{
			get
			{
				return (uint)Marshal.ReadInt32(Handle, 20);
			}
		}

		public uint Mtu
		{
			get
			{
				return (uint)Marshal.ReadInt32(Handle, 24);
			}
		}

		public uint RandomSeed
		{
			get
			{
				return (uint)Marshal.ReadInt32(Handle, 28);
			}
		}

		public Host(IntPtr handle)
		{
			Handle = handle;
		}

		public void Clear()
		{
			Handle = IntPtr.Zero;
		}

		public void Dispose()
		{
			if (Handle != IntPtr.Zero)
			{
				Plugin.HostDestroy(this);
				Handle = IntPtr.Zero;
			}
		}
	}
}
