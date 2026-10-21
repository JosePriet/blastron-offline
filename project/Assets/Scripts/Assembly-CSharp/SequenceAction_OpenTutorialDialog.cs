using EB;
using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Actions/OpenTutorialDialog")]
public class SequenceAction_OpenTutorialDialog : EB.Sequence.Runtime.Action
{
	[Property]
	public string TutorialId;

	[Property(Hint = "LocId")]
	public string Title;

	[Property(Hint = "LocId")]
	public string Description;

	[Property(Hint = "LocId")]
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
		GameObject gameObject = ScreenManager.Instance.OpenPopup("TutorialDialogUI");
		TutorialDialogUI component = gameObject.GetComponent<TutorialDialogUI>();
		if (string.IsNullOrEmpty(buttonText))
		{
			buttonText = "ID_UI_RESULTS_NEXT";
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
		component.SetParameters(TutorialId, Description, Title, buttonText, uiTexture, headTexture, headScale, faceTexture, faceScale, faceOffset, material);
		Finished.Invoke();
	}
}
