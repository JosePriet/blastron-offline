namespace EB.IAP.Internal
{
	internal class ProviderFactory
	{
		public static Provider Create(Config config)
		{
			return (Provider)System.Activator.CreateInstance(System.Type.GetType("EB.Sparx.LocalProvider, Assembly-CSharp", true), config);
		}
	}
}
