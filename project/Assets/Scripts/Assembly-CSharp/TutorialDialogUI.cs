using EB;
using EB.Sequence;
using SmoothMoves;
using UnityEngine;

public class TutorialDialogUI : PopupScript
{
	public string _tutorialId;

	public TextureAtlas headAtlas;

	public TextureAtlas faceAtlas;

	private TextureAtlas loadedFaceAtlas;

	private Material material;

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

	protected override void OnStart()
	{
		base.OnStart();
		GameHUD.Instance.HideElementsForTutorialPopup(true);
	}

	protected override void OnTransitionOutComplete()
	{
		base.OnTransitionOutComplete();
		EB.Sequence.Component.Activate(base.gameObject, _tutorialId, typeof(SequenceEvent_OnTutorialDialogClosed));
		GameHUD.Instance.HideElementsForTutorialPopup(false);
	}

	public void SetParameters(string tutorialId, string description, string title, string buttonText, string uiTexturePath, string headTexture, Vector3 headScale, string faceTexture, Vector3 faceScale, Vector3 faceOffset, Material material)
	{
		_tutorialId = tutorialId;
		UIUtils.SetLabelContents(base.gameObject, "LabelPopupTitle", title);
		UIUtils.SetLabelContents(base.gameObject, "LabelDescription", description);
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "CloseButton");
		UIUtils.SetLabelContents(objectExactMatch, "Label", buttonText);
		if (!string.IsNullOrEmpty(uiTexturePath))
		{
			TexturePoolManager.Instance.LoadTexture(uiTexturePath, OnTextureLoaded);
		}
		if (!string.IsNullOrEmpty(headTexture))
		{
			Material material2 = new Material(headAtlas.material);
			material2.shader = Shader.Find("Unlit/Transparent Colored");
			UITexture component = Util.GetObjectExactMatch(base.gameObject, "Texture").GetComponent<UITexture>();
			component.material = material2;
			string textureGUIDFromName = headAtlas.GetTextureGUIDFromName(headTexture);
			int textureIndex = headAtlas.GetTextureIndex(textureGUIDFromName);
			Rect uvRect = headAtlas.uvs[textureIndex];
			component.uvRect = uvRect;
			if (headScale != Vector3.zero)
			{
				component.transform.localScale = headScale;
			}
		}
		if (!string.IsNullOrEmpty(faceTexture))
		{
			Material material3 = new Material(loadedFaceAtlas.material);
			material3.shader = Shader.Find("Unlit/Transparent Colored");
			UITexture component2 = Util.GetObjectExactMatch(base.gameObject, "Texture2").GetComponent<UITexture>();
			component2.material = material3;
			string textureGUIDFromName2 = loadedFaceAtlas.GetTextureGUIDFromName(faceTexture);
			int textureIndex2 = loadedFaceAtlas.GetTextureIndex(textureGUIDFromName2);
			Rect uvRect2 = loadedFaceAtlas.uvs[textureIndex2];
			component2.uvRect = uvRect2;
			if (faceScale != Vector3.zero)
			{
				component2.transform.localScale = faceScale;
			}
			component2.transform.localPosition = component2.transform.localPosition + faceOffset;
		}
		if (material != null)
		{
			Material material4 = new Material(material);
			material4.shader = Shader.Find("Unlit/Transparent Colored");
			UITexture component3 = Util.GetObjectExactMatch(base.gameObject, "Texture").GetComponent<UITexture>();
			component3.material = material4;
			component3.MakePixelPerfect();
			component3.transform.localScale *= 1.5f;
		}
	}

	public void OnTextureLoaded(Texture2D t)
	{
		if (t != null)
		{
			UITexture component = Util.GetObjectExactMatch(base.gameObject, "Texture").GetComponent<UITexture>();
			if (component != null)
			{
				component.mainTexture = t;
				component.MakePixelPerfect();
				component.transform.localScale = component.transform.localScale * 0.75f;
			}
		}
	}
}
