namespace Forum.Business.Helpers
{
	public static class Extensions
	{
		public static string RemoveAccent(this string txt)
		{
			byte[] bytes = System.Text.Encoding.GetEncoding("Cyrillic").GetBytes(txt);
			return System.Text.Encoding.ASCII.GetString(bytes);
		}

		public static string Slugify(this string phrase)
		{
			string str = phrase.RemoveAccent().ToLower();
			str = System.Text.RegularExpressions.Regex.Replace(str, @"[^a-z0-9\s-]", ""); // Remove all non valid chars          
			str = System.Text.RegularExpressions.Regex.Replace(str, @"\s+", " ").Trim(); // convert multiple spaces into one space  
			str = System.Text.RegularExpressions.Regex.Replace(str, @"\s", "-"); // //Replace spaces by dashes
			str = System.Text.RegularExpressions.Regex.Replace(str, @"\-+", "- "); // Convert multiple hyphens into one hyphen
			return str;
		}

		public static bool NullOrWhiteSpace(this string source)
		{
			return string.IsNullOrWhiteSpace(source);
		}

		public static bool HasText(this string source)
		{
			return !source.NullOrWhiteSpace();
		}
    }
}
