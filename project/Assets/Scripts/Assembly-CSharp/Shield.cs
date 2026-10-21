using EB;
using UnityEngine;

public class Shield : MonoBehaviour
{
	private UISlider _healthProgressBar;

	private Healthbar _healthBar;

	private float _HP;

	private float _maxHP;

	private bool _isActive;

	public void Initialize(float shieldHP)
	{
		_maxHP = shieldHP;
		_HP = _maxHP;
		EB.Debug.Log("Setting shield maxHP: " + _maxHP);
	}

	public void RegisterHealthbar(Healthbar bar, UISlider progressBar)
	{
		_healthBar = bar;
		_healthProgressBar = progressBar;
		EB.Debug.Log(string.Concat("Registering healthBar: ", bar, ", progressBar: ", progressBar));
	}

	public void Activate()
	{
		_isActive = true;
		_HP = _maxHP;
		_healthProgressBar.sliderValue = 1f;
		_healthBar.ShowShield(true);
	}

	public void Deactivate()
	{
		_isActive = false;
		_HP = 0f;
		_healthProgressBar.sliderValue = 0f;
		_healthBar.ShowShield(false);
		Object.Destroy(this);
	}

	public bool ApplyHit(ref Misc.HitInfo hitInfo)
	{
		float num = _HP - hitInfo._damage;
		float num2 = 0f;
		if (num <= 0f)
		{
			num = 0f;
			num2 = hitInfo._damage - _HP;
		}
		else
		{
			num2 = 0f;
		}
		hitInfo._damage = num2;
		_HP = num;
		if (_HP <= 0f)
		{
			Deactivate();
			return true;
		}
		_healthProgressBar.sliderValue = _HP / _maxHP;
		return false;
	}
}
