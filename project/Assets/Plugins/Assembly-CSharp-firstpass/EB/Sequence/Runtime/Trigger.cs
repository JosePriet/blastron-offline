namespace EB.Sequence.Runtime
{
	public class Trigger
	{
		public event EB.Action trigger;

		public bool IsValid()
		{
			if (trigger != null)
			{
				return true;
			}
			return false;
		}

		public void Invoke()
		{
			if (trigger != null)
			{
				trigger();
			}
		}
	}
}
