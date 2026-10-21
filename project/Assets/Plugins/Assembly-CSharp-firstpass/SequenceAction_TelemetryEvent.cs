using EB.Sequence;
using EB.Sequence.Runtime;

[MenuItem(Path = "Actions/Telemetry Event")]
public class SequenceAction_TelemetryEvent : Action
{
	[Property]
	public string name = string.Empty;

	[Property]
	public string category = "tut";

	[Property]
	public int value = 1;

	[Property]
	public bool flush = true;

	[Entry]
	public void Record()
	{
		string text = name;
		if (!string.IsNullOrEmpty(category))
		{
			text = category + "/" + name;
		}
		Telemetry.Event(text, value);
		if (flush)
		{
			Telemetry.Flush();
		}
	}
}
