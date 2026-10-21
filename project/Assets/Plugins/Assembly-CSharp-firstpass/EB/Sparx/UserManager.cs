using System.Collections;
using System.Collections.Generic;

namespace EB.Sparx
{
	public class UserManager : Manager
	{
		private Dictionary<Id, User> _users = new Dictionary<Id, User>();

		public User this[Id id]
		{
			get
			{
				User value;
				_users.TryGetValue(id, out value);
				return value;
			}
		}

		public override void Initialize(Config config)
		{
		}

		public override IEnumerator LoadOfflineData()
		{
			_users.Clear();
			yield break;
		}

		public User GetUser(Hashtable data)
		{
			Id id = new Id(Dot.Find("uid", data));
			if (id.Valid)
			{
				User value;
				if (!_users.TryGetValue(id, out value))
				{
					value = new User(id);
					_users[value.Id] = value;
				}
				value.Update(data);
				return value;
			}
			return null;
		}
	}
}
