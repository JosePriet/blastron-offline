using System;
using UnityEngine;

namespace EB.Sparx
{
	public static class Device
	{
		public static string UniqueIdentifier
		{
			get
			{
				string text = PlayerPrefs.GetString("udid", string.Empty);
				if (string.IsNullOrEmpty(text))
				{
					text = Version.GetUDID();
					if (string.IsNullOrEmpty(text))
					{
						text = NewUniqueIdentifier();
					}
				}
				return text;
			}
		}

		public static string Platform
		{
			get
			{
				return "android";
			}
		}

		public static string DeviceModel
		{
			get
			{
				return SystemInfo.deviceModel;
			}
		}

		public static string MobilePlatform
		{
			get
			{
				return "android";
			}
		}

		public static string NewUniqueIdentifier()
		{
			string text = Guid.NewGuid().ToString();
			PlayerPrefs.SetString("udid", text);
			PlayerPrefs.Save();
			return text;
		}
	}
}
