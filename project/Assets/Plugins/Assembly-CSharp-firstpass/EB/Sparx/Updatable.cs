namespace EB.Sparx
{
	public interface Updatable
	{
		bool UpdateOffline { get; }

		void Update();
	}
}
