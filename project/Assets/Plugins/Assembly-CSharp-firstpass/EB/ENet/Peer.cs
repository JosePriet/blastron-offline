using System;
using System.Net;
using System.Runtime.InteropServices;

namespace EB.ENet
{
	public class Peer : IDisposable
	{
		public IntPtr Handle { get; private set; }

		public uint PeerIp
		{
			get
			{
				return (uint)Marshal.ReadInt32(Handle, 24);
			}
		}

		public ushort PeerPort
		{
			get
			{
				return (ushort)Marshal.ReadInt16(Handle, 28);
			}
		}

		public Peer(IntPtr handle)
		{
			Handle = handle;
			Debug.Log("New Peer: " + handle.ToInt32());
			Debug.Log("PeerIp: " + new IPAddress(PeerIp));
			Debug.Log("PeerPort: " + PeerPort);
		}

		public void Clear()
		{
			Handle = IntPtr.Zero;
		}

		public void Dispose()
		{
			if (Handle != IntPtr.Zero)
			{
				Plugin.PeerDisconnectNow(this, 0u);
				Handle = IntPtr.Zero;
			}
		}
	}
}
