using EB.Sequence;
using EB.Sequence.Runtime;

[MenuItem(Path = "Conditions/Gate")]
public class SequenceCondition_Gate : Condition
{
	[Property]
	public bool StartOpen;

	[Property]
	public bool CloseImmediate;

	private bool _isOpen;

	[Trigger]
	public Trigger Out = new Trigger();

	[Entry]
	public void Open()
	{
		_isOpen = true;
	}

	[Entry]
	public void Close()
	{
		_isOpen = false;
	}

	public override void Init()
	{
		if (StartOpen)
		{
			_isOpen = true;
		}
		base.Init();
	}

	public override bool Update()
	{
		if (_isOpen)
		{
			Out.Invoke();
			if (CloseImmediate)
			{
				_isOpen = false;
			}
		}
		return false;
	}
}
