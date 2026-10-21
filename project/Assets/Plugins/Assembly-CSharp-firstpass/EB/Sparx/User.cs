using System.Collections;

namespace EB.Sparx
{
	public class User
	{
		public Id Id { get; private set; }

		public string Name { get; private set; }

		public bool HasName
		{
			get
			{
				return !string.IsNullOrEmpty(Name);
			}
		}

		public string Email { get; private set; }

		public bool HasEmail
		{
			get
			{
				return !string.IsNullOrEmpty(Email);
			}
		}

		public string FacebookId { get; private set; }

		public bool HasFacebookId
		{
			get
			{
				return !string.IsNullOrEmpty(FacebookId);
			}
		}

		public bool IsGuest
		{
			get
			{
				return !HasEmail;
			}
		}

		public string GameCenterId { get; private set; }

		public int CohortDate { get; set; }

		public int Revenue { get; set; }

		public User(Id id)
		{
			Id = id;
			Name = string.Empty;
			GameCenterId = string.Empty;
		}

		public void Update(object data)
		{
			Name = Dot.String("name", data, Name);
			Email = Dot.String("email", data, string.Empty);
			GameCenterId = Dot.String("gcid", data, GameCenterId);
			FacebookId = Dot.String("fbid", data, FacebookId);
			CohortDate = Dot.Integer("cohort", data, CohortDate);
			Revenue = Dot.Integer("revenue", data, Revenue);
		}

		private Hashtable ToHashtable()
		{
			Hashtable hashtable = new Hashtable();
			hashtable["uid"] = Id;
			hashtable["name"] = Name;
			return hashtable;
		}

		public void Load(string key)
		{
			Update(SecurePrefs.GetJSON(key));
		}

		public void Save(string key)
		{
			SecurePrefs.SetJSON(key, ToHashtable());
		}
	}
}
