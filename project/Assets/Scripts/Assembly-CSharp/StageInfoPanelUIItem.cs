using EB;
using EB.Sparx;
using UnityEngine;

public class StageInfoPanelUIItem : MonoBehaviour
{
	public enum ScoreType
	{
		KILLS = 0,
		POINTS = 1
	}

	private const string LOCID_FORMAT_POINTS = "ID_UI_STAGEINFO_FORMAT_POINTS";

	private const string LOCID_FORMAT_KILLS = "ID_UI_STAGEINFO_FORMAT_KILLS";

	public UILabel _playerName;

	public UILabel _playerScore;

	public UILabel _rank;

	public UITexture _portraitTexture;

	public UISprite _rankBG;

	public Color[] rankColors = new Color[4]
	{
		new Color(1f, 215f / 255f, 0f),
		new Color(192f / 255f, 192f / 255f, 192f / 255f),
		new Color(205f / 255f, 149f / 255f, 12f / 255f),
		Color.white
	};

	public Color yourFrameColor = new Color(253f / 256f, 175f / 256f, 0f, 1f);

	private int _rankIndex;

	private TextureFade _portraitFader;

	public void Init(int rankIndex)
	{
		_rankIndex = rankIndex;
		if (_rankIndex <= 10)
		{
			NGUITools.SetActive(Util.GetObjectExactMatch(base.gameObject, "Line"), false);
		}
	}

	private void Awake()
	{
		_portraitFader = _portraitTexture.gameObject.AddComponent<TextureFade>();
	}

	private void Start()
	{
		_rankBG.color = rankColors[Mathf.Min(_rankIndex, rankColors.Length - 1)];
		_rank.text = (_rankIndex + 1).ToString();
	}

	public void SetData(ChampionshipLeaderboards.PlayerData playerData, ScoreType scoreType)
	{
		_playerName.text = playerData.personName;
		if (scoreType.Equals(ScoreType.KILLS))
		{
			if (playerData.isTied)
			{
				_playerScore.text = Localizer.Format("ID_UI_STAGEINFO_FORMAT_KILLS", playerData.score) + " &\n" + Localizer.Format("ID_UI_STAGEINFO_FORMAT_POINTS", playerData.altScore);
			}
			else
			{
				_playerScore.text = Localizer.Format("ID_UI_STAGEINFO_FORMAT_KILLS", playerData.score);
			}
		}
		else
		{
			_playerScore.text = Localizer.Format("ID_UI_STAGEINFO_FORMAT_POINTS", playerData.score);
		}
		_portraitFader.HideTexture();
		_portraitTexture.enabled = false;
		if (playerData.fbId == Hub.Instance.FacebookManager.LocalUser.FacebookId)
		{
			GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "Frame");
			objectExactMatch.GetComponent<UISlicedSprite>().color = yourFrameColor;
		}
		else
		{
			GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "sectionBackground");
			if (objectExactMatch2 != null)
			{
				NGUITools.SetActive(objectExactMatch2, false);
			}
		}
		if (!string.IsNullOrEmpty(playerData.fbId))
		{
			string imageUrlForUser = FacebookUser.GetImageUrlForUser(playerData.fbId, FacebookUser.ImageType.square);
			FacebookPortraitManager.Load(imageUrlForUser, (Texture2D tex) =>
			{
				if (tex != null)
				{
					if (_portraitTexture != null)
					{
						_portraitTexture.mainTexture = tex;
						_portraitTexture.enabled = true;
						_portraitFader.SetTexture(_portraitTexture);
						_portraitTexture.MarkAsChanged();
					}
				}
				else
				{
					EB.Debug.LogWarning("could not find texture for fbid " + playerData.fbId);
				}
			});
			return;
		}
		CharacterDB.MetaData metaDataOrDefault = CharacterDB.GetMetaDataOrDefault(playerData.recipe, CharacterDB.Category.kHead);
		if (metaDataOrDefault != null)
		{
			TexturePoolManager.Instance.LoadTexture("cac_ui_parts/" + metaDataOrDefault.cacTexturePath, (Texture2D tex) =>
			{
				_portraitTexture.mainTexture = tex;
				_portraitTexture.enabled = true;
				_portraitFader.SetTexture(_portraitTexture);
				_portraitTexture.MarkAsChanged();
			});
		}
	}
}
