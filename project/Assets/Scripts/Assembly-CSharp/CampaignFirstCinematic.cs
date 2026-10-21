using System;
using System.Collections;
using System.Collections.Generic;
using EB;
using UnityEngine;

public class CampaignFirstCinematic : MonoBehaviour
{
	[System.Serializable]
	public class CinematicParams
	{
		public GameObject Panel;

		public string AssetName;

		public string Label;

		public float durationToNext;

		public float durationToFade;
	}

	public List<CinematicParams> _parameters = new List<CinematicParams>();

	private float timer;

	private int _currentImage;

	private UITexture image;

	private TextureFade fader;

	private UILabel Label;

	private string imagePath;

	private bool goToNext = true;

	private void Awake()
	{
		Label = Util.GetObjectExactMatch(base.gameObject, "Label_Subtitle1").GetComponent<UILabel>();
		Label.text = string.Empty;
		if (ScreenManager.Instance.GetPopup("ConfirmationDialogUI") != null)
		{
			ScreenManager.Instance.ClosePopup("ConfirmationDialogUI");
		}
	}

	private void OnOpenPopup()
	{
		if (_parameters.Count > 0)
		{
			Refresh();
			ScreenManager.Instance.FadeOut(null);
		}
		else
		{
			Exit();
		}
	}

	private void Refresh()
	{
		timer = UnityEngine.Time.time;
		if (_parameters[_currentImage].AssetName != string.Empty)
		{
			image = Util.FindComponent<UITexture>(_parameters[_currentImage].Panel);
			fader = Util.FindComponent<TextureFade>(_parameters[_currentImage].Panel);
			imagePath = "cinematics/" + _parameters[_currentImage].AssetName;
			ReleaseTexture();
			StartCoroutine(FadeOut(_parameters[_currentImage].durationToFade, image));
			LoadTexture(imagePath);
		}
		if (_parameters[_currentImage].Label != string.Empty)
		{
			Label.text = Localizer.GetString(_parameters[_currentImage].Label);
		}
	}

	private IEnumerator FadeOut(float time, UITexture tex)
	{
		yield return new WaitForSeconds(time);
		while (tex.alpha > 0f)
		{
			tex.alpha -= 2f * UnityEngine.Time.deltaTime;
			yield return null;
		}
		tex.alpha = 0f;
		if (tex != null)
		{
			UnityEngine.Object.Destroy(tex.mainTexture);
			tex.mainTexture = null;
			tex.enabled = false;
		}
	}

	private void LoadTexture(string imagePath)
	{
		string url = Loader.GetBaseURL("championship") + imagePath + ".jpg";
		StartCoroutine(DoLoadTexure(url));
	}

	private IEnumerator DoLoadTexure(string url)
	{
		WWW www = new WWW(url);
		yield return www;
		OnTextureLoaded(www.texture);
	}

	public void OnTextureLoaded(Texture2D t)
	{
		if (image != null && t != null)
		{
			image.mainTexture = t;
			image.enabled = true;
			fader.SetTexture(image);
		}
	}

	private void ReleaseTexture()
	{
		if (image != null)
		{
			UnityEngine.Object.Destroy(image.mainTexture);
			image.mainTexture = null;
			image.enabled = false;
		}
	}

	private void Update()
	{
		if (goToNext && UnityEngine.Time.time - timer >= _parameters[_currentImage].durationToNext)
		{
			_currentImage++;
			if (_currentImage >= _parameters.Count)
			{
				Exit();
			}
			else
			{
				Refresh();
			}
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Exit();
		}
	}

	private void OnClick()
	{
		Exit();
	}

	private void Exit()
	{
		if (goToNext)
		{
			goToNext = false;
			ScreenManager.Instance.FadeIn(() =>
			{
				StopAllCoroutines();
				StoryManager.Instance.isStoryPlaying = false;
				ScreenManager.Instance.ClosePopup();
			});
		}
	}
}
