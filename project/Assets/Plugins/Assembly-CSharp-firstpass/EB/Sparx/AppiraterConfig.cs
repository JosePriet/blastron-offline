namespace EB.Sparx
{
	public class AppiraterConfig
	{
		public string AppId = string.Empty;

		public double DaysUntilPrompt = 1.0;

		public int UsesUntilPrompt = 10;

		public int SignificantEventsUntilPrompt = 3;

		public double TimeBeforeReminding = 1.0;

		public bool CanPromptOnLaunched;

		public bool CanPromptOnUse;

		public bool CanPromptOnEvent;

		public string Title = string.Empty;

		public string Message = string.Empty;

		public string RateButton = string.Empty;

		public string CancelButton = string.Empty;

		public string RemindButton = string.Empty;

		public AppiraterListener Listener;
	}
}
