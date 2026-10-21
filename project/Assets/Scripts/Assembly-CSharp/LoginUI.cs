using EB;
using EB.Sparx;
using UnityEngine;

public class LoginUI : MonoBehaviour
{
	private GameObject _panel1;

	private GameObject _panel2;

	private GameObject _panel3;

	private GameObject _panel4;

	private int activePanel;

	private bool _canSkip = true;

	public string Email
	{
		get
		{
			GameObject obj = ((activePanel != 2) ? _panel3 : _panel2);
			GameObject gameObject = Util.GetObject(obj, "Input_Email");
			UIInput component = gameObject.GetComponent<UIInput>();
			return component.text;
		}
		set
		{
			GameObject obj = ((activePanel != 2) ? _panel3 : _panel2);
			GameObject gameObject = Util.GetObject(obj, "Input_Email");
			UIInput component = gameObject.GetComponent<UIInput>();
			component.text = value;
		}
	}

	public string Name
	{
		get
		{
			GameObject gameObject = Util.GetObject(base.gameObject, "Input_Name");
			UIInput component = gameObject.GetComponent<UIInput>();
			return component.text;
		}
		set
		{
			GameObject gameObject = Util.GetObject(base.gameObject, "Input_Name");
			UIInput component = gameObject.GetComponent<UIInput>();
			component.text = value;
		}
	}

	public string Password
	{
		get
		{
			GameObject obj = ((activePanel != 2) ? _panel3 : _panel2);
			GameObject gameObject = Util.GetObject(obj, "Input_Password");
			UIInput component = gameObject.GetComponent<UIInput>();
			return component.text;
		}
		set
		{
			GameObject obj = ((activePanel != 2) ? _panel3 : _panel2);
			GameObject gameObject = Util.GetObject(obj, "Input_Password");
			UIInput component = gameObject.GetComponent<UIInput>();
			component.text = value;
			component.label.password = true;
		}
	}

	public string Password2
	{
		get
		{
			GameObject gameObject = Util.GetObject(base.gameObject, "Input_Password2");
			UIInput component = gameObject.GetComponent<UIInput>();
			return component.text;
		}
		set
		{
			GameObject gameObject = Util.GetObject(base.gameObject, "Input_Password2");
			UIInput component = gameObject.GetComponent<UIInput>();
			component.text = value;
			component.label.password = true;
		}
	}

	public bool CanSkip
	{
		get
		{
			return _canSkip;
		}
		set
		{
			_canSkip = value;
			if (!_canSkip)
			{
				GameObject obj = Util.GetObject(base.gameObject, "Button_Skip");
				Util.SetActiveRecursive(obj, false);
			}
		}
	}

	public string Error
	{
		get
		{
			GameObject gameObject = Util.GetObject(base.gameObject, "Label_Error");
			UILabel component = gameObject.GetComponent<UILabel>();
			return component.text;
		}
		set
		{
			GameObject gameObject = Util.GetObject(base.gameObject, "Label_Error");
			UILabel component = gameObject.GetComponent<UILabel>();
			component.text = value;
		}
	}

	private void Awake()
	{
		Error = string.Empty;
		_panel1 = Util.GetObjectExactMatch(base.gameObject, "Panel1");
		_panel2 = Util.GetObjectExactMatch(base.gameObject, "Panel2");
		_panel3 = Util.GetObjectExactMatch(base.gameObject, "Panel3");
		_panel4 = Util.GetObjectExactMatch(base.gameObject, "Panel4");
		User localUser = Hub.Instance.LoginManager.LocalUser;
		if (localUser != null)
		{
			Name = localUser.Name;
		}
		ShowPanel(1);
	}

	private void ShowPanel(int panelIndex)
	{
		activePanel = panelIndex;
		switch (panelIndex)
		{
		case 1:
			_panel1.SetActive(true);
			_panel2.SetActive(false);
			_panel3.SetActive(false);
			_panel4.SetActive(false);
			break;
		case 2:
			_panel1.SetActive(false);
			_panel2.SetActive(true);
			_panel3.SetActive(false);
			_panel4.SetActive(false);
			break;
		case 3:
			_panel1.SetActive(false);
			_panel2.SetActive(false);
			_panel3.SetActive(true);
			_panel4.SetActive(false);
			break;
		case 4:
			_panel1.SetActive(false);
			_panel2.SetActive(false);
			_panel3.SetActive(false);
			_panel4.SetActive(true);
			break;
		default:
			EB.Debug.LogError("invalid panel!");
			break;
		}
	}

	private void OnGoPanel2()
	{
		ShowPanel(2);
	}

	private void OnGoPanel3()
	{
		ShowPanel(3);
	}

	private void OnGoPanel1()
	{
		ShowPanel(1);
	}

	public void ShowNameSelect()
	{
		ShowPanel(4);
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "Button_Skip");
		objectExactMatch.SetActive(false);
	}

	private void OnSelect()
	{
		Error = string.Empty;
		Hub.Instance.LoginManager.SelectName(Name);
	}

	private void OnLogin()
	{
		Error = string.Empty;
		EB.Debug.LogWarning("Email " + Email + ", Password " + Password);
		Hub.Instance.LoginManager.LoginEmail(Email, Password);
	}

	private void OnSignup()
	{
		Error = string.Empty;
		Hub.Instance.LoginManager.CreateAccount(Email, Password, Password2, null);
	}

	private void OnSkip()
	{
		if (_canSkip)
		{
			Error = string.Empty;
			Hub.Instance.LoginManager.LoginGuest();
		}
	}
}
