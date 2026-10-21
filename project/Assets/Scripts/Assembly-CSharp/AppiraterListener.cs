using EB.Sparx;

public class AppiraterListener : EB.Sparx.AppiraterListener
{
	public void OnDisplay()
	{
		Telemetry.Event("rate_display");
	}

	public void OnRated()
	{
		Telemetry.Event("rate_rated");
		if (!string.IsNullOrEmpty(Tuning.Instance.RateInventoryObject))
		{
			Hub.Instance.InventoryManager.AddItem(Tuning.Instance.RateInventoryObject, Tuning.Instance.RateInventoryCount);
		}
	}

	public void OnDeclined()
	{
		Telemetry.Event("rate_declined");
	}

	public void OnRemind()
	{
		Telemetry.Event("rate_remind");
	}
}
