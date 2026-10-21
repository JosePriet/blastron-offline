public static class DebugSettings
{
	public static bool LevelDestruction { get; set; }

	public static bool SlideJumpLeft { get; set; }

	public static bool JoystickLeft { get; set; }

	public static bool FireOnEndAim { get; set; }

	public static bool LevelDataTimedEvents { get; set; }

	public static bool FingerBools { get; set; }

	public static bool ExplosionsOn { get; set; }

	public static bool MuzzleFlashOn { get; set; }

	public static bool FSDebugOn { get; set; }

	public static bool DisplayConcussionDebug { get; set; }

	public static bool FasterIdling { get; set; }

	public static bool UnlockAllWeapons { get; set; }

	public static bool QuickGame { get; set; }

	public static bool TurboGame { get; set; }

	public static bool ForceTutorial { get; set; }

	public static bool ForceRevengeSaleRoll { get; set; }

	public static bool PretendSlowDevice { get; set; }

	public static void Init()
	{
		LevelDestruction = true;
		SlideJumpLeft = true;
		FireOnEndAim = true;
		LevelDataTimedEvents = false;
		JoystickLeft = true;
		FingerBools = false;
		ExplosionsOn = true;
		MuzzleFlashOn = true;
		FSDebugOn = false;
		LobbyUI.fiddleMode = false;
		DisplayConcussionDebug = true;
		FasterIdling = false;
		UnlockAllWeapons = false;
		QuickGame = false;
		TurboGame = false;
		ForceTutorial = false;
		PretendSlowDevice = false;
	}
}
