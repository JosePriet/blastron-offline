using UnityEngine;

public class ResetButton : MonoBehaviour
{
	private void Awake()
	{
		NGUITools.SetActive(base.gameObject, GameAttributes.IsAuxFlagSet(GameAttributes.AuxFlagsType.kResetable));
	}

	private void OnResetLevel()
	{
		LevelData.Instance.PauseGame = false;
		ScreenManager.Instance.FadeOut(() =>
		{
			ScreenManager.Instance.ClearPersistentScreens();
			CampaignData.Instance.Missions[CampaignData.Instance.CurrentMission].Play();
		});
	}
}
