using EB.Sequence;
using EB.Sequence.Runtime;

[MenuItem(Path = "Actions/Select Player Weapon")]
public class SequenceAction_SelectPlayerWeapon : Action
{
	[Property]
	public string weaponName;

	[Trigger]
	public Trigger Finished = new Trigger();

	[Entry]
	public void Process()
	{
		Player player = LevelData.Instance.GetPlayer();
		int weaponIndex = Arsenal.GetWeaponIndex(weaponName);
		player.SelectWeapon(weaponIndex);
		Finished.Invoke();
	}
}
