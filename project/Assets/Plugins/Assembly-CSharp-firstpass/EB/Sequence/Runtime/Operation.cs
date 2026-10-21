namespace EB.Sequence.Runtime
{
	public abstract class Operation : Node
	{
		public bool IsActive { get; set; }

		public virtual bool Activate()
		{
			if (!IsActive)
			{
				IsActive = true;
				base.Parent.AddToUpdate(this);
				return true;
			}
			return false;
		}

		public virtual bool Update()
		{
			return false;
		}
	}
}
