namespace EB.Sequence.Runtime
{
	public class Variable : Node
	{
		public const string ValueLinkName = "Value";

		public static Variable Null = new Variable();

		public bool IsNull
		{
			get
			{
				return base.Id == 0;
			}
		}

		public virtual object Value
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public T GetValue<T>()
		{
			object value = Value;
			if (value != null && value is T)
			{
				return (T)value;
			}
			return default(T);
		}

		public override void GlobalValueChanged()
		{
			ValueChanged();
		}

		public void ValueChanged()
		{
			base.Parent.VariableChanged(this);
		}
	}
}
