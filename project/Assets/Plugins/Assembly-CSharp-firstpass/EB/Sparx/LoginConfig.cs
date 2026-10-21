using System;

namespace EB.Sparx
{
	public class LoginConfig
	{
		public LoginListener Listener;

		public bool RequireName;

		public Type LoginManagerType = typeof(LoginManagerSparx);
	}
}
