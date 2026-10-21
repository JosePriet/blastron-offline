using System;
using System.Collections;
using EB;
using UnityEngine;

public class IntroSequenceUI : MonoBehaviour
{
	public enum IntroState
	{
		Image1_wait1 = 0,
		Image1_text1 = 1,
		Image1_scroll = 2,
		Image1_wait2 = 3,
		Image1_text2 = 4,
		Image2_wait1 = 5,
		Image2_text = 6,
		Image3_wait1 = 7,
		Image3_text = 8,
		Image4_wait1 = 9,
		Image4_text1 = 10,
		Image4_text2 = 11,
		Image5_wait1 = 12,
		Image6_wait1 = 13,
		Image7_wait1 = 14,
		Image8_wait1 = 15,
		Image9_wait1 = 16,
		Image9_text1 = 17,
		Image9_text2 = 18,
		Count = 19
	}

	[System.Serializable]
	public class State
	{
		public IntroState state;

		public float duration;
	}

	public State[] States = new State[19];

	private IntroState currentState;

	private float timer;

	private UITexture image;

	private TextureFade fader;

	private UILabel text1;

	private UILabel text2;

	private string imagePath;

	public Vector3 initialImagePosition = new Vector3(-256f, 0f, 0f);

	public bool DoLoginOnFinish;

	private int pixelsToScroll = 512;

	private bool scaleBigger;

	private bool _didExit;

	private void Awake()
	{
		currentState = IntroState.Image1_wait1;
		fader = GetComponentInChildren<TextureFade>();
		image = Util.GetObjectExactMatch(base.gameObject, "Image").GetComponent<UITexture>();
		text1 = Util.GetObjectExactMatch(base.gameObject, "Label_Subtitle1").GetComponent<UILabel>();
		text2 = Util.GetObjectExactMatch(base.gameObject, "Label_Subtitle2").GetComponent<UILabel>();
		text1.text = string.Empty;
		text2.text = string.Empty;
		scaleBigger = false;
		if (Misc.IsPhone())
		{
			scaleBigger = true;
		}
		if (scaleBigger)
		{
			text1.transform.localPosition = new Vector3(0f, -270f, -110f);
			text2.transform.localPosition = new Vector3(0f, -330f, -110f);
			GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "TextureHolder");
			UIPanel component = objectExactMatch.GetComponent<UIPanel>();
			component.clipRange = new Vector4(0f, 0f, 1024f, 512f);
			initialImagePosition *= 2f;
			pixelsToScroll *= 2;
		}
		else
		{
			text1.transform.localPosition = new Vector3(0f, -180f, -110f);
			text2.transform.localPosition = new Vector3(0f, -240f, -110f);
			GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "TextureHolder");
			UIPanel component2 = objectExactMatch2.GetComponent<UIPanel>();
			component2.clipRange = new Vector4(0f, 0f, 665.6f, 332.8f);
			initialImagePosition *= 1.3f;
			pixelsToScroll = (int)((float)pixelsToScroll * 1.3f);
		}
		bool Argument = true;
		bool Argument2 = false;
		Messenger<bool, bool>.BroadcastToAllListeners("MuteAudioJukebox", ref Argument, ref Argument2);
	}

	private void OnOpenPopup()
	{
		ScreenManager.Instance.FadeOut(null);
		Refresh();
	}

	private void Update()
	{
		if (currentState < IntroState.Count && UnityEngine.Time.time - timer > States[(int)currentState].duration)
		{
			currentState++;
			Refresh();
		}
		else if (currentState >= IntroState.Count)
		{
			Exit();
		}
	}

	private void OnDestroy()
	{
		bool Argument = Options.sound == 0;
		bool Argument2 = false;
		Messenger<bool, bool>.BroadcastToAllListeners("MuteAudioJukebox", ref Argument, ref Argument2);
		ReleaseTexture();
	}

	private void LoadTexture(string imagePath)
	{
		string url = Loader.GetBaseURL("ui") + imagePath + ".jpg";
		StartCoroutine(DoLoadTexure(url));
	}

	private IEnumerator DoLoadTexure(string url)
	{
		WWW www = new WWW(url);
		yield return www;
		OnTextureLoaded(www.texture);
	}

	private void Refresh()
	{
		timer = UnityEngine.Time.time;
		switch (currentState)
		{
		case IntroState.Image1_wait1:
			imagePath = "intro/opening_01";
			LoadTexture(imagePath);
			break;
		case IntroState.Image1_text1:
			text1.text = "ID_UI_INTRO_IMAGE1_TEXT1";
			break;
		case IntroState.Image1_scroll:
			StartCoroutine("imageScroll", States[(int)currentState].duration);
			break;
		case IntroState.Image1_wait2:
			text1.text = string.Empty;
			image.transform.localPosition = initialImagePosition - Vector3.right * pixelsToScroll;
			break;
		case IntroState.Image1_text2:
			text1.text = "ID_UI_INTRO_IMAGE1_TEXT2";
			break;
		case IntroState.Image2_wait1:
			ReleaseTexture();
			imagePath = "intro/opening_02";
			LoadTexture(imagePath);
			text1.text = string.Empty;
			break;
		case IntroState.Image2_text:
			text1.text = "ID_UI_INTRO_IMAGE2_TEXT";
			break;
		case IntroState.Image3_wait1:
			text1.text = string.Empty;
			ReleaseTexture();
			imagePath = "intro/opening_03";
			LoadTexture(imagePath);
			break;
		case IntroState.Image3_text:
			text1.text = "ID_UI_INTRO_IMAGE3_TEXT";
			break;
		case IntroState.Image4_wait1:
			ReleaseTexture();
			text1.text = string.Empty;
			imagePath = "intro/opening_04";
			LoadTexture(imagePath);
			break;
		case IntroState.Image4_text1:
			text1.text = "ID_UI_INTRO_IMAGE4_TEXT1";
			break;
		case IntroState.Image4_text2:
			text1.text = "ID_UI_INTRO_IMAGE4_TEXT2";
			break;
		case IntroState.Image5_wait1:
			text1.text = string.Empty;
			ReleaseTexture();
			imagePath = "intro/opening_05";
			LoadTexture(imagePath);
			break;
		case IntroState.Image6_wait1:
			ReleaseTexture();
			imagePath = "intro/opening_06";
			LoadTexture(imagePath);
			break;
		case IntroState.Image7_wait1:
			ReleaseTexture();
			imagePath = "intro/opening_07";
			LoadTexture(imagePath);
			break;
		case IntroState.Image8_wait1:
			ReleaseTexture();
			imagePath = "intro/opening_08";
			LoadTexture(imagePath);
			text1.text = "ID_UI_INTRO_IMAGE8_TEXT";
			break;
		case IntroState.Image9_wait1:
			ReleaseTexture();
			imagePath = "intro/opening_09";
			LoadTexture(imagePath);
			text1.text = string.Empty;
			break;
		case IntroState.Image9_text1:
			text1.text = "ID_UI_INTRO_IMAGE9_TEXT1_1";
			text2.text = "ID_UI_INTRO_IMAGE9_TEXT1_2";
			break;
		case IntroState.Image9_text2:
			text1.text = "ID_UI_INTRO_IMAGE9_TEXT2";
			text2.text = string.Empty;
			break;
		}
	}

	private IEnumerator imageScroll(float duration)
	{
		float startTime = UnityEngine.Time.time;
		float rate = (float)pixelsToScroll / duration;
		while (UnityEngine.Time.time - startTime < duration)
		{
			yield return new WaitForFixedUpdate();
			image.transform.localPosition = initialImagePosition - Vector3.right * rate * (UnityEngine.Time.time - startTime);
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

	public void OnTextureLoaded(Texture2D t)
	{
		if (image != null && t != null)
		{
			image.mainTexture = t;
			image.enabled = true;
			if (imagePath == "intro/opening_01")
			{
				fader.SetTexture(image);
			}
			image.transform.localPosition = initialImagePosition;
			if (scaleBigger)
			{
				image.transform.localScale = new Vector3((float)t.width * 2f, (float)t.height * 2f, 1f);
			}
			else
			{
				image.transform.localScale = new Vector3((float)t.width * 1.3f, (float)t.height * 1.3f, 1f);
			}
		}
	}

	private void OnClick()
	{
		Exit();
	}

	private void Exit()
	{
		if (_didExit)
		{
			return;
		}
		_didExit = true;
		ScreenManager.Instance.FadeIn(() =>
		{
			ScreenManager.Instance.ClosePopup();
			if (DoLoginOnFinish)
			{
				Setup.Instance.CheckForFirstLogin();
			}
		});
	}
}
