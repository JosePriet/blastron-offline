namespace EB.TestFlight
{
	public static class Plugin
	{
		private static void TestFlight_TakeOff(string apiToken)
		{
		}

		private static void TestFlight_SetUDID()
		{
		}

		private static void TestFlight_Log(string l)
		{
		}

		private static void TestFlight_Checkpoint(string l)
		{
		}

		public static void TakeOff(string apiToken)
		{
			TestFlight_TakeOff(apiToken);
		}

		public static void SetUDID()
		{
			TestFlight_SetUDID();
		}

		public static void Log(string message)
		{
			TestFlight_Log(message);
		}

		public static void Checkpoint(string message)
		{
			TestFlight_Checkpoint(message);
		}
	}
}
