using EB;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

public static class SerializationUtils
{
	public static void WriteFVector2(BitStream stream, FVector2 v)
	{
		stream.Serialize(ref v.X);
		stream.Serialize(ref v.Y);
	}

	public static FVector2 ReadFVector2(BitStream stream)
	{
		FVector2 result = default(FVector2);
		stream.Serialize(ref result.X);
		stream.Serialize(ref result.Y);
		return result;
	}

	public static void SerializeBody(BitStream stream, Body body)
	{
		if (body != null)
		{
			if (stream.isWriting)
			{
				WriteFVector2(stream, body.Position);
				WriteFVector2(stream, body.LinearVelocity);
				float data = body.Rotation;
				stream.Serialize(ref data);
			}
			else
			{
				body.Position = ReadFVector2(stream);
				body.LinearVelocity = ReadFVector2(stream);
				float data2 = 0f;
				stream.Serialize(ref data2);
				body.Rotation = data2;
			}
		}
	}
}
