using EB;
using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Actions/Open First Boot Panel")]
public class SequenceAction_OpenFirstBootPanel : EB.Sequence.Runtime.Action
{
	[Property(Hint = "LocId")]
	public string Description;

	[Property(Hint = "LocId")]
	public string buttonText;

	[Property]
	public string headTexture;

	[Property]
	public string faceTexture;

	[Property]
	public bool showOk;

	[Property]
	public bool showFader;

	[Property]
	public bool showPreviousFader;

	[Property]
	public Vector3 position;

	[Property(MapTo = typeof(EBUI_Transition.TransitionTypes))]
	public int introTransition;

	[Property(MapTo = typeof(EBUI_Transition.TransitionTypes))]
	public int outroTransition;

	[Trigger]
	public Trigger Finished = new Trigger();

	[Entry]
	public void Process()
	{
		GameObject obj = ScreenManager.Instance.OpenPopup("FirstBootPanelUI", true, showFader, showPreviousFader);
		FirstBootPanelUI firstBootPanelUI = Util.FindComponent<FirstBootPanelUI>(obj);
		firstBootPanelUI.SetParameters(Description, buttonText, headTexture, faceTexture, showOk, position, (EBUI_Transition.TransitionTypes)introTransition, (EBUI_Transition.TransitionTypes)outroTransition);
		firstBootPanelUI.callback = OnConfirm;
		firstBootPanelUI.BroadcastMessage("DoOpenPopup");
	}

	private void OnConfirm()
	{
		Finished.Invoke();
	}
}
