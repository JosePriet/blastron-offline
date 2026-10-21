using System;
using EB;
using EB.Sparx;

public static class TrialPay
{
	public static bool ShouldShowTrialPay()
	{
		return TrialPayIsEnabled() && CheckTutorials() && CheckRevenue() && CheckCohortDate() && CheckCountry() && CheckBeginnerSale();
	}

	private static bool TrialPayIsEnabled()
	{
		bool trialPay_Enabled = Tuning.Instance.TrialPay_Enabled;
		Debug.Log("TrialPay > TrialPayIsEnabled > enabled: " + trialPay_Enabled);
		return trialPay_Enabled;
	}

	private static bool CheckTutorials()
	{
		bool flag = Profile.Instance.IsTutorialDone("firstboot_shown") && Profile.Instance.IsTutorialDone("mp") && Profile.Instance.IsTutorialDone("vault");
		Debug.Log("TrialPay > CheckTutorials > tutorialsComplete: " + flag);
		return flag;
	}

	private static bool CheckBeginnerSale()
	{
		bool flag = Profile.Instance.HasBeginnerSaleExpired();
		Debug.Log("TrialPay > CheckBeginnerSale > enabled: " + flag);
		return flag;
	}

	private static bool CheckRevenue()
	{
		int revenue = Hub.Instance.LoginManager.LocalUser.Revenue;
		Debug.Log("TrialPay > CheckRevenue > revenue: " + revenue);
		return Hub.Instance.LoginManager.LocalUser.Revenue == 0;
	}

	private static bool CheckCountry()
	{
		string text = EB.Version.GetCountryCode().ToString().ToLower();
		Debug.Log("TrialPay > CheckCountry > user country: " + text);
		Debug.Log("TrialPay > CheckCountry > trial pay countries: " + Tuning.Instance.TrialPay_Countries);
		char[] separator = new char[1] { ',' };
		string[] array = Tuning.Instance.TrialPay_Countries.Split(separator, StringSplitOptions.RemoveEmptyEntries);
		string[] array2 = array;
		foreach (string text2 in array2)
		{
			if (text2.ToLower().Equals("all") || text2.ToLower().Equals(text))
			{
				return true;
			}
		}
		return false;
	}

	private static bool CheckCohortDate()
	{
		int num = Time.Now - Hub.Instance.LoginManager.LocalUser.CohortDate;
		bool flag = num > Tuning.Instance.TrialPay_Time;
		Debug.Log("TrialPay > CheckCohortDate > show if timeSinceCohortDate (" + num + ") > trialPayTime(" + Tuning.Instance.TrialPay_Time + "): " + flag);
		return flag;
	}
}
