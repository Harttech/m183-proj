namespace Forum.Business.Helpers
{
	public static class Patterns
	{
		public static string EmailPattern { get; } = @"^.+@.+\..+$";
		public static string PhonePattern { get; } = @"^(\+|[00])\d+$";
		public static string PassPattern { get; } = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@@#$%^&*_=+-]).{8,12}$";
	}
}
