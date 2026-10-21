using System;
using System.Net;
using System.Runtime.InteropServices;

namespace EB.ENet
{
	public static class Plugin
	{
		private const string DLL_NAME = "enetlib";

		[DllImport("enetlib")]
		private static extern int enet_initialize();

		[DllImport("enetlib")]
		private static extern void enet_deinitialize();

		[DllImport("enetlib")]
		private static extern uint enet_time_get();

		[DllImport("enetlib")]
		private static extern IntPtr enet_host_create([MarshalAs(UnmanagedType.LPStruct)] Address address, uint peerCount, uint channelLimit, uint incomingBandwidth, uint outgoingBandwidth);

		[DllImport("enetlib")]
		private static extern int enet_host_service(IntPtr host, IntPtr netevent, uint timeout);

		[DllImport("enetlib")]
		private static extern int enet_address_set_host(IntPtr address, [MarshalAs(UnmanagedType.LPStr)] string hostname);

		[DllImport("enetlib")]
		private static extern IntPtr enet_host_connect(IntPtr host, [MarshalAs(UnmanagedType.LPStruct)] Address address, uint channelCount, uint data);

		[DllImport("enetlib")]
		private static extern int enet_host_compress_with_range_coder(IntPtr host);

		[DllImport("enetlib")]
		private static extern IntPtr enet_packet_create(byte[] data, int dataLength, uint flags);

		[DllImport("enetlib")]
		private static extern void enet_packet_destroy(IntPtr packet);

		[DllImport("enetlib")]
		private static extern void enet_peer_disconnect_now(IntPtr peer, uint data);

		[DllImport("enetlib")]
		private static extern void enet_peer_disconnect(IntPtr peer, uint data);

		[DllImport("enetlib")]
		private static extern int enet_peer_send(IntPtr peer, byte channel, IntPtr packet);

		[DllImport("enetlib")]
		private static extern void enet_host_destroy(IntPtr host);

		public static int Initialize()
		{
			return enet_initialize();
		}

		public static void Shutdown()
		{
			enet_deinitialize();
		}

		public static uint GetTime()
		{
			return enet_time_get();
		}

		private static IntPtr ToPtr<T>(T obj)
		{
			IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(T)));
			Marshal.StructureToPtr(obj, intPtr, false);
			return intPtr;
		}

		private static void Free(IntPtr ptr)
		{
			if (ptr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(ptr);
			}
		}

		public static int Lookup(string hostname, out Address addr)
		{
			IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Address)));
			int result = enet_address_set_host(intPtr, hostname);
			addr = (Address)Marshal.PtrToStructure(intPtr, typeof(Address));
			Marshal.FreeHGlobal(intPtr);
			Debug.Log("Lookup: " + hostname + " = " + new IPAddress(addr.host).ToString());
			return result;
		}

		public static Host CreateHost(Address addr, uint peerCount, uint channelLimit, uint incomingBandwidth, uint outgoingBandwidth)
		{
			IntPtr intPtr = enet_host_create(addr, peerCount, channelLimit, incomingBandwidth, outgoingBandwidth);
			if (intPtr != IntPtr.Zero)
			{
				return new Host(intPtr);
			}
			return null;
		}

		public static int Compress(Host host)
		{
			return enet_host_compress_with_range_coder(host.Handle);
		}

		public static Peer Connect(Host host, Address addr, uint channelCount, uint data)
		{
			IntPtr intPtr = enet_host_connect(host.Handle, addr, channelCount, data);
			if (intPtr != IntPtr.Zero)
			{
				return new Peer(intPtr);
			}
			return null;
		}

		public static int SerivceHost(Host host, out Event netEvent, uint timeout)
		{
			netEvent = default(Event);
			if (host.Handle == IntPtr.Zero)
			{
				return 0;
			}
			IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Event)));
			int result = enet_host_service(host.Handle, intPtr, timeout);
			Event obj = (Event)Marshal.PtrToStructure(intPtr, typeof(Event));
			Marshal.FreeHGlobal(intPtr);
			netEvent = obj;
			return result;
		}

		public static Packet CreatePacket(byte[] data, int length, PacketFlag flags)
		{
			IntPtr intPtr = enet_packet_create(data, length, (uint)flags);
			if (intPtr != IntPtr.Zero)
			{
				return new Packet(intPtr);
			}
			return null;
		}

		public static void DestroyPacket(Packet packet)
		{
			if (packet != null)
			{
				IntPtr handle = packet.Handle;
				packet.Reset();
				if (handle != IntPtr.Zero)
				{
					enet_packet_destroy(handle);
				}
			}
		}

		public static int Send(Peer peer, int channel, Packet packet)
		{
			if (peer == null || peer.Handle == IntPtr.Zero)
			{
				return -1;
			}
			packet.MarkSent();
			return enet_peer_send(peer.Handle, (byte)channel, packet.Handle);
		}

		public static void PeerDisconnectNow(Peer peer, uint data)
		{
			if (peer != null && !(peer.Handle == IntPtr.Zero))
			{
				enet_peer_disconnect_now(peer.Handle, data);
				peer.Clear();
			}
		}

		public static void PeerDisconnect(Peer peer, uint data)
		{
			if (peer != null && !(peer.Handle == IntPtr.Zero))
			{
				enet_peer_disconnect(peer.Handle, data);
				peer.Clear();
			}
		}

		public static void HostDestroy(Host host)
		{
			if (host != null && !(host.Handle == IntPtr.Zero))
			{
				enet_host_destroy(host.Handle);
				host.Clear();
			}
		}
	}
}
