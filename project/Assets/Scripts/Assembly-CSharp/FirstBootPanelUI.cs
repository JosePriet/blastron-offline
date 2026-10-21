using EB;
using EB.Sequence;
using SmoothMoves;
using UnityEngine;

public class FirstBootPanelUI : PopupScript
{
	public Action callback;

	public TextureAtlas headAtlas;

	public TextureAtlas faceAtlas;

	private TextureAtlas loadedFaceAtlas;

	protected override void OnAwake()
	{
		CharacterDB.MetaData defaultPart = CharacterDB.GetDefaultPart(0);
		GameObject gameObject = Assets.Load(defaultPart.assetPrefabPath) as GameObject;
		CharacterData component = gameObject.GetComponent<CharacterData>();
		if (component != null)
		{
			loadedFaceAtlas = component.FaceAtlas;
		}
		base.OnAwake();
	}

	public void SetParameters(string description, string ButtonText, string headTexture, string faceTexture, bool showOk, Vector3 position, EBUI_Transition.TransitionTypes intro, EBUI_Transition.TransitionTypes outro)
	{
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "Panel");
		if (position != Vector3.zero)
		{
			objectExactMatch.transform.localPosition = position;
		}
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "PopupTransition");
		EBUI_TransitionInfo component = objectExactMatch2.GetComponent<EBUI_TransitionInfo>();
		if (component != null)
		{
			component.transition_intro.transitionType = intro;
			component.transition_outro.transitionType = outro;
		}
		GameObject objectExactMatch3 = Util.GetObjectExactMatch(base.gameObject, "ButtonOK");
		UIUtils.SetLabelContents(base.gameObject, "LabelDescription", description);
		if (showOk)
		{
			NGUITools.SetActive(objectExactMatch3, true);
			UIUtils.SetLabelContents(objectExactMatch3, "LabelOk", ButtonText);
			Util.FindComponent<ButtonScript>(objectExactMatch3).onButtonClick += OnOk;
		}
		else
		{
			NGUITools.SetActive(objectExactMatch3, false);
			if (objectExactMatch != null)
			{
				BoxCollider component2 = objectExactMatch.GetComponent<BoxCollider>();
				if (component2 != null)
				{
					component2.enabled = false;
				}
			}
		}
		if (!string.IsNullOrEmpty(headTexture))
		{
			Material material = new Material(headAtlas.material);
			material.shader = Shader.Find("Unlit/Transparent Colored");
			UITexture component3 = Util.GetObjectExactMatch(base.gameObject, "Texture").GetComponent<UITexture>();
			component3.material = material;
			string textureGUIDFromName = headAtlas.GetTextureGUIDFromName(headTexture);
			int textureIndex = headAtlas.GetTextureIndex(textureGUIDFromName);
			Rect uvRect = headAtlas.uvs[textureIndex];
			component3.uvRect = uvRect;
		}
		if (!string.IsNullOrEmpty(faceTexture))
		{
			Material material2 = new Material(loadedFaceAtlas.material);
			material2.shader = Shader.Find("Unlit/Transparent Colored");
			UITexture component4 = Util.GetObjectExactMatch(base.gameObject, "Texture2").GetComponent<UITexture>();
			component4.material = material2;
			string textureGUIDFromName2 = loadedFaceAtlas.GetTextureGUIDFromName(faceTexture);
			int textureIndex2 = loadedFaceAtlas.GetTextureIndex(textureGUIDFromName2);
			Rect uvRect2 = loadedFaceAtlas.uvs[textureIndex2];
			component4.uvRect = uvRect2;
		}
	}

	private void OnOk(GameObject caller)
	{
		OnClose();
	}

	protected override void HandleOpenPopup()
	{
	}

	private void DoOpenPopup()
	{
		EBUI_TransitionManager.Instance.RunIntroTransitions(base.gameObject, SafeAction.Wrap(this, OnTransitionInComplete));
	}

	protected override void OnTransitionInComplete()
	{
		base.OnTransitionInComplete();
		EB.Sequence.Component.Activate(base.gameObject, "FirstBootPanelUI", typeof(SequenceEvent_OnScreenOpened));
	}

	protected override void OnTransitionOutComplete()
	{
		base.OnTransitionOutComplete();
		if (callback != null)
		{
			callback();
		}
		else if ((bool)parentScreen)
		{
			parentScreen.SendMessage("OnConfirmationDialogUIClosed", SendMessageOptions.RequireReceiver);
		}
		EB.Sequence.Component.Activate(base.gameObject, "FirstBootPanelUI", typeof(SequenceEvent_OnScreenClosed));
	}
}
