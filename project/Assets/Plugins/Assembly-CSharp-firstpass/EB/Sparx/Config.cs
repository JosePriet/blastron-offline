using System;
using System.Collections.Generic;

namespace EB.Sparx
{
	public class Config
	{
		public string ApiEndpoint = string.Empty;

		public Key ApiKey;

		public int ApiKeepAlive = 60;

		public Language Locale = Language.English;

		public bool LoadLocalizer = true;

		public bool BackgroundOnPause = true;

		public bool UsePush = true;

		public LoginConfig LoginConfig = new LoginConfig();

		public GameManagerConfig GameManagerConfig = new GameManagerConfig();

		public InventoryConfig InventoryConfig = new InventoryConfig();

		public WalletConfig WalletConfig = new WalletConfig();

		public GameCenterConfig GameCenterConfig = new GameCenterConfig();

		public FacebookConfig FacebookConfig = new FacebookConfig();

		public AppiraterConfig AppiraterConfig = new AppiraterConfig();

		public GachaConfig GachaConfig = new GachaConfig();

		public Type HubType = typeof(SparxHub);

		public List<Type> GameComponents = new List<Type>();
	}
}
