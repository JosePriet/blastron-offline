namespace EB.Replication
{
	public interface ISerializable
	{
		void Serialize(BitStream bs);
	}
}
