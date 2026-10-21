using EB;
using EB.Sequence;
using SmoothMoves;
using UnityEngine;

public class PromoPopupUI : PopupScript
{
	public enum ButtonIds
	{
		Back = 0,
		Ok = 1
	}

	public enum ImageType
	{
		Robot = 0,
		Weapon = 1
	}

	public string headTextureName;

	public string faceTextureName;

	public TextureAtlas headAtlas;

	public TextureAtlas faceAtlas;

	private TextureAtlas loadedFaceAtlas;

	public Action<ButtonIds> _callback;

	private ButtonIds _action;

	public string weaponTexturePath = "weapon_ui/UI_weapon_minigun";

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
		InitHead(headTextureName, faceTextureName);
		EB.Sequence.Component.Activate(base.gameObject, "PromoPopupUI", typeof(SequenceEvent_OnScreenOpened));
	}

	private void Update()
	{
	}

	public void Init(string titleString, string descString, ImageType img, Action<ButtonIds> callback)
	{
		UIUtils.SetLabelContents(base.gameObject, "LabelPopupTitle", titleString);
		UIUtils.SetLabelContents(base.gameObject, "LabelDescription", descString);
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "ButtonOK");
		Util.FindComponent<ButtonScript>(objectExactMatch).onButtonClick += OnOk;
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "WeaponTexture");
		if (objectExactMatch2 != null)
		{
			NGUITools.SetActive(objectExactMatch2, img == ImageType.Weapon);
		}
		GameObject objectExactMatch3 = Util.GetObjectExactMatch(base.gameObject, "PartsTexture");
		if (objectExactMatch3 != null)
		{
			NGUITools.SetActive(objectExactMatch3, img != ImageType.Weapon);
		}
		_callback = callback;
	}

	protected override void OnTransitionOutComplete()
	{
		base.OnTransitionOutComplete();
		if (_callback != null)
		{
			_callback(_action);
		}
		EB.Sequence.Component.Activate(base.gameObject, "PromoPopupUI", typeof(SequenceEvent_OnScreenClosed));
	}

	protected override void OnClose()
	{
		_action = ButtonIds.Back;
		base.OnClose();
	}

	private void OnOk(GameObject caller)
	{
		_action = ButtonIds.Ok;
		base.OnClose();
	}

	private void InitHead(string headTexture, string faceTexture)
	{
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "RobotFace");
		if (!string.IsNullOrEmpty(headTexture))
		{
			Material material = new Material(headAtlas.material);
			material.shader = Shader.Find("Unlit/Transparent Colored");
			UITexture component = Util.GetObjectExactMatch(objectExactMatch, "Texture").GetComponent<UITexture>();
			component.material = material;
			string textureGUIDFromName = headAtlas.GetTextureGUIDFromName(headTexture);
			int textureIndex = headAtlas.GetTextureIndex(textureGUIDFromName);
			Rect uvRect = headAtlas.uvs[textureIndex];
			component.uvRect = uvRect;
			component.transform.localScale = new Vector3(122.5f, 122.5f, 0f);
		}
		if (!string.IsNullOrEmpty(faceTexture))
		{
			Material material2 = new Material(loadedFaceAtlas.material);
			material2.shader = Shader.Find("Unlit/Transparent Colored");
			UITexture component2 = Util.GetObjectExactMatch(objectExactMatch, "Texture2").GetComponent<UITexture>();
			component2.material = material2;
			string textureGUIDFromName2 = loadedFaceAtlas.GetTextureGUIDFromName(faceTexture);
			int textureIndex2 = loadedFaceAtlas.GetTextureIndex(textureGUIDFromName2);
			Rect uvRect2 = loadedFaceAtlas.uvs[textureIndex2];
			component2.uvRect = uvRect2;
			component2.transform.localScale = new Vector3(100f, 78f, 0f);
			component2.transform.localPosition = component2.transform.localPosition + new Vector3(-6f, -6f, 0f);
		}
	}
}
