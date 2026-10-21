using System.Runtime.InteropServices;

namespace FarseerPhysics.Collision
{
	[StructLayout((LayoutKind)2)]
	public struct ContactID
	{
		[FieldOffset(0)]
		public ContactFeature Features;

		[FieldOffset(0)]
		public uint Key;
	}
}
