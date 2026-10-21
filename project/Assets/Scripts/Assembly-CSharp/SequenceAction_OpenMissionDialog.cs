using EB;
using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Actions/Open Mission Dialog")]
public class SequenceAction_OpenMissionDialog : EB.Sequence.Runtime.Action
{
	[Property]
	public string popupId;

	[Property]
	public string buttonText;

	[Property]
	public string headTexture;

	[Property]
	public Vector3 headScale;

	[Property]
	public string faceTexture;

	[Property]
	public Vector3 faceScale;

	[Property]
	public Vector3 faceOffset;

	[Property]
	public GameObject prefab;

	[Property]
	public string uiTexture;

	[Trigger]
	public Trigger Finished = new Trigger();

	[Entry]
	public void Process()
	{
		EB.Debug.LogWarning("Opening mission dialog!");
		SinglePlayerPrefab singlePlayerPrefab = CampaignData.Instance.CurrentMissionPrefab();
		string empty = string.Empty;
		string description = singlePlayerPrefab.missionDialogDesc;
		GameObject gameObject = ScreenManager.Instance.OpenPopup("TutorialDialogUI");
		TutorialDialogUI component = gameObject.GetComponent<TutorialDialogUI>();
		if (string.IsNullOrEmpty(buttonText))
		{
			buttonText = "ID_UI_RESULTS_NEXT";
		}
		int highScore = CampaignData.Instance.GetHighScore(singlePlayerPrefab);
		if (CampaignData.Instance.IsCampaignPlus() && singlePlayerPrefab.GameMode != GameAttributes.GameModeType.Tutorial)
		{
			description = Localizer.Format("ID_GAME_MESSAGE_HS_MSG", highScore);
			buttonText = "ID_UI_COMMON_OK";
		}
		Material material = null;
		if (prefab != null)
		{
			GameObject objectExactMatch = Util.GetObjectExactMatch(prefab, "Shape");
			if (objectExactMatch != null && objectExactMatch.renderer != null)
			{
				material = objectExactMatch.renderer.material;
			}
		}
		component.SetParameters(popupId, description, empty, buttonText, uiTexture, headTexture, headScale, faceTexture, faceScale, faceOffset, material);
		Finished.Invoke();
	}
}
