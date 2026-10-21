namespace EB.Sparx
{
	public abstract class SubSystem : Manager
	{
		public SubSystemState State { get; set; }

		public abstract void Connect();

		public abstract void Disconnect(bool isLogout);

		protected void FatalError(string error)
		{
			Debug.LogError("SubSystem Error: {0}", error);
			State = SubSystemState.Error;
			base.Hub.FatalError(error);
		}
	}
}
