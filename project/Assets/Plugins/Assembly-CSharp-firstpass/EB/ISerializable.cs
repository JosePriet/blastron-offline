namespace EB
{
	public interface ISerializable
	{
		object Serialize();

		void Deserialize(object src);
	}
}
