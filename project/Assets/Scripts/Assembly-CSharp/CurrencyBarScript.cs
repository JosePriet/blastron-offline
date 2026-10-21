using EB;
using UnityEngine;

public class CurrencyBarScript : MonoBehaviour
{
	private UILabel labelSoft;

	private UILabel labelHard;

	private int softCurrencyValue;

	private int hardCurrencyValue;

	private void Awake()
	{
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "LabelSoft");
		labelSoft = Util.FindComponent<UILabel>(objectExactMatch);
		objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "LabelHard");
		labelHard = Util.FindComponent<UILabel>(objectExactMatch);
		UpdateLabelContents();
	}

	private void Update()
	{
		if (!(Profile.Instance == null) && (Profile.Instance.SoftCurrency != softCurrencyValue || Profile.Instance.HardCurrency != hardCurrencyValue))
		{
			UpdateLabelContents();
		}
	}

	private void UpdateLabelContents()
	{
		if (!(Profile.Instance == null))
		{
			softCurrencyValue = Profile.Instance.SoftCurrency;
			labelSoft.text = softCurrencyValue.ToString();
			hardCurrencyValue = Profile.Instance.HardCurrency;
			labelHard.text = hardCurrencyValue.ToString();
		}
	}
}
