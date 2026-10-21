using System;

[Serializable]
public class CampaignCinematic
{
	[Serializable]
	public class ImgParameters
	{
		public float Duration;

		public string label;

		public bool isSubtitle;
	}

	public string ImageAssetName;

	public ImgParameters[] ImageParams = new ImgParameters[0];

	internal float duration;
}
