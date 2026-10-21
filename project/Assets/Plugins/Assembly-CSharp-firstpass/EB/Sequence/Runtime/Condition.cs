namespace EB.Sequence.Runtime
{
	public abstract class Condition : Operation
	{
		[Property]
		public bool AutoCheck = true;

		public override void OnInputChanged()
		{
			if (AutoCheck)
			{
				Activate();
			}
		}

		public override void Init()
		{
			base.Init();
			if (AutoCheck)
			{
				Activate();
			}
		}

		[Entry]
		public void Check()
		{
			Activate();
		}
	}
}
