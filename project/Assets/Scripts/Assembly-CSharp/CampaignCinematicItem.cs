using System.Collections;
using System.Collections.Generic;
using EB;
using UnityEngine;

public class CampaignCinematicItem : MonoBehaviour
{
	private UILabel label1;

	private UILabel label2;

	private UITexture image;

	private UIPanel panel;

	public static List<CampaignCinematic> _items = new List<CampaignCinematic>();

	public static bool isFirstCampaignCinematic;

	private int _CurrentStoryMoment;

	private string imagePath;

	private float timer;

	private Vector4 origPanel;

	private TextureFade fader;

	private void Awake()
	{
		GameObject gameObject = GameObject.Find("SinglePlayerUI");
		if (gameObject != null)
		{
			AudioSource[] components = gameObject.GetComponents<AudioSource>();
			AudioSource[] array = components;
			foreach (AudioSource audioSource in array)
			{
				audioSource.enabled = false;
			}
		}
		panel = Util.GetObjectExactMatch(base.gameObject, "TextureHolder").GetComponent<UIPanel>();
		origPanel = panel.clipRange;
		fader = GetComponentInChildren<TextureFade>();
		image = Util.GetObjectExactMatch(base.gameObject, "Image").GetComponent<UITexture>();
		label1 = Util.GetObjectExactMatch(base.gameObject, "Label_Subtitle1").GetComponent<UILabel>();
		label2 = Util.GetObjectExactMatch(base.gameObject, "Label_Subtitle2").GetComponent<UILabel>();
		label1.text = string.Empty;
		label2.text = string.Empty;
	}

	private void OnOpenPopup()
	{
		Refresh();
	}

	private IEnumerator AdvancePanel(float index)
	{
		label1.text = string.Empty;
		label2.text = string.Empty;
		while (_items[_CurrentStoryMoment].ImageParams.Length > 0)
		{
			if (_items[_CurrentStoryMoment].ImageParams[(int)index - 1].label != string.Empty)
			{
				if (!_items[_CurrentStoryMoment].ImageParams[(int)index - 1].isSubtitle)
				{
					label1.text = Localizer.GetString(_items[_CurrentStoryMoment].ImageParams[(int)index - 1].label);
				}
				else
				{
					label2.text = Localizer.GetString(_items[_CurrentStoryMoment].ImageParams[(int)index - 1].label);
				}
			}
			float percentage = index / (float)_items[_CurrentStoryMoment].ImageParams.Length;
			Vector4 clipRange = panel.clipRange;
			clipRange.x = ((clipRange.z = origPanel.z * percentage) - origPanel.z) / 2f;
			panel.clipRange = clipRange;
			yield return new WaitForSeconds(_items[_CurrentStoryMoment].ImageParams[(int)index - 1].Duration);
			index++;
		}
	}

	private void Update()
	{
		if (UnityEngine.Time.time - timer >= _items[_CurrentStoryMoment].duration)
		{
			_CurrentStoryMoment++;
			if (_CurrentStoryMoment >= _items.Count)
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

	private void Refresh()
	{
		StopCoroutine("AdvancePanel");
		timer = UnityEngine.Time.time;
		if (_items[_CurrentStoryMoment].ImageAssetName != string.Empty)
		{
			ReleaseTexture();
			float num = 0f;
			for (int i = 0; i < _items[_CurrentStoryMoment].ImageParams.Length; i++)
			{
				num += _items[_CurrentStoryMoment].ImageParams[i].Duration;
			}
			_items[_CurrentStoryMoment].duration = num;
			imagePath = "cinematics/" + _items[_CurrentStoryMoment].ImageAssetName;
			LoadTexture(imagePath);
			StartCoroutine("AdvancePanel", 1);
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
			if (_CurrentStoryMoment == 0)
			{
				fader.SetTexture(image);
			}
		}
	}

	private void ReleaseTexture()
	{
		if (image != null)
		{
			Object.Destroy(image.mainTexture);
			image.mainTexture = null;
			image.enabled = false;
		}
	}

	private void OnClick()
	{
		Exit();
	}

	private void Exit()
	{
		StopAllCoroutines();
		StoryManager.Instance.isStoryPlaying = false;
		ScreenManager.Instance.ClosePopup();
	}

	private void OnClosePopup()
	{
		if (isFirstCampaignCinematic)
		{
			isFirstCampaignCinematic = false;
			EB.Debug.Log("WOOOT!!!!");
			ScreenManager.Instance.OpenPopup("SinglePlayerUI");
		}
	}
}
