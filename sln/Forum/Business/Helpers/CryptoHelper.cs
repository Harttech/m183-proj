namespace Forum.Business.Helpers
{
	public static class CryptoHelper
	{
		private const int Cost = 13;

		public static string GenerateRandomSalt()
		{
			return BCrypt.Net.BCrypt.GenerateSalt(Cost);
		}

		public static string HashString(string toHash, string salt)
		{
			if (string.IsNullOrWhiteSpace(salt))
			{
				return BCrypt.Net.BCrypt.HashPassword(toHash, Cost);
			}
			else
			{
				return BCrypt.Net.BCrypt.HashPassword(toHash, salt);
			}
		}

		public static bool VerifyHash(string plain, string hash)
		{
			return BCrypt.Net.BCrypt.Verify(plain, hash);
		}

		public static string ExtractSalt(string hash)
		{
			var index = 0;
			for (int i = 0; i < 3; i++)
			{
				index = hash.IndexOf('$', index) + 1;
			}

			if (index == -1)
				return null;

			var length = index + 22;
			if (hash.Length < length)
				return null;

			return hash.Substring(0, length);
		}
	}
}
