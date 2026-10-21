using System;
using System.Collections;
using EB;
using UnityEngine;

public class EnergyTimer : MonoBehaviour
{
	private UILabel _energyLabel;

	private UILabel _timeLabel;

	private UILabel _energyFullLabel;

	private GameObject _timeObject;

	private void Awake()
	{
		Transform transform = base.transform.FindChild("EnergyButton");
		Transform transform2 = base.transform.FindChild("TimeWidget");
		_timeObject = transform2.gameObject;
		_energyLabel = transform.transform.FindChild("Label").GetComponent<UILabel>();
		_timeLabel = transform2.transform.FindChild("Label").GetComponent<UILabel>();
		_energyLabel.text = Profile.Instance.Energy.ToString();
		_energyFullLabel = base.transform.FindChild("EnergyFull").GetComponent<UILabel>();
	}

	private void OnEnable()
	{
		StartCoroutine(UpdateTimer());
	}

	private IEnumerator UpdateTimer()
	{
		while (true)
		{
			DetermineGrant();
			_energyLabel.text = Localizer.Format("ID_UI_STATUS_BAR_XP", Profile.Instance.Energy.ToString(), Profile.Instance.EnergyMax.ToString());
			if (Profile.Instance.Energy < Profile.Instance.EnergyMax)
			{
				int delta = Profile.Instance.NextEnergyAt - EB.Time.Now;
				TimeSpan t = TimeSpan.FromSeconds(delta);
				_timeLabel.text = t.ToString().Substring(3);
				_energyFullLabel.enabled = false;
				_timeObject.SetActive(true);
			}
			else
			{
				_energyFullLabel.enabled = true;
				_timeObject.SetActive(false);
			}
			yield return new WaitForSeconds(1f);
		}
	}

	private void DetermineGrant()
	{
		int num = EB.Time.Now - Profile.Instance.NextEnergyAt;
		if (num >= 0)
		{
			int num2 = Tuning.Instance.GrantEnergyInSecs - num % Tuning.Instance.GrantEnergyInSecs;
			Profile.Instance.RestoreEnergy(1 + num / Tuning.Instance.GrantEnergyInSecs, EB.Time.Now + num2);
		}
	}

	private void OnClick()
	{
		if (Profile.Instance.Energy == Profile.Instance.EnergyMax)
		{
			ScreenManager.Instance.OpenPopup("EnergyFullUI", true, true);
		}
		else
		{
			ScreenManager.Instance.OpenPopup("GetMoreEnergyUI", true, true);
		}
	}
}
