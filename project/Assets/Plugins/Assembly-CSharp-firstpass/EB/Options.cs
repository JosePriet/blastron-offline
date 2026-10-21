namespace EB
{
	public static class Options
	{
		public static int sound
		{
			get
			{
				return GetOption("sound", 1);
			}
			set
			{
				SetOption("sound", value);
			}
		}

		public static event Action SettingsChanged;

		public static int GetOption(string name, int def)
		{
			return SecurePrefs.GetInt(name, def);
		}

		public static void SetOption(string name, int value)
		{
			SecurePrefs.SetInt(name, value);
			SecurePrefs.Save();
			if (SettingsChanged != null)
			{
				SettingsChanged();
			}
		}
	}
}
