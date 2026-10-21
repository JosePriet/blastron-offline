namespace EB.Sparx
{
	public class FacebookUser
	{
		public enum ImageType
		{
			square = 0,
			small = 1,
			normal = 2,
			large = 3
		}

		public Id UserId { get; private set; }

		public string FacebookId { get; private set; }

		public string FirstName { get; private set; }

		public string LastName { get; private set; }

		public bool IsAppUser { get; private set; }

		public string GameCenterId { get; private set; }

		public FacebookUser(string fbId)
		{
			FacebookId = fbId;
		}

		public void Update(object data)
		{
			UserId = new Id(Dot.Find("uid", data));
			FirstName = Dot.String("first_name", data, FirstName);
			LastName = Dot.String("last_name", data, LastName);
			IsAppUser = Dot.Bool("is_app_user", data, IsAppUser);
		}

		public string GetImageUrl(ImageType type)
		{
			return "https://graph.facebook.com/" + FacebookId + "/picture?return_ssl_resources=1&type=" + type;
		}

		public static string GetImageUrlForUser(string fbId, ImageType type)
		{
			return "https://graph.facebook.com/" + fbId + "/picture?return_ssl_resources=1&type=" + type;
		}

		public static string GetImageUrlForUser(string fbId, int width, int height)
		{
			return "https://graph.facebook.com/" + fbId + "/picture?return_ssl_resources=1&width=" + width + "&height=" + height;
		}
	}
}
