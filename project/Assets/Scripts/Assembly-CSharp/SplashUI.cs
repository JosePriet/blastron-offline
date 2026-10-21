using System.Collections;
using EB;
using UnityEngine;

public class SplashUI : MonoBehaviour
{
	public Material TextureMat;

	private UITexture _logoUITexture;

	private UITexture _logoJapaneseUITexture;

	private Transform _logoT;

	private bool _fadeIn;

	private float _t;

	private static Language _locale;

	private static bool _localeSet;

	public static Language Locale
	{
		get
		{
			return _locale;
		}
		set
		{
			_locale = value;
			_localeSet = true;
		}
	}

	private Transform LogoT
	{
		get
		{
			return _logoT;
		}
		set
		{
			_logoT = value;
			_fadeIn = true;
			_t = 0f;
		}
	}

	private IEnumerator Start()
	{
		while (!_localeSet)
		{
			yield return new WaitForFixedUpdate();
		}
		EB.Debug.LogWarning("LOCALE " + Locale);
		LogoT = base.transform.FindChild("logoName");
		if (!(LogoT != null))
		{
			yield break;
		}
		_logoUITexture = _logoT.GetComponent<UITexture>();
		EB.Debug.LogWarning("LogoUItex is " + _logoUITexture.mainTexture);
		string path = "SplashText/blastron_";
		switch (Locale)
		{
		case Language.English:
			path += "english";
			break;
		case Language.Korean:
			path += "korean";
			break;
		case Language.ChineseSimplified:
			path += "chinese_simplified";
			break;
		case Language.ChineseTraditional:
			path += "chinese_traditional";
			break;
		case Language.Japanese:
		{
			Transform logoJapanT = base.transform.FindChild("logoJapanese");
			if (logoJapanT != null)
			{
				string japanesePath = path + "japanese";
				_logoJapaneseUITexture = logoJapanT.GetComponent<UITexture>();
				yield return StartCoroutine(LoadTexture(japanesePath, _logoJapaneseUITexture));
				logoJapanT.gameObject.SetActive(true);
			}
			path += "english";
			break;
		}
		default:
			path += "english";
			break;
		}
		yield return StartCoroutine(LoadTexture(path, _logoUITexture));
		LogoT.gameObject.SetActive(true);
	}

	private IEnumerator LoadTexture(string imagePath, UITexture texHolder)
	{
		string path = Loader.GetBaseURL("ui") + imagePath + ".png";
		StartCoroutine(DoLoadTexure(path, texHolder));
		yield break;
	}

	private IEnumerator DoLoadTexure(string url, UITexture texHolder)
	{
		WWW www = new WWW(url);
		yield return www;
		StartCoroutine(OnTextureLoaded(www.texture, texHolder));
	}

	private IEnumerator OnTextureLoaded(Texture2D t, UITexture texHolder)
	{
		if (texHolder != null && t != null)
		{
			texHolder.mainTexture = t;
			texHolder.enabled = true;
		}
		yield break;
	}

	private void ReleaseTexture(UITexture texHolder)
	{
		if (texHolder != null)
		{
			Object.Destroy(texHolder.mainTexture);
			texHolder.mainTexture = null;
		}
	}

	private void Update()
	{
		if (_fadeIn)
		{
			_logoUITexture.alpha = _t;
			if (_logoJapaneseUITexture != null)
			{
				_logoJapaneseUITexture.alpha = _t;
			}
			_t += UnityEngine.Time.smoothDeltaTime;
			if (_t >= 1f)
			{
				_fadeIn = false;
			}
		}
	}

	private void OnDestroy()
	{
		if (TextureMat != null)
		{
			ReleaseTexture(_logoJapaneseUITexture);
			ReleaseTexture(_logoUITexture);
			Resources.UnloadAsset(TextureMat.mainTexture);
		}
	}
}
