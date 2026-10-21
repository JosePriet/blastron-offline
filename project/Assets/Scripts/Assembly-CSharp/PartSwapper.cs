using EB;
using SmoothMoves;
using UnityEngine;

public class PartSwapper : MonoBehaviour
{
	private BoneAnimation _smCharacter;

	private void Awake()
	{
		_smCharacter = GetComponent<BoneAnimation>();
	}

	public void ResetTexture(CharacterDB.Category category)
	{
		switch (category)
		{
		case CharacterDB.Category.kHead:
			ResetHeadTexture();
			break;
		case CharacterDB.Category.kTorso:
			ResetBodyTexture();
			break;
		case CharacterDB.Category.kLegs:
			ResetLegTexture();
			break;
		default:
			EB.Debug.LogError("trying to reset texture on invalid part " + category);
			break;
		}
	}

	private void ResetHeadTexture()
	{
		_smCharacter.RestoreBoneTexture("head");
		_smCharacter.RestoreBoneTexture("Facial expression");
	}

	private void ResetBodyTexture()
	{
		_smCharacter.RestoreBoneTexture("body");
		_smCharacter.RestoreBoneTexture("shoulder_R");
		_smCharacter.RestoreBoneTexture("arm_R");
		_smCharacter.RestoreBoneTexture("arm_L");
	}

	private void ResetLegTexture()
	{
		_smCharacter.RestoreBoneTexture("upper_leg_R");
		_smCharacter.RestoreBoneTexture("lower_leg_R");
		_smCharacter.RestoreBoneTexture("upper_leg_L");
		_smCharacter.RestoreBoneTexture("lower_leg_L");
	}

	public void SetTexture(CharacterDB.Category cat, TextureAtlas defaultAtlas, TextureAtlas otherAtlas)
	{
		if (otherAtlas == null)
		{
			EB.Debug.LogError("SetTexture:Missing atlas!!!");
			return;
		}
		if (defaultAtlas == null)
		{
			EB.Debug.LogError("SetTexture:Missing default atlas!!!");
			return;
		}
		bool useSearchAtlasPivots = false;
		if (cat == CharacterDB.Category.kHead)
		{
			useSearchAtlasPivots = false;
		}
		for (int i = 0; i < defaultAtlas.textureGUIDs.Count && i < otherAtlas.textureGUIDs.Count; i++)
		{
			_smCharacter.SwapTexture(defaultAtlas, defaultAtlas.textureGUIDs[i], otherAtlas, otherAtlas.textureGUIDs[i], useSearchAtlasPivots);
		}
	}

	public void SwapFaceTexture(TextureAtlas defaultAtlas, TextureAtlas otherAtlas)
	{
		if (otherAtlas == null)
		{
			EB.Debug.LogError(string.Format("SwapFaceTexture: Attempt to exchange {0} with otherAtlas failed because otherAtlas was null", defaultAtlas));
			return;
		}
		if (defaultAtlas == null)
		{
			EB.Debug.LogError("SwapFaceTexture: Missing default atlas!!!");
			return;
		}
		if (otherAtlas.textureGUIDs.Count > 1)
		{
			_smCharacter.RestoreBoneTexture("Facial expression");
			return;
		}
		for (int i = 0; i < defaultAtlas.textureGUIDs.Count; i++)
		{
			_smCharacter.SwapTexture(defaultAtlas, defaultAtlas.textureGUIDs[i], otherAtlas, otherAtlas.textureGUIDs[0], true);
		}
	}

	public void SwapMaterial(Material oldMaterial, Material newMaterial)
	{
		if (oldMaterial != null && newMaterial != null)
		{
			_smCharacter.SwapMaterial(oldMaterial, newMaterial);
		}
	}
}
