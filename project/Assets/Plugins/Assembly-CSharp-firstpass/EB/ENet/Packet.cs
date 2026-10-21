using System;
using System.Runtime.InteropServices;

namespace EB.ENet
{
	public class Packet : IDisposable
	{
		private bool isSent;

		public IntPtr Handle { get; private set; }

		public int RefCount
		{
			get
			{
				return Marshal.ReadInt32(Handle, 0);
			}
		}

		public uint Flags
		{
			get
			{
				return (uint)Marshal.ReadInt32(Handle, 4);
			}
		}

		public byte[] Data
		{
			get
			{
				IntPtr source = Marshal.ReadIntPtr(Handle, 8);
				int num = Marshal.ReadInt32(Handle, 12);
				byte[] array = new byte[num];
				Marshal.Copy(source, array, 0, num);
				return array;
			}
		}

		public Packet(IntPtr handle)
		{
			Handle = handle;
		}

		public void Dispose()
		{
			if (!isSent && Handle != IntPtr.Zero)
			{
				Plugin.DestroyPacket(this);
			}
		}

		public void Reset()
		{
			Handle = IntPtr.Zero;
			isSent = false;
		}

		public void MarkSent()
		{
			isSent = true;
		}
	}
}
