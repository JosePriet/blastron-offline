using System.Collections;
using EB;
using UnityEngine;

public class ChampionshipRewardUI : PopupScript
{
	public float _timeOnScreen = 1f;

	private UILabel firstLabel;

	private UILabel secondLabel;

	private Weapon _weapon;

	private UITexture texture;

	private string _secondary = string.Empty;

	private string _texturePath = string.Empty;

	private void Awake()
	{
		firstLabel = Util.GetObjectExactMatch(base.gameObject, "Label").GetComponent<UILabel>();
		secondLabel = Util.GetObjectExactMatch(base.gameObject, "WeaponLabel").GetComponent<UILabel>();
		texture = Util.FindComponent<UITexture>(Util.GetObjectExactMatch(base.gameObject, "Texture"));
		firstLabel.text = Localizer.Format("ID_UI_STAGEINFO_SPECIAL_REWARDS", string.Empty);
	}

	public void SetParameters(string texturePath, string secondary, bool isUnlock)
	{
		if (isUnlock)
		{
			firstLabel.text = Localizer.Format("ID_UI_RESULTS_UNLOCK", string.Empty);
		}
		_secondary = secondary;
		_texturePath = texturePath;
		LoadTexture(texturePath);
	}

	private void LoadTexture(string Path)
	{
		TexturePoolManager.Instance.LoadTexture(Path, (Texture2D tex) =>
		{
			texture.mainTexture = tex;
			secondLabel.text = _secondary;
		});
	}

	protected override void OnTransitionInComplete()
	{
		StartCoroutine(stayOnScreen());
	}

	protected override void OnTransitionOutComplete()
	{
		ChampionshipData.Instance._rewardWeapon = false;
		ChampionshipData.Instance._rewardItem = false;
		ReleaseTexture();
		base.OnTransitionOutComplete();
	}

	private IEnumerator stayOnScreen()
	{
		yield return new WaitForSeconds(_timeOnScreen);
		base.Close(base.gameObject);
	}

	private void ReleaseTexture()
	{
		if (!string.IsNullOrEmpty(_texturePath))
		{
			TexturePoolManager.Instance.ReleaseTexture(_texturePath);
		}
	}
}
