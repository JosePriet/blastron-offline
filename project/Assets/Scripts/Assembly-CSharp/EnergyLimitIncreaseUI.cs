using SmoothMoves;

public class EnergyLimitIncreaseUI : PopupScript
{
	public CharacterPrefab _mickE;

	public CacDisplay _display;

	private BoneAnimation _smAnim;

	private void Start()
	{
		_display.Recipe = _mickE.ToRecipe();
		_display.SpawnPlayer(() =>
		{
			_smAnim = _display.gameObject.GetComponentInChildren<BoneAnimation>();
		});
		SinglePlayerUI.UserInputToggle();
		base.OnStart();
	}

	protected override void OnClose()
	{
		SinglePlayerUI.UserInputToggle();
		base.OnClose();
	}

	private void Update()
	{
		if (_smAnim != null && !_smAnim.IsPlaying("bot_idle_mick-E"))
		{
			_smAnim.Play("bot_idle_mick-E");
		}
	}
}
