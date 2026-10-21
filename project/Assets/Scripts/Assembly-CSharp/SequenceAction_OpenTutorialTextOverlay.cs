using EB;
using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Actions/OpenTutorialTextOverlay")]
public class SequenceAction_OpenTutorialTextOverlay : EB.Sequence.Runtime.Action
{
	[Property]
	public string TutorialId = string.Empty;

	[Property(Hint = "LocId")]
	public string Title = string.Empty;

	[Property(Hint = "LocId")]
	public string Description = string.Empty;

	[Property]
	public string headTexture = string.Empty;

	[Property]
	public Vector3 headScale;

	[Property]
	public string faceTexture = string.Empty;

	[Property]
	public Vector3 faceScale;

	[Property]
	public Vector3 faceOffset;

	[Property]
	public GameObject prefab;

	[Property]
	public string uiTexture = string.Empty;

	[Trigger]
	public Trigger Finished = new Trigger();

	[Entry]
	public void Process()
	{
		GameObject gameObject = ScreenManager.Instance.FindPopup("TutorialTextOverlayUI");
		TutorialTextOverlayUI component = gameObject.GetComponent<TutorialTextOverlayUI>();
		Material material = null;
		if (prefab != null)
		{
			GameObject objectExactMatch = Util.GetObjectExactMatch(prefab, "Shape");
			if (objectExactMatch != null && objectExactMatch.renderer != null)
			{
				material = objectExactMatch.renderer.material;
			}
		}
		component.SetParameters(TutorialId, Description, Title, uiTexture, headTexture, headScale, faceTexture, faceScale, faceOffset, material);
		gameObject.SendMessage("OnOpenPopup");
		Finished.Invoke();
	}
}
