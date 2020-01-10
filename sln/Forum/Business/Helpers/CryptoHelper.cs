namespace Forum.Business.Helpers
{
	public static class CryptoHelper
	{
		private const int Cost = 15;

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
	}
}
