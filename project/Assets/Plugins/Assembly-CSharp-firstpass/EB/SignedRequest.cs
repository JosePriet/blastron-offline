namespace EB
{
	public static class SignedRequest
	{
		public static byte[] Parse(string contents, Hmac hmac)
		{
			string[] array = contents.Split('.');
			if (array.Length != 2)
			{
				return null;
			}
			string text = array[0];
			string text2 = array[1];
			hmac.Update(Encoding.GetBytes(text2));
			string text3 = Encoding.ToBase64Url(hmac.Final());
			if (text3 != text)
			{
				return null;
			}
			return Encoding.BytesFromBase64Url(text2);
		}

		public static string Stringify(byte[] payload, Hmac hmac)
		{
			string text = Encoding.ToBase64Url(payload);
			hmac.Update(Encoding.GetBytes(text));
			byte[] data = hmac.Final();
			return Encoding.ToBase64Url(data) + "." + text;
		}
	}
}
